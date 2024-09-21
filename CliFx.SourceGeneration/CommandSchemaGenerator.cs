using System.Linq;
using CliFx.SourceGeneration.SemanticModel;
using CliFx.SourceGeneration.Utils.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CliFx.SourceGeneration;

[Generator]
public class CommandSchemaGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var values = context.SyntaxProvider.ForAttributeWithMetadataName<(
            CommandSymbol?,
            Diagnostic?
        )>(
            KnownSymbolNames.CliFxCommandAttribute,
            (n, _) => n is TypeDeclarationSyntax,
            (x, _) =>
            {
                // Predicate above ensures that these casts are safe
                var commandTypeSyntax = (TypeDeclarationSyntax)x.TargetNode;
                var commandTypeSymbol = (INamedTypeSymbol)x.TargetSymbol;

                // Check if the target type and all its containing types are partial
                if (
                    commandTypeSyntax
                        .AncestorsAndSelf()
                        .Any(a =>
                            a is TypeDeclarationSyntax t
                            && !t.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword))
                        )
                )
                {
                    return (
                        null,
                        Diagnostic.Create(
                            DiagnosticDescriptors.CommandMustBePartial,
                            commandTypeSyntax.Identifier.GetLocation()
                        )
                    );
                }

                // Check if the target type implements ICommand
                var hasCommandInterface = commandTypeSymbol.AllInterfaces.Any(i =>
                    i.DisplayNameMatches(KnownSymbolNames.CliFxCommandInterface)
                );

                if (!hasCommandInterface)
                {
                    return (
                        null,
                        Diagnostic.Create(
                            DiagnosticDescriptors.CommandMustImplementInterface,
                            commandTypeSymbol.Locations.First()
                        )
                    );
                }

                // Resolve the command
                var commandAttribute = x.Attributes.First(a =>
                    a.AttributeClass?.DisplayNameMatches(KnownSymbolNames.CliFxCommandAttribute)
                    == true
                );

                var command = CommandSymbol.FromSymbol(commandTypeSymbol, commandAttribute);

                // TODO: validate command

                return (command, null);
            }
        );

        // Report diagnostics
        var diagnostics = values.Select((v, _) => v.Item2).WhereNotNull();
        context.RegisterSourceOutput(diagnostics, (x, d) => x.ReportDiagnostic(d));

        // Generate command schemas
        var symbols = values.Select((v, _) => v.Item1).WhereNotNull();
        context.RegisterSourceOutput(
            symbols,
            (x, c) =>
                x.AddSource(
                    $"{c.Type.FullyQualifiedName}.CommandSchema.Generated.cs",
                    // lang=csharp
                    $$"""
                    namespace {{c.Type.Namespace}};

                    partial class {{c.Type.Name}}
                    {
                        public static CliFx.Schema.CommandSchema<{{c.Type.FullyQualifiedName}}> Schema { get; } = {{c.GenerateSchemaInitializationCode()}};
                    }
                    """
                )
        );

        // Generate extension methods
        var symbolsCollected = symbols.Collect();
        context.RegisterSourceOutput(
            symbolsCollected,
            (x, cs) =>
                x.AddSource(
                    "CommandSchemaExtensions.Generated.cs",
                    // lang=csharp
                    $$"""
                  namespace CliFx;

                  static partial class GeneratedExtensions
                  {
                      public static CliFx.CliApplicationBuilder AddCommandsFromThisAssembly(this CliFx.CliApplicationBuilder builder)
                      {
                          {{
                              cs.Select(c => c.Type.FullyQualifiedName)
                                  .Select(t =>
                                      // lang=csharp
                                      $"builder.AddCommand({t}.Schema);"
                                  )
                                  .JoinToString("\n")
                          }}
                          
                          return builder;
                      }
                  }
                  """
                )
        );
    }
}
