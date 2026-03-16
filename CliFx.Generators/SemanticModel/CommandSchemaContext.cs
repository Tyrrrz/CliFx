using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace CliFx.Generators.SemanticModel;

/// <summary>
/// Carries all contextual data for a single command's schema generation pass:
/// the CliFx type references resolved from the compilation, the built command
/// descriptor (including accumulated diagnostics), and the ready-to-emit source
/// text with its hint name.  An instance is created as a discrete step in the
/// Roslyn incremental pipeline and flushed into a <see cref="SourceProductionContext"/>
/// at the end of the pipeline.
/// </summary>
internal sealed class CommandSchemaContext(
    CliFxReferences refs,
    CommandDescriptor descriptor,
    string hintName,
    string source
)
{
    /// <summary>All well-known CliFx types resolved from the current compilation.</summary>
    public CliFxReferences Refs { get; } = refs;

    /// <summary>The semantic descriptor built from the <c>[Command]</c> class symbol.</summary>
    public CommandDescriptor Descriptor { get; } = descriptor;

    /// <summary>The hint name under which the generated source will be registered.</summary>
    public string HintName { get; } = hintName;

    /// <summary>The fully generated C# source text for this command's partial class.</summary>
    public string Source { get; } = source;

    /// <summary>
    /// Generates the source text for <paramref name="descriptor"/> via
    /// <see cref="CommandSchemaEmitter"/> and returns a fully populated context
    /// ready to be flushed into a <see cref="SourceProductionContext"/>.
    /// </summary>
    public static CommandSchemaContext Create(CommandDescriptor descriptor, CliFxReferences refs)
    {
        var source = new CommandSchemaEmitter(refs).GenerateSource(descriptor);
        var hintName = $"{descriptor.Type.FullyQualifiedName.Replace('.', '_')}_Schema.g.cs";
        return new CommandSchemaContext(refs, descriptor, hintName, source);
    }

    /// <summary>
    /// Reports all accumulated diagnostics and adds the generated source file
    /// to the <paramref name="ctx"/>.
    /// </summary>
    public void FlushTo(SourceProductionContext ctx)
    {
        foreach (var diagnostic in Descriptor.Diagnostics)
            ctx.ReportDiagnostic(diagnostic);

        ctx.AddSource(HintName, SourceText.From(Source, Encoding.UTF8));
    }
}
