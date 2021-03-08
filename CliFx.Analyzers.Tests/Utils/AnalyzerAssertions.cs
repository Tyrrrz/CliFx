using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CliFx.Analyzers.Tests.Utils
{
    internal class AnalyzerAssertions : ReferenceTypeAssertions<DiagnosticAnalyzer, AnalyzerAssertions>
    {
        protected override string Identifier { get; } = "analyzer";

        public AnalyzerAssertions(DiagnosticAnalyzer analyzer)
            : base(analyzer)
        {
        }

        private IReadOnlyList<Diagnostic> GetProducedDiagnostics(string sourceCode)
        {
            var result = new List<Diagnostic>();

            var analyzers = ImmutableArray.Create(Subject);

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

            var metadataReferences = MetadataReferences
                .Transitive(typeof(CliApplication).Assembly)
                .ToArray();

            // Library to avoid having to define a static Main() method
            var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);

            var solution = CodeFactory.CreateSolution(wrappedSourceCode, compilationOptions, metadataReferences);

            foreach (var project in solution.Projects)
            {
                var compilation =
                    project.GetCompilationAsync().GetAwaiter().GetResult() ??
                    throw new InvalidOperationException("Failed to compile project.");

                // Ensure there are no compilation errors
                var compilationErrors = compilation
                    .GetDiagnostics()
                    .Where(diagnostic => diagnostic.Severity >= DiagnosticSeverity.Error)
                    .ToArray();

                foreach (var compilationError in compilationErrors)
                {
                    throw new InvalidOperationException(
                        "Failed to compile project:" + Environment.NewLine +
                        compilationError
                    );
                }

                // Get analyzer-specific diagnostics
                var analyzerDiagnostics = compilation
                    .WithAnalyzers(analyzers, project.AnalyzerOptions)
                    .GetAnalyzerDiagnosticsAsync(analyzers, default)
                    .GetAwaiter().GetResult();

                result.AddRange(analyzerDiagnostics);
            }

            return result;
        }

        public void ProduceDiagnostics(string sourceCode)
        {
            var producedDiagnostics = GetProducedDiagnostics(sourceCode);

            var expectedIds = Subject.SupportedDiagnostics.Select(d => d.Id).Distinct().OrderBy(d => d).ToArray();
            var producedIds = producedDiagnostics.Select(d => d.Id).Distinct().OrderBy(d => d).ToArray();

            var result = expectedIds.Intersect(producedIds).Count() == expectedIds.Length;

            Execute.Assertion.ForCondition(result).FailWith($@"
Expected and produced diagnostics do not match.

Expected: {string.Join(", ", expectedIds)}
Produced: {(producedIds.Any() ? string.Join(", ", producedIds) : "<none>")}
".Trim());
        }

        public void NotProduceDiagnostics(string sourceCode)
        {
            var producedDiagnostics = GetProducedDiagnostics(sourceCode);

            var expectedIds = Subject.SupportedDiagnostics.Select(d => d.Id).Distinct().OrderBy(d => d).ToArray();
            var producedIds = producedDiagnostics.Select(d => d.Id).Distinct().OrderBy(d => d).ToArray();

            var result = !expectedIds.Intersect(producedIds).Any();

            Execute.Assertion.ForCondition(result).FailWith($@"
Expected no produced diagnostics.

Produced: {string.Join(", ", producedIds)}
".Trim());
        }
    }

    internal static class AnalyzerAssertionsExtensions
    {
        public static AnalyzerAssertions Should(this DiagnosticAnalyzer analyzer) => new(analyzer);
    }
}