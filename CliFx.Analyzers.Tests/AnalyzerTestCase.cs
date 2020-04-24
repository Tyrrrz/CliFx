using Microsoft.CodeAnalysis;

namespace CliFx.Analyzers.Tests
{
    public class AnalyzerTestCase
    {
        public string Name { get; }

        public DiagnosticDescriptor TestedDiagnostic { get; }

        public string SourceCode { get; }

        public AnalyzerTestCase(string name, DiagnosticDescriptor testedDiagnostic, string sourceCode)
        {
            Name = name;
            TestedDiagnostic = testedDiagnostic;
            SourceCode = sourceCode;
        }

        public override string ToString() => Name;
    }
}