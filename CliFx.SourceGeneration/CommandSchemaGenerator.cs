using System;
using System.Collections.Generic;
using System.Linq;
using CliFx.SourceGeneration.Utils.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CliFx.SourceGeneration;

[Generator]
public partial class CommandSchemaGenerator : IIncrementalGenerator
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
                // Predicate ensures that these casts are safe
                var typeDeclarationSyntax = (TypeDeclarationSyntax)x.TargetNode;
                var namedTypeSymbol = (INamedTypeSymbol)x.TargetSymbol;

                // Check if the target type and all its containing types are partial
                if (
                    typeDeclarationSyntax
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
                            typeDeclarationSyntax.Identifier.GetLocation()
                        )
                    );
                }

                // Check if the target type implements ICommand
                var hasCommandInterface = namedTypeSymbol.AllInterfaces.Any(i =>
                    i.DisplayNameMatches("CliFx.ICommand")
                );

                if (!hasCommandInterface)
                {
                    return (
                        null,
                        Diagnostic.Create(
                            DiagnosticDescriptors.CommandMustImplementInterface,
                            namedTypeSymbol.Locations.First()
                        )
                    );
                }

                // Get the command name
                var commandAttribute = x.Attributes.First(a =>
                    a.AttributeClass?.DisplayNameMatches(KnownSymbolNames.CliFxCommandAttribute)
                    == true
                );

                var commandName =
                    commandAttribute.ConstructorArguments.FirstOrDefault().Value as string;

                // Get all parameter inputs
                var parameterSymbols = namedTypeSymbol
                    .GetMembers()
                    .OfType<IPropertySymbol>()
                    .Select(p =>
                    {
                        var parameterAttribute = p.GetAttributes()
                            .FirstOrDefault(a =>
                                a.AttributeClass?.DisplayNameMatches(
                                    KnownSymbolNames.CliFxCommandParameterAttribute
                                ) == true
                            );

                        if (parameterAttribute is null)
                            return null;

                        var order =
                            parameterAttribute.ConstructorArguments.FirstOrDefault().Value as int?;

                        var isRequired =
                            parameterAttribute
                                .NamedArguments.FirstOrDefault(a =>
                                    string.Equals(a.Key, "IsRequired", StringComparison.Ordinal)
                                )
                                .Value.Value as bool?
                            ?? p.IsRequired;

                        var name =
                            parameterAttribute
                                .NamedArguments.FirstOrDefault(a =>
                                    string.Equals(a.Key, "Name", StringComparison.Ordinal)
                                )
                                .Value.Value as string;

                        var description =
                            parameterAttribute
                                .NamedArguments.FirstOrDefault(a =>
                                    string.Equals(a.Key, "Description", StringComparison.Ordinal)
                                )
                                .Value.Value as string;

                        var converter =
                            parameterAttribute
                                .NamedArguments.FirstOrDefault(a =>
                                    string.Equals(a.Key, "Converter", StringComparison.Ordinal)
                                )
                                .Value.Value as ITypeSymbol;

                        var validators = parameterAttribute
                            .NamedArguments.FirstOrDefault(a =>
                                string.Equals(a.Key, "Validators", StringComparison.Ordinal)
                            )
                            .Value.Values.CastArray<ITypeSymbol>();

                        return new CommandParameterSymbol(
                            new PropertyInfo(
                                p.Name,
                                p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                            ),
                            order,
                            isRequired,
                            name,
                            description,
                            converter?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                            validators
                                .Select(v =>
                                    v.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                                )
                                .ToArray()
                        );
                    })
                    .WhereNotNull()
                    .ToArray();

                // Get all option inputs
                var optionSymbols = namedTypeSymbol
                    .GetMembers()
                    .OfType<IPropertySymbol>()
                    .Select(p =>
                    {
                        var optionAttribute = p.GetAttributes()
                            .FirstOrDefault(a =>
                                a.AttributeClass?.DisplayNameMatches(
                                    KnownSymbolNames.CliFxCommandOptionAttribute
                                ) == true
                            );

                        if (optionAttribute is null)
                            return null;

                        var names =
                            optionAttribute.ConstructorArguments.FirstOrDefault().Value as string[];

                        var description =
                            optionAttribute
                                .NamedArguments.FirstOrDefault(a =>
                                    string.Equals(a.Key, "Description", StringComparison.Ordinal)
                                )
                                .Value.Value as string;

                        var converter =
                            optionAttribute
                                .NamedArguments.FirstOrDefault(a =>
                                    string.Equals(a.Key, "Converter", StringComparison.Ordinal)
                                )
                                .Value.Value as ITypeSymbol;

                        var validators = optionAttribute
                            .NamedArguments.FirstOrDefault(a =>
                                string.Equals(a.Key, "Validators", StringComparison.Ordinal)
                            )
                            .Value.Values.CastArray<ITypeSymbol>();

                        return new CommandOptionSymbol(
                            new PropertyInfo(
                                p.Name,
                                p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                            ),
                            names,
                            description,
                            converter?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                            validators
                                .Select(v =>
                                    v.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                                )
                                .ToArray()
                        );
                    })
                    .WhereNotNull()
                    .ToArray();

                return (
                    new CommandSymbol(
                        namedTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                        commandName,
                        parameterSymbols,
                        optionSymbols
                    ),
                    null
                );
            }
        );

        // Report diagnostics
        var diagnostics = values.Select((v, _) => v.Item2).WhereNotNull();
        context.RegisterSourceOutput(diagnostics, (x, d) => x.ReportDiagnostic(d));

        // Generate source
        var symbols = values.Select((v, _) => v.Item1).WhereNotNull();
        context.RegisterSourceOutput(
            symbols,
            (x, c) =>
            {
                var source =
                    // lang=csharp
                    $$"""
                  using System.Linq;
                  using CliFx.Schema;
                  using CliFx.Extensibility;
                  
                  partial class {{ c.TypeName }}
                  {
                      public static CommandSchema<{{ c.TypeName }}> Schema { get; } = new(
                          {{ c.Name }},
                          [
                              {{ c.Parameters.Select(p =>
                                  // lang=csharp
                                  $$"""
                                    new CommandParameterSchema<{{ c.TypeName }}, {{ p.Property.TypeName }}>(
                                        new PropertyBinding<{{ c.TypeName }}, {{ p.Property.TypeName }}>(
                                            obj => obj.{{ p.Property.Name }},
                                            (obj, value) => obj.{{ p.Property.Name }} = value
                                        ),
                                        p.Order,
                                        p.IsRequired,
                                        p.Name,
                                        p.Description,
                                        new {{ p.ConverterTypeName }}(),
                                        [ 
                                            {{ p.ValidatorTypeNames.Select(v =>
                                                // lang=csharp
                                                $"new {v}()").JoinToString(",\n")
                                            }}
                                        ]
                                    )
                                    """
                                  ).JoinToString(",\n")
                              }}
                          ]
                  }
                  """;

                x.AddSource($"{c.TypeName}.CommandSchema.Generated.cs", source);
            }
        );
    }
}

public partial class CommandSchemaGenerator
{
    // TODO make all types structurally equatable
    private record PropertyInfo(string Name, string TypeName);

    private record CommandParameterSymbol(
        PropertyInfo Property,
        int? Order,
        bool IsRequired,
        string? Name,
        string? Description,
        string? ConverterTypeName,
        IReadOnlyList<string> ValidatorTypeNames
    );

    private record CommandOptionSymbol(
        PropertyInfo Property,
        string[] Names,
        string? Description,
        string? ConverterTypeName,
        IReadOnlyList<string> ValidatorTypeNames
    );

    private record CommandSymbol(
        string TypeName,
        string? Name,
        IReadOnlyList<CommandParameterSymbol> Parameters,
        IReadOnlyList<CommandOptionSymbol> Options
    );
}
