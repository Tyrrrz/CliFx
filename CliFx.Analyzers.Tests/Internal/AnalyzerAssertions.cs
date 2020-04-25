using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CliFx.Analyzers.Tests.Internal
{
    internal partial class AnalyzerAssertions : ReferenceTypeAssertions<DiagnosticAnalyzer, AnalyzerAssertions>
    {
        protected override string Identifier { get; } = "analyzer";

        public AnalyzerAssertions(DiagnosticAnalyzer analyzer)
            : base(analyzer)
        {
        }

        public void ProduceDiagnostics(
            IReadOnlyList<DiagnosticDescriptor> diagnostics,
            IReadOnlyList<string> sourceCodes)
        {
            var producedDiagnostics = GetProducedDiagnostics(Subject, sourceCodes);

            var expectedIds = diagnostics.Select(d => d.Id).Distinct().OrderBy(d => d).ToArray();
            var producedIds = producedDiagnostics.Select(d => d.Id).Distinct().OrderBy(d => d).ToArray();

            var result = expectedIds.SequenceEqual(producedIds);

            Execute.Assertion.ForCondition(result).FailWith($@"
Expected and produced diagnostics do not match.

Expected: {string.Join(", ", expectedIds)}
Produced: {(producedIds.Any() ? string.Join(", ", producedIds) : "<none>")}
".Trim());
        }

        public void ProduceDiagnostics(AnalyzerTestCase testCase) =>
            ProduceDiagnostics(testCase.TestedDiagnostics, testCase.SourceCodes);

        public void NotProduceDiagnostics(
            IReadOnlyList<DiagnosticDescriptor> diagnostics,
            IReadOnlyList<string> sourceCodes)
        {
            var producedDiagnostics = GetProducedDiagnostics(Subject, sourceCodes);

            var expectedIds = diagnostics.Select(d => d.Id).Distinct().OrderBy(d => d).ToArray();
            var producedIds = producedDiagnostics.Select(d => d.Id).Distinct().OrderBy(d => d).ToArray();

            var result = !expectedIds.Intersect(producedIds).Any();

            Execute.Assertion.ForCondition(result).FailWith($@"
Expected no produced diagnostics.

Produced: {string.Join(", ", producedIds)}
".Trim());
        }

        public void NotProduceDiagnostics(AnalyzerTestCase testCase) =>
            NotProduceDiagnostics(testCase.TestedDiagnostics, testCase.SourceCodes);
    }

    internal partial class AnalyzerAssertions
    {
        private static IReadOnlyList<MetadataReference> DefaultMetadataReferences { get; } = new[]
        {
            MetadataReference.CreateFromFile(Assembly.Load("netstandard, Version=2.1.0.0").Location),
            MetadataReference.CreateFromFile(Assembly.Load("System.Runtime, Version=4.2.2.0").Location),
            MetadataReference.CreateFromFile(Assembly.Load("System.Runtime.Extensions, Version=4.2.2.0").Location),
            MetadataReference.CreateFromFile(typeof(string).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ValueTask).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(CliApplication).Assembly.Location)
        };

        private static string WrapCodeWithUsingDirectives(string code)
        {
            var usingDirectives = new[]
            {
                "using System;",
                "using System.Threading.Tasks;",
                "using CliFx;",
                "using CliFx.Attributes;",
                "using CliFx.Exceptions;",
                "using CliFx.Utilities;"
            };

            return
                string.Join(Environment.NewLine, usingDirectives) +
                Environment.NewLine +
                code;
        }

        private static IReadOnlyList<Diagnostic> GetProducedDiagnostics(
            DiagnosticAnalyzer analyzer,
            IReadOnlyList<string> sourceCodes)
        {
            var compilationOptions = new CSharpCompilationOptions(OutputKind.ConsoleApplication);
            var wrappedSourceCodes = sourceCodes.Select(WrapCodeWithUsingDirectives).ToArray();

            return Analyze.GetDiagnostics(analyzer, wrappedSourceCodes, compilationOptions, DefaultMetadataReferences)
                .SelectMany(d => d)
                .ToArray();
        }
    }

    internal static class AnalyzerAssertionsExtensions
    {
        public static AnalyzerAssertions Should(this DiagnosticAnalyzer analyzer) => new AnalyzerAssertions(analyzer);
    }
}