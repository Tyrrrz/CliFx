using System;
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

                var commandDescription =
                    commandAttribute
                        .NamedArguments.FirstOrDefault(a =>
                            string.Equals(a.Key, "Description", StringComparison.Ordinal)
                        )
                        .Value.Value as string;

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

                        var isSequence = false; // TODO

                        var order = parameterAttribute.ConstructorArguments.First().Value as int?;

                        var isRequired =
                            parameterAttribute
                                .NamedArguments.FirstOrDefault(a =>
                                    string.Equals(a.Key, "IsRequired", StringComparison.Ordinal)
                                )
                                .Value.Value as bool?
                            ?? true;

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
                            new PropertyDescriptor(
                                new TypeDescriptor(
                                    p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                                ),
                                p.Name
                            ),
                            isSequence,
                            order,
                            isRequired,
                            name,
                            description,
                            converter
                                ?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                                ?.Pipe(n => new TypeDescriptor(n)),
                            validators
                                .Select(v =>
                                    v.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                                )
                                .Select(n => new TypeDescriptor(n))
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

                        var isSequence = false; // TODO

                        var name =
                            optionAttribute
                                .ConstructorArguments.Where(a =>
                                    a.Type?.SpecialType == SpecialType.System_String
                                )
                                .Select(a => a.Value)
                                .FirstOrDefault() as string;

                        var shortName =
                            optionAttribute
                                .ConstructorArguments.Where(a =>
                                    a.Type?.SpecialType == SpecialType.System_Char
                                )
                                .Select(a => a.Value)
                                .FirstOrDefault() as char?;

                        var environmentVariable =
                            optionAttribute
                                .NamedArguments.FirstOrDefault(a =>
                                    string.Equals(
                                        a.Key,
                                        "EnvironmentVariable",
                                        StringComparison.Ordinal
                                    )
                                )
                                .Value.Value as string;

                        var isRequired =
                            optionAttribute
                                .NamedArguments.Where(a => a.Key == "IsRequired")
                                .Select(a => a.Value.Value)
                                .FirstOrDefault() as bool?
                            ?? p.IsRequired;

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
                            new PropertyDescriptor(
                                new TypeDescriptor(
                                    p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                                ),
                                p.Name
                            ),
                            isSequence,
                            name,
                            shortName,
                            environmentVariable,
                            isRequired,
                            description,
                            converter
                                ?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                                ?.Pipe(n => new TypeDescriptor(n)),
                            validators
                                .Select(v =>
                                    v.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                                )
                                .Select(n => new TypeDescriptor(n))
                                .ToArray()
                        );
                    })
                    .WhereNotNull()
                    .ToArray();

                return (
                    new CommandSymbol(
                        new TypeDescriptor(
                            namedTypeSymbol.ToDisplayString(
                                SymbolDisplayFormat.FullyQualifiedFormat
                            )
                        ),
                        commandName,
                        commandDescription,
                        parameterSymbols.Concat<CommandInputSymbol>(optionSymbols).ToArray()
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
                  
                  namespace {{ c.Type.Namespace }};
                  
                  partial class {{ c.Type.Name }}
                  {
                      public static CommandSchema<{{ c.Type.FullyQualifiedName }}> Schema { get; } = new(
                          {{ c.Name }},
                          {{ c.Description }},
                          [
                              {{ c.Inputs.Select(i => i switch {
                                  CommandParameterSymbol parameter =>
                                  // lang=csharp
                                  $$"""
                                    new CommandParameterSchema<{{ c.Type.FullyQualifiedName }}, {{ i.Property.Type.FullyQualifiedName }}>(
                                        new PropertyBinding<{{ c.Type.FullyQualifiedName }}, {{ i.Property.Type.FullyQualifiedName }}>(
                                            obj => obj.{{ i.Property.Name }},
                                            (obj, value) => obj.{{ i.Property.Name }} = value
                                        ),
                                        p.Order,
                                        p.IsRequired,
                                        p.Name,
                                        p.Description,
                                        new {{ i.ConverterType.FullyQualifiedName }}(),
                                        [ 
                                            {{ i.ValidatorTypes.Select(v =>
                                                // lang=csharp
                                                $"new {v.FullyQualifiedName}()").JoinToString(",\n")
                                            }}
                                        ]
                                    )
                                    """,
                                  CommandOptionSymbol option => ""
                                  }).JoinToString(",\n")
                              }}
                          ]
                  }
                  """;

                x.AddSource($"{c.TypeName}.CommandSchema.Generated.cs", source);
            }
        );
    }
}
