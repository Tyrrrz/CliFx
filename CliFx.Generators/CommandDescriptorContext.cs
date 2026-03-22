using System.Collections.Generic;
using System.Linq;
using System.Text;
using CliFx.Generators.Binding;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace CliFx.Generators;

internal partial class CommandDescriptorContext(
    string? hintName,
    string? source,
    IReadOnlyList<Diagnostic> diagnostics
)
{
    public CommandDescriptorContext(IReadOnlyList<Diagnostic> diagnostics)
        : this(null, null, diagnostics) { }

    public void FlushTo(SourceProductionContext sourceContext)
    {
        foreach (var diagnostic in diagnostics)
            sourceContext.ReportDiagnostic(diagnostic);

        if (!string.IsNullOrWhiteSpace(hintName) && !string.IsNullOrWhiteSpace(source))
            sourceContext.AddSource(hintName, SourceText.From(source, Encoding.UTF8));
    }
}

internal partial class CommandDescriptorContext
{
    public static CommandDescriptorContext? Resolve(
        INamedTypeSymbol commandTypeSymbol,
        KnownSymbols knownSymbols
    )
    {
        var commandSymbol = CommandSymbol.TryResolve(
            commandTypeSymbol,
            knownSymbols,
            out var commandDiagnostics
        );

        if (commandSymbol is null)
        {
            if (commandDiagnostics.Any())
                return new CommandDescriptorContext(commandDiagnostics);

            return null;
        }

        var hintName = commandSymbol.Type.FullyQualifiedName.Replace('.', '_') + "_Descriptor.g.cs";

        var source = new CommandDescriptorEmitter(knownSymbols).GenerateSource(
            commandSymbol,
            out var emitterDiagnostics
        );

        return new CommandDescriptorContext(
            hintName,
            source,
            commandDiagnostics.Concat(emitterDiagnostics).ToArray()
        );
    }
}
