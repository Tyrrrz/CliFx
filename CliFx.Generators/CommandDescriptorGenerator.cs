using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CliFx.Generators;

[Generator]
public class CommandDescriptorGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var knownSymbols = context.CompilationProvider.Select(
            static (compilation, _) => new KnownSymbols(compilation)
        );

        var commandNodes = context.SyntaxProvider.ForAttributeWithMetadataName(
            KnownSymbols.CommandAttributeMetadataName,
            static (node, _) => node is ClassDeclarationSyntax,
            static (ctx, _) =>
                (
                    ClassDeclaration: (ClassDeclarationSyntax)ctx.TargetNode,
                    Symbol: (INamedTypeSymbol)ctx.TargetSymbol
                )
        );

        var commandNodesWithKnownSymbols = commandNodes.Combine(knownSymbols);

        var commandResults = commandNodesWithKnownSymbols.Select(
            static (pair, _) =>
            {
                var (item, knownSymbols) = pair;
                return CommandDescriptorContext.Resolve(item.Symbol, knownSymbols);
            }
        );

        context.RegisterSourceOutput(
            commandResults,
            static (ctx, descriptorContext) => descriptorContext?.FlushTo(ctx)
        );
    }
}
