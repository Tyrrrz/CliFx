using System;
using System.Linq;
using CliFx.Generators.SemanticModel;
using CliFx.Generators.Utils.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CliFx.Generators;

/// <summary>
/// Source generator that generates strongly-typed command schema registration code for CliFx commands.
/// </summary>
[Generator]
public class CommandSchemaGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var cliFxRefs = context.CompilationProvider.Select(
            static (compilation, _) => new CliFxReferences(compilation)
        );

        var commandNodes = context.SyntaxProvider.ForAttributeWithMetadataName(
            CliFxReferences.CommandAttributeMetadataName,
            static (node, _) => node is ClassDeclarationSyntax,
            static (ctx, _) =>
                (
                    ClassDeclaration: (ClassDeclarationSyntax)ctx.TargetNode,
                    Symbol: (INamedTypeSymbol)ctx.TargetSymbol
                )
        );

        var commandNodesWithRefs = commandNodes.Combine(cliFxRefs);

        // Emit diagnostics for [Command] classes that are not partial or don't implement ICommand
        var diagnostics = commandNodesWithRefs.SelectMany(
            static (pair, _) =>
            {
                var (item, refs) = pair;

                // Abstract classes are intentionally skipped — no diagnostic needed
                if (item.Symbol.IsAbstract)
                    return [];

                if (!item.ClassDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword))
                    return
                    [
                        Diagnostic.Create(
                            DiagnosticDescriptors.CommandMustBePartial,
                            item.ClassDeclaration.Identifier.GetLocation(),
                            item.Symbol.Name
                        ),
                    ];

                if (
                    !item.Symbol.AllInterfaces.Any(i =>
                        SymbolEqualityComparer.Default.Equals(i, refs.ICommand.Symbol)
                    )
                )
                    return
                    [
                        Diagnostic.Create(
                            DiagnosticDescriptors.CommandMustImplementICommand,
                            item.ClassDeclaration.Identifier.GetLocation(),
                            item.Symbol.Name
                        ),
                    ];

                return Array.Empty<Diagnostic>();
            }
        );
        context.RegisterSourceOutput(
            diagnostics,
            static (ctx, diagnostic) => ctx.ReportDiagnostic(diagnostic)
        );

        // Only process classes that are partial, implement ICommand, and are not abstract
        var commandDeclarations = commandNodesWithRefs
            .Select(
                static (pair, _) =>
                {
                    var (item, refs) = pair;
                    if (
                        !item.ClassDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword)
                        || item.Symbol.IsAbstract
                        || !item.Symbol.AllInterfaces.Any(i =>
                            SymbolEqualityComparer.Default.Equals(i, refs.ICommand.Symbol)
                        )
                    )
                        return null;

                    return new CommandDescriptorBuilder(refs).TryBuild(item.Symbol);
                }
            )
            .WhereNotNull();

        // Build one CommandSchemaContext per command in the pipeline, then flush each
        // individually.  Using per-item RegisterSourceOutput (instead of .Collect()) means
        // Roslyn only regenerates the file for the command that actually changed.
        var commandContexts = commandDeclarations
            .Combine(cliFxRefs)
            .Select(static (pair, _) => CommandSchemaContext.Create(pair.Left, pair.Right));

        context.RegisterSourceOutput(commandContexts, static (ctx, genCtx) => genCtx.FlushTo(ctx));
    }
}
