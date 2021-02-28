using System.Collections.Generic;
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
            var compilationOptions = new CSharpCompilationOptions(OutputKind.ConsoleApplication);

            // Wrap code with default using directive
            var wrappedSourceCode = $@"
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Utilities

{sourceCode}".Trim();

            // Infer metadata references
            var metadataReferences = MetadataReferences
                .Transitive(typeof(CliApplication).Assembly)
                .ToArray();

            return Analyze
                .GetDiagnostics(Subject, new[] {wrappedSourceCode}, compilationOptions, metadataReferences)
                .SelectMany(d => d)
                .ToArray();
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