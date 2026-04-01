using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Basic.Reference.Assemblies;
using CliFx.Binding;
using CliFx.Generators;
using CliFx.Tests.Utils.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace CliFx.Tests.Utils;

internal static class CommandCompiler
{
    private static Compilation CreateCompilation(
        string sourceCode,
        OutputKind outputKind,
        out IReadOnlyList<Diagnostic> diagnostics
    )
    {
        // Get default system namespaces
        var defaultSystemNamespaces = new[]
        {
            "System",
            "System.Collections",
            "System.Collections.Generic",
            "System.Linq",
            "System.Threading.Tasks",
            "System.Globalization",
        };

        // Get default CliFx namespaces
        var defaultCliFxNamespaces = typeof(ICommand)
            .Assembly.GetTypes()
            .Where(t => t.IsPublic)
            .Select(t => t.Namespace)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        // Append default imports to the source code
        var sourceCodeWithUsings =
            string.Join(Environment.NewLine, defaultSystemNamespaces.Select(n => $"using {n};"))
            + string.Join(Environment.NewLine, defaultCliFxNamespaces.Select(n => $"using {n};"))
            + Environment.NewLine
            + sourceCode;

        // Parse the source code
        var syntaxTree = SyntaxFactory.ParseSyntaxTree(
            SourceText.From(sourceCodeWithUsings),
            CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview)
        );

        // Compile the code to IL
        var initialCompilation = CSharpCompilation.Create(
            "CliFxTests_DynamicAssembly_" + Guid.NewGuid(),
            [syntaxTree],
            Net100
                .References.All.Append(
                    MetadataReference.CreateFromFile(typeof(ICommand).Assembly.Location)
                )
                .Append(
                    MetadataReference.CreateFromFile(typeof(CommandCompiler).Assembly.Location)
                ),
            new CSharpCompilationOptions(outputKind)
        );

        CSharpGeneratorDriver
            .Create(
                [
                    new CommandDescriptorGenerator().AsSourceGenerator(),
                    new ProgramEntryPointGenerator().AsSourceGenerator(),
                ],
                parseOptions: CSharpParseOptions.Default.WithLanguageVersion(
                    LanguageVersion.Preview
                )
            )
            .RunGeneratorsAndUpdateCompilation(
                initialCompilation,
                out var updatedCompilation,
                out var generatorDiagnostics
            );

        diagnostics = updatedCompilation.GetDiagnostics().Concat(generatorDiagnostics).ToArray();

        return updatedCompilation;
    }

    public static IReadOnlyList<CommandDescriptor> Compile(
        string sourceCode,
        OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary,
        bool treatWarningsAsErrors = false
    )
    {
        var compilation = CreateCompilation(sourceCode, outputKind, out var diagnostics);

        var compilationErrors = diagnostics
            .Where(d =>
                d.Severity
                >= (treatWarningsAsErrors ? DiagnosticSeverity.Warning : DiagnosticSeverity.Error)
            )
            .ToArray();

        if (compilationErrors.Any())
        {
            throw new InvalidOperationException(
                $"""
                Failed to compile code.
                {string.Join(Environment.NewLine, compilationErrors.Select(e => e.ToString()))}
                """
            );
        }

        // Emit the code to an in-memory buffer
        using var buffer = new MemoryStream();
        var emit = compilation.Emit(buffer);

        var emitErrors = emit
            .Diagnostics.Where(d => d.Severity >= DiagnosticSeverity.Error)
            .ToArray();

        if (emitErrors.Any())
        {
            throw new InvalidOperationException(
                $"""
                Failed to emit code.
                {string.Join(Environment.NewLine, emitErrors.Select(e => e.ToString()))}
                """
            );
        }

        // Load the generated assembly
        var generatedAssembly = Assembly.Load(buffer.ToArray());

        // Return types for all defined commands
        var commandTypes = generatedAssembly
            .GetTypes()
            .Where(t => t.IsAssignableTo(typeof(ICommand)) && !t.IsAbstract)
            .ToArray();

        if (commandTypes.Length <= 0)
        {
            throw new InvalidOperationException(
                "There are no command definitions in the provided source code."
            );
        }

        return commandTypes
            .Select(t =>
                (CommandDescriptor?)
                    t.GetProperty("Descriptor", BindingFlags.Public | BindingFlags.Static)!
                        .GetValue(null)
            )
            .WhereNotNull()
            .ToArray();
    }
}
