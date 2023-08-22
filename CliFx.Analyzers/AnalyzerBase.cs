using System.Collections.Immutable;
using CliFx.Analyzers.Utils.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CliFx.Analyzers;

public abstract class AnalyzerBase : DiagnosticAnalyzer
{
    public DiagnosticDescriptor SupportedDiagnostic { get; }

    public sealed override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }

    protected AnalyzerBase(
        string diagnosticTitle,
        string diagnosticMessage,
        DiagnosticSeverity diagnosticSeverity = DiagnosticSeverity.Error
    )
    {
        SupportedDiagnostic = new DiagnosticDescriptor(
            "CliFx_" + GetType().Name.TrimEnd("Analyzer"),
            diagnosticTitle,
            diagnosticMessage,
            "CliFx",
            diagnosticSeverity,
            true
        );

        SupportedDiagnostics = ImmutableArray.Create(SupportedDiagnostic);
    }

    protected Diagnostic CreateDiagnostic(Location location, params object?[]? messageArgs) =>
        Diagnostic.Create(SupportedDiagnostic, location, messageArgs);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
    }
}
