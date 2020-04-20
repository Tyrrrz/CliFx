using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CliFx.Analyzers.Tests
{
    public abstract partial class AnalyzerTestsBase
    {
        protected void AssertCodeValid(DiagnosticAnalyzer analyzer, DiagnosticDescriptor diagnostic, string code) =>
            RoslynAssert.Valid(analyzer, diagnostic, new[] {WrapCodeWithUsingDirectives(code)},
                metadataReferences: DefaultMetadataReferences,
                suppressWarnings: new[] {"CS8019"});

        protected void AssertCodeInvalid(DiagnosticAnalyzer analyzer, DiagnosticDescriptor diagnostic, string code) =>
            RoslynAssert.Diagnostics(analyzer, ExpectedDiagnostic.Create(diagnostic), new[] {WrapCodeWithUsingDirectives(code)},
                metadataReferences: DefaultMetadataReferences,
                suppressWarnings: new[] {"CS8019"});
    }

    public partial class AnalyzerTestsBase
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

        private static string WrapCodeWithUsingDirectives(string code) => $@"
using System;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Utilities;

{code}".Trim();
    }
}