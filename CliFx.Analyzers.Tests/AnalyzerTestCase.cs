using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace CliFx.Analyzers.Tests
{
    public class AnalyzerTestCase
    {
        public string Name { get; }

        public IReadOnlyList<DiagnosticDescriptor> TestedDiagnostics { get; }

        public IReadOnlyList<string> SourceCodes { get; }

        public AnalyzerTestCase(
            string name,
            IReadOnlyList<DiagnosticDescriptor> testedDiagnostics,
            IReadOnlyList<string> sourceCodes)
        {
            Name = name;
            TestedDiagnostics = testedDiagnostics;
            SourceCodes = sourceCodes;
        }

        public AnalyzerTestCase(
            string name,
            IReadOnlyList<DiagnosticDescriptor> testedDiagnostics,
            string sourceCode)
            : this(name, testedDiagnostics, new[] {sourceCode})
        {
        }

        public AnalyzerTestCase(
            string name,
            DiagnosticDescriptor testedDiagnostic,
            string sourceCode)
            : this(name, new[] {testedDiagnostic}, sourceCode)
        {
        }

        public override string ToString() => $"{Name} [{string.Join(", ", TestedDiagnostics.Select(d => d.Id))}]";
    }
}