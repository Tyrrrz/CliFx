using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using CliFx.Generators.Binding;
using CliFx.Generators.Utils;
using CliFx.Generators.Utils.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace CliFx.Generators;

[Generator]
public class Generator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var commands = context
            .SyntaxProvider.ForAttributeWithMetadataName(
                "CliFx.Binding.CommandAttribute",
                static (node, cancellationToken) => node is ClassDeclarationSyntax,
                static (ctx, cancellationToken) =>
                {
                    var classDeclaration = (ClassDeclarationSyntax)ctx.TargetNode;
                    var typeSymbol = (INamedTypeSymbol)ctx.TargetSymbol;

                    return (ClassDeclaration: classDeclaration, Symbol: typeSymbol);
                }
            )
            .Select(
                static (item, cancellationToken) =>
                {
                    var diagnostics = new List<Diagnostic>();

                    var command = CommandSymbol.TryResolve(
                        item.Symbol,
                        new DiagnosticReporter(diagnostics)
                    );

                    return (Command: command, Diagnostics: diagnostics.ToImmutableArray());
                }
            );

        // Generate command descriptors
        context.RegisterSourceOutput(
            commands.Select(
                static (item, cancellationToken) =>
                {
                    if (item.Command is null)
                    {
                        return (
                            HintName: null,
                            Source: null,
                            Diagnostics: item.Diagnostics.ToImmutableArray()
                        );
                    }

                    var hintName =
                        item.Command.Type.GetGloballyQualifiedName()
                            .Replace("global::", "")
                            .Replace('.', '_') + "_Descriptor.g.cs";

                    var emitterDiagnostics = new List<Diagnostic>();

                    var source = CommandDescriptorEmitter.Emit(
                        item.Command,
                        new DiagnosticReporter(emitterDiagnostics)
                    );

                    return (
                        HintName: hintName,
                        Source: source,
                        Diagnostics: item.Diagnostics.Concat(emitterDiagnostics).ToImmutableArray()
                    );
                }
            ),
            static (ctx, item) =>
            {
                foreach (var diagnostic in item.Diagnostics)
                {
                    ctx.ReportDiagnostic(diagnostic);
                }

                if (
                    !string.IsNullOrWhiteSpace(item.HintName)
                    && !string.IsNullOrWhiteSpace(item.Source)
                )
                {
                    ctx.AddSource(item.HintName, SourceText.From(item.Source, Encoding.UTF8));
                }
            }
        );

        // Generate the AddCommandsFromThisAssembly() extension method
        context.RegisterSourceOutput(
            commands
                .Select(static (item, cancellationToken) => item.Command)
                .WhereNotNull()
                // Only generate for commands that will be accessible by the generated code
                .Where(
                    static (command) =>
                        command.Type.GetActualAccessibility() >= Accessibility.Internal
                )
                .Collect(),
            static (ctx, commands) =>
            {
                ctx.AddSource(
                    "CommandRegistrations.g.cs",
                    SourceText.From(CommandRegistrationEmitter.Emit(commands), Encoding.UTF8)
                );
            }
        );
    }
}
