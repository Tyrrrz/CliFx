using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Basic.Reference.Assemblies;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace CliFx.Analyzers.Tests.Utils;

internal class AnalyzerAssertions(DiagnosticAnalyzer analyzer)
    : ReferenceTypeAssertions<DiagnosticAnalyzer, AnalyzerAssertions>(analyzer)
{
    protected override string Identifier => "analyzer";

    private Compilation Compile(string sourceCode)
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
            Net80.References.All.Append(
                MetadataReference.CreateFromFile(typeof(ICommand).Assembly.Location)
            ),
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
                $"""
                Failed to compile code.
                {string.Join(Environment.NewLine, compilationErrors.Select(e => e.ToString()))}
                """
            );
        }

        return compilation;
    }

    private IReadOnlyList<Diagnostic> GetProducedDiagnostics(string sourceCode)
    {
        var analyzers = ImmutableArray.Create(Subject);
        var compilation = Compile(sourceCode);

        return compilation
            .WithAnalyzers(analyzers)
            .GetAnalyzerDiagnosticsAsync(analyzers, default)
            .GetAwaiter()
            .GetResult();
    }

    public void ProduceDiagnostics(string sourceCode)
    {
        var expectedDiagnostics = Subject.SupportedDiagnostics;
        var producedDiagnostics = GetProducedDiagnostics(sourceCode);

        var expectedDiagnosticIds = expectedDiagnostics.Select(d => d.Id).Distinct().ToArray();
        var producedDiagnosticIds = producedDiagnostics.Select(d => d.Id).Distinct().ToArray();

        var isSuccessfulAssertion =
            expectedDiagnosticIds.Intersect(producedDiagnosticIds).Count()
            == expectedDiagnosticIds.Length;

        Execute
            .Assertion.ForCondition(isSuccessfulAssertion)
            .FailWith(() =>
            {
                var buffer = new StringBuilder();

                buffer.AppendLine("Expected and produced diagnostics do not match.");
                buffer.AppendLine();

                buffer.AppendLine("Expected diagnostics:");

                foreach (var expectedDiagnostic in expectedDiagnostics)
                {
                    buffer.Append("  - ");
                    buffer.Append(expectedDiagnostic.Id);
                    buffer.AppendLine();
                }

                buffer.AppendLine();

                buffer.AppendLine("Produced diagnostics:");

                if (producedDiagnostics.Any())
                {
                    foreach (var producedDiagnostic in producedDiagnostics)
                    {
                        buffer.Append("  - ");
                        buffer.Append(producedDiagnostic);
                    }
                }
                else
                {
                    buffer.AppendLine("  < none >");
                }

                return new FailReason(buffer.ToString());
            });
    }

    public void NotProduceDiagnostics(string sourceCode)
    {
        var producedDiagnostics = GetProducedDiagnostics(sourceCode);
        var isSuccessfulAssertion = !producedDiagnostics.Any();

        Execute
            .Assertion.ForCondition(isSuccessfulAssertion)
            .FailWith(() =>
            {
                var buffer = new StringBuilder();

                buffer.AppendLine("Expected no produced diagnostics.");
                buffer.AppendLine();

                buffer.AppendLine("Produced diagnostics:");

                foreach (var producedDiagnostic in producedDiagnostics)
                {
                    buffer.Append("  - ");
                    buffer.Append(producedDiagnostic);
                }

                return new FailReason(buffer.ToString());
            });
    }
}

internal static class AnalyzerAssertionsExtensions
{
    public static AnalyzerAssertions Should(this DiagnosticAnalyzer analyzer) => new(analyzer);
}
