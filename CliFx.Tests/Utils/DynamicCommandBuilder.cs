using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace CliFx.Tests.Utils
{
    internal static class DynamicCommandBuilder
    {
        public static Type Compile(string sourceCode)
        {
            var wrappedSourceCode = $@"
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx;
using CliFx.Infrastructure;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Extensibility;

{sourceCode}".Trim();

            var ast = SyntaxFactory.ParseSyntaxTree(
                SourceText.From(wrappedSourceCode),
                CSharpParseOptions.Default
            );

            var classDeclaration = ast
                .GetRoot()
                .DescendantNodesAndSelf()
                .OfType<ClassDeclarationSyntax>()
                .FirstOrDefault();

            if (classDeclaration is null)
            {
                throw new InvalidOperationException(
                    "Provided code does not contain a class declaration"
                );
            }

            var className = classDeclaration.Identifier.ValueText;

            var references = new[]
            {
                MetadataReference.CreateFromFile(Assembly.Load("netstandard").Location),
                MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(DynamicCommandBuilder).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(ICommand).Assembly.Location)
            };

            var compilation = CSharpCompilation.Create(
                "CliFxTests_DynamicAssembly_" + Guid.NewGuid(),
                new[] {ast},
                references,
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

            var generatedAssembly = Assembly.Load(buffer.ToArray());

            var generatedType =
                generatedAssembly.GetType(className) ??
                throw new InvalidOperationException("Cannot find generated type in the output assembly.");

            return generatedType;
        }
    }
}