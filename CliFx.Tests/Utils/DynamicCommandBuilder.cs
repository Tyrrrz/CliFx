using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace CliFx.Tests.Utils
{
    // This class uses Roslyn to compile commands dynamically.
    // It allows us to collocate tests and their commands more
    // easily, which helps a lot when reasoning about them.
    // Unfortunately, this comes at a cost of static typing,
    // but this is a trade off I'm willing to take.
    // Maybe one day C# will allow declaring classes inside
    // methods and doing this will no longer be necessary.
    internal static class DynamicCommandBuilder
    {
        public static IReadOnlyList<Type> CompileMany(string sourceCode)
        {
            // Get default system namespaces
            var defaultSystemNamespaces = new[]
            {
                "System",
                "System.Collections.Generic",
                "System.Threading.Tasks",
                "System.Globalization"
            };

            // Get default CliFx namespaces
            var defaultCliFxNamespaces = typeof(ICommand)
                .Assembly
                .GetTypes()
                .Where(t => t.IsPublic)
                .Select(t => t.Namespace)
                .Distinct()
                .ToArray();

            // Append default imports to the source code
            var sourceCodeWithUsings =
                string.Join(Environment.NewLine, defaultSystemNamespaces.Select(n => $"using {n};")) +
                string.Join(Environment.NewLine, defaultCliFxNamespaces.Select(n => $"using {n};")) +
                Environment.NewLine +
                sourceCode;

            // Parse the source code
            var ast = SyntaxFactory.ParseSyntaxTree(
                SourceText.From(sourceCodeWithUsings),
                CSharpParseOptions.Default
            );

            // Compile the code to IL
            var compilation = CSharpCompilation.Create(
                "CliFxTests_DynamicAssembly_" + Guid.NewGuid(),
                new[] {ast},
                new[]
                {
                    MetadataReference.CreateFromFile(Assembly.Load("netstandard").Location),
                    MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(DynamicCommandBuilder).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(ICommand).Assembly.Location)
                },
                // DLL to avoid having to define the Main() method
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            );

            var compilationErrors = compilation
                .GetDiagnostics()
                .Where(d => d.Severity >= DiagnosticSeverity.Error)
                .ToArray();

            if (compilationErrors.Any())
            {
                throw new InvalidOperationException(
                    "Failed to compile code." +
                    Environment.NewLine +
                    string.Join(Environment.NewLine, compilationErrors.Select(e => e.ToString()))
                );
            }

            // Emit the code to an in-memory buffer
            using var buffer = new MemoryStream();
            var emit = compilation.Emit(buffer);

            var emitErrors = emit
                .Diagnostics
                .Where(d => d.Severity >= DiagnosticSeverity.Error)
                .ToArray();

            if (emitErrors.Any())
            {
                throw new InvalidOperationException(
                    "Failed to emit code." +
                    Environment.NewLine +
                    string.Join(Environment.NewLine, emitErrors.Select(e => e.ToString()))
                );
            }

            // Load the generated assembly
            var generatedAssembly = Assembly.Load(buffer.ToArray());

            // Return all defined commands
            var commandTypes = generatedAssembly
                .GetTypes()
                .Where(t => t.IsAssignableTo(typeof(ICommand)))
                .ToArray();

            if (commandTypes.Length <= 0)
            {
                throw new InvalidOperationException(
                    "There are no command definitions in the provide source code."
                );
            }

            return commandTypes;
        }

        public static Type Compile(string sourceCode)
        {
            var commandTypes = CompileMany(sourceCode);

            if (commandTypes.Count > 1)
            {
                throw new InvalidOperationException(
                    "There are more than one command definitions in the provide source code."
                );
            }

            return commandTypes.Single();
        }
    }
}