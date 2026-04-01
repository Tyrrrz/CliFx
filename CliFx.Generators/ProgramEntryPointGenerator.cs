using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace CliFx.Generators;

[Generator]
public class ProgramEntryPointGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
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

        // Generate a Program entry point for executable projects that don't already have one
        context.RegisterSourceOutput(
            hasMainMethod.Combine(hasGlobalStatements).Combine(isConsoleApplication),
            static (ctx, pair) =>
            {
                var ((hasMainMethod, hasGlobalStatements), isConsoleApplication) = pair;
                if (isConsoleApplication && !hasMainMethod && !hasGlobalStatements)
                {
                    ctx.AddSource(
                        "Program.g.cs",
                        SourceText.From(ProgramEntryPointEmitter.Emit(), Encoding.UTF8)
                    );
                }
            }
        );
    }
}
