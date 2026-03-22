using System.Text;
using CliFx.Generators.Binding;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace CliFx.Generators;

/// <summary>
/// Carries all contextual data for a single command's descriptor generation pass:
/// the CliFx type references resolved from the compilation, the built command
/// descriptor (including accumulated diagnostics), and the ready-to-emit source
/// text with its hint name.  An instance is created as a discrete step in the
/// Roslyn incremental pipeline and flushed into a <see cref="SourceProductionContext"/>
/// at the end of the pipeline.
/// </summary>
internal sealed class CommandDescriptorContext(
    KnownSymbols knownSymbols,
    CommandSymbol command,
    string hintName,
    string source
)
{
    /// <summary>All well-known CliFx types resolved from the current compilation.</summary>
    public KnownSymbols KnownSymbols { get; } = knownSymbols;

    /// <summary>The command symbol built from the <c>[Command]</c> class symbol.</summary>
    public CommandSymbol Command { get; } = command;

    /// <summary>The hint name under which the generated source will be registered.</summary>
    public string HintName { get; } = hintName;

    /// <summary>The fully generated C# source text for this command's partial class.</summary>
    public string Source { get; } = source;

    /// <summary>
    /// Generates the source text for <paramref name="descriptor"/> via
    /// <see cref="CommandDescriptorEmitter"/> and returns a fully populated context
    /// ready to be flushed into a <see cref="SourceProductionContext"/>.
    /// </summary>
    public static CommandDescriptorContext Create(CommandSymbol command, KnownSymbols knownSymbols)
    {
        var source = new CommandDescriptorEmitter(knownSymbols).GenerateSource(command);
        var hintName = $"{command.Type.FullyQualifiedName.Replace('.', '_')}_Descriptor.g.cs";
        return new CommandDescriptorContext(knownSymbols, command, hintName, source);
    }

    /// <summary>
    /// Reports all accumulated diagnostics and adds the generated source file
    /// to the <paramref name="ctx"/>.
    /// </summary>
    public void FlushTo(SourceProductionContext ctx)
    {
        foreach (var diagnostic in Command.Diagnostics)
            ctx.ReportDiagnostic(diagnostic);

        ctx.AddSource(HintName, SourceText.From(Source, Encoding.UTF8));
    }
}
