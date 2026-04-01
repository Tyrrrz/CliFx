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
public partial class Generator : IIncrementalGenerator
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

                    var source = EmitCommandDescriptor(
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
        var accessibleCommands = commands
            .Select(static (item, cancellationToken) => item.Command)
            .WhereNotNull()
            // Only generate for commands that will be accessible by the generated code
            .Where(
                static (command) => command.Type.GetActualAccessibility() >= Accessibility.Internal
            )
            .Collect();

        context.RegisterSourceOutput(
            accessibleCommands,
            static (ctx, commands) =>
            {
                ctx.AddSource(
                    "CommandRegistrations.g.cs",
                    SourceText.From(EmitCommandRegistrations(commands), Encoding.UTF8)
                );
            }
        );

        // Detect whether the compilation already contains an entry point.
        // Any method named 'Main' is treated conservatively as an entry point, even if its
        // signature doesn't match exactly. This avoids accidentally generating a duplicate
        // entry point in ambiguous cases.
        var hasMainMethod = context
            .SyntaxProvider.CreateSyntaxProvider(
                static (node, _) =>
                    node is MethodDeclarationSyntax method && method.Identifier.Text == "Main",
                static (ctx, _) => true
            )
            .Collect()
            .Select(static (items, _) => items.Length > 0);

        var hasGlobalStatements = context
            .SyntaxProvider.CreateSyntaxProvider(
                static (node, _) => node is GlobalStatementSyntax,
                static (ctx, _) => true
            )
            .Collect()
            .Select(static (items, _) => items.Length > 0);

        // Detect whether the compilation targets an executable output kind
        var isConsoleApplication = context.CompilationProvider.Select(
            static (compilation, _) =>
                compilation.Options.OutputKind == OutputKind.ConsoleApplication
                || compilation.Options.OutputKind == OutputKind.WindowsApplication
        );

        // Generate a Program entry point only for executable projects with commands but no entry point
        context.RegisterSourceOutput(
            accessibleCommands
                .Combine(hasMainMethod)
                .Combine(hasGlobalStatements)
                .Combine(isConsoleApplication),
            static (ctx, pair) =>
            {
                var (((commands, hasMainMethod), hasGlobalStatements), isConsoleApplication) = pair;
                if (
                    isConsoleApplication
                    && commands.Length > 0
                    && !hasMainMethod
                    && !hasGlobalStatements
                )
                {
                    ctx.AddSource(
                        "Program.g.cs",
                        SourceText.From(EmitProgramEntryPoint(), Encoding.UTF8)
                    );
                }
            }
        );
    }
}
