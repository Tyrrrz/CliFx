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
    // This class allows us to embed command definitions inside the
    // tests that use them, by compiling the source code at runtime.
    // It's a bit of hack but it helps achieve test collocation,
    // which is really important.
    // Maybe one day C# will allow us to declare local classes inside
    // methods and this won't be necessary.
    internal static class DynamicCommandBuilder
    {
        public static IReadOnlyList<Type> CompileMany(string sourceCode)
        {
            // Get default system namespaces
            var defaultSystemNamespaces = new[]
            {
                "System",
                "System.Collections.Generic",
                "System.Threading.Tasks"
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
            return generatedAssembly
                .GetTypes()
                .Where(t => t.IsAssignableTo(typeof(ICommand)))
                .ToArray();
        }

        public static Type Compile(string sourceCode) => CompileMany(sourceCode).Single();
    }
}