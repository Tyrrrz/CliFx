using System;
using System.Linq;
using CliFx.Generators.Binding;
using CliFx.Generators.Utils.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CliFx.Generators;

/// <summary>
/// Source generator that generates strongly-typed command schema registration code for CliFx commands.
/// </summary>
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

        // Collect and report diagnostics
        var diagnostics = commandNodesWithKnownSymbols.SelectMany(
            static (pair, _) =>
            {
                var (item, knownSymbols) = pair;

                if (item.Symbol.IsAbstract)
                    return [];

                // Must be partial to allow source generation to add members
                if (!item.ClassDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword))
                {
                    return
                    [
                        Diagnostic.Create(
                            DiagnosticDescriptors.CommandMustBePartial,
                            item.ClassDeclaration.Identifier.GetLocation(),
                            item.Symbol.Name
                        ),
                    ];
                }

                // Must implement ICommand
                if (
                    !item.Symbol.AllInterfaces.Any(i =>
                        SymbolEqualityComparer.Default.Equals(i, knownSymbols.ICommand.Symbol)
                    )
                )
                {
                    return
                    [
                        Diagnostic.Create(
                            DiagnosticDescriptors.CommandMustImplementICommand,
                            item.ClassDeclaration.Identifier.GetLocation(),
                            item.Symbol.Name
                        ),
                    ];
                }

                return Array.Empty<Diagnostic>();
            }
        );

        context.RegisterSourceOutput(
            diagnostics,
            static (ctx, diagnostic) => ctx.ReportDiagnostic(diagnostic)
        );

        // Only process classes that are partial, implement ICommand, and are not abstract
        var commandSymbols = commandNodesWithKnownSymbols
            .Select(
                static (pair, _) =>
                {
                    var (item, knownSymbols) = pair;
                    if (
                        !item.ClassDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword)
                        || item.Symbol.IsAbstract
                        || !item.Symbol.AllInterfaces.Any(i =>
                            SymbolEqualityComparer.Default.Equals(i, knownSymbols.ICommand.Symbol)
                        )
                    )
                        return null;

                    return new CommandSymbolBuilder(knownSymbols).TryBuild(item.Symbol);
                }
            )
            .WhereNotNull();

        // Build one CommandDescriptorContext per command in the pipeline, then flush each
        // individually.  Using per-item RegisterSourceOutput (instead of .Collect()) means
        // Roslyn only regenerates the file for the command that actually changed.
        var descriptorContexts = commandSymbols
            .Combine(knownSymbols)
            .Select(static (pair, _) => CommandDescriptorContext.Create(pair.Left, pair.Right));

        context.RegisterSourceOutput(
            descriptorContexts,
            static (ctx, descriptorCtx) => descriptorCtx.FlushTo(ctx)
        );
    }
}
