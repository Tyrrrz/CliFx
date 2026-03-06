using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Basic.Reference.Assemblies;
using CliFx.Schema;
using CliFx.SourceGeneration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace CliFx.Tests.Utils;

// This class uses Roslyn to compile commands dynamically.
// It allows us to collocate commands with tests more easily, which helps a lot when reasoning about them.
// Unfortunately, this comes at a cost of static typing, but this is still a worthwhile trade off.
// Maybe one day C# will allow declaring classes inside methods and doing this will no longer be necessary.
// Language proposal: https://github.com/dotnet/csharplang/discussions/130
internal static class DynamicCommandBuilder
{
    public static IReadOnlyList<CommandSchema> CompileMany(string sourceCode)
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
            .Distinct()
            .ToArray();

        // Append default imports to the source code
        var sourceCodeWithUsings =
            string.Join(Environment.NewLine, defaultSystemNamespaces.Select(n => $"using {n};"))
            + string.Join(Environment.NewLine, defaultCliFxNamespaces.Select(n => $"using {n};"))
            + Environment.NewLine
            + sourceCode;

        // Parse the source code
        var ast = SyntaxFactory.ParseSyntaxTree(
            SourceText.From(sourceCodeWithUsings),
            CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview)
        );

        // Compile the code to IL
        var compilation = CSharpCompilation.Create(
            "CliFxTests_DynamicAssembly_" + Guid.NewGuid(),
            [ast],
            Net100
                .References.All.Append(
                    MetadataReference.CreateFromFile(typeof(ICommand).Assembly.Location)
                )
                .Append(
                    MetadataReference.CreateFromFile(
                        typeof(DynamicCommandBuilder).Assembly.Location
                    )
                ),
            // DLL to avoid having to define the Main() method
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        // Run the source generator to produce Schema properties on [Command] classes
        CSharpGeneratorDriver
            .Create(
                [new CommandSchemaGenerator().AsSourceGenerator()],
                parseOptions: CSharpParseOptions.Default.WithLanguageVersion(
                    LanguageVersion.Preview
                )
            )
            .RunGeneratorsAndUpdateCompilation(compilation, out var updatedCompilation, out _);

        var compilationErrors = updatedCompilation
            .GetDiagnostics()
            .Where(d => d.Severity >= DiagnosticSeverity.Error)
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
        var emit = updatedCompilation.Emit(buffer);

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

        // Return schemas for all defined commands via the source-generated Schema property
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
                (CommandSchema)
                    t.GetProperty("Schema", BindingFlags.Public | BindingFlags.Static)!
                        .GetValue(null)!
            )
            .ToArray();
    }

    public static CommandSchema Compile(string sourceCode)
    {
        var commandSchemas = CompileMany(sourceCode);
        if (commandSchemas.Count > 1)
        {
            throw new InvalidOperationException(
                "There are more than one command definitions in the provided source code."
            );
        }

        return commandSchemas.Single();
    }
}
