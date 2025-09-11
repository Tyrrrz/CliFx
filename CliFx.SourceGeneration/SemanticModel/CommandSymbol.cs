using System;
using System.Collections.Generic;
using System.Linq;
using CliFx.SourceGeneration.Utils.Extensions;
using Microsoft.CodeAnalysis;

namespace CliFx.SourceGeneration.SemanticModel;

internal partial class CommandSymbol(
    TypeDescriptor type,
    string? name,
    string? description,
    IReadOnlyList<CommandInputSymbol> inputs
)
{
    public TypeDescriptor Type { get; } = type;

    public string? Name { get; } = name;

    public string? Description { get; } = description;

    public IReadOnlyList<CommandInputSymbol> Inputs { get; } = inputs;

    public IReadOnlyList<CommandParameterSymbol> Parameters =>
        Inputs.OfType<CommandParameterSymbol>().ToArray();

    public IReadOnlyList<CommandOptionSymbol> Options =>
        Inputs.OfType<CommandOptionSymbol>().ToArray();

    private string GeneratePropertyBindingInitializationCode(PropertyDescriptor property) =>
        // lang=csharp
        $$"""
            new CliFx.Schema.PropertyBinding<{{Type.FullyQualifiedName}}, {{property
                .Type
                .FullyQualifiedName}}>(
                (obj) => obj.{{property.Name}},
                (obj, value) => obj.{{property.Name}} = value
            )
            """;

    private string GenerateSchemaInitializationCode(CommandInputSymbol input) =>
        input switch
        {
            CommandParameterSymbol parameter =>
            // lang=csharp
            $$"""
                    new CliFx.Schema.CommandParameterSchema<{{Type.FullyQualifiedName}}, {{parameter
                        .Property
                        .Type
                        .FullyQualifiedName}}>(
                        {{GeneratePropertyBindingInitializationCode(parameter.Property)}},
                        {{parameter.IsSequence}},
                        {{parameter.Order}},
                        "{{parameter.Name}}",
                        {{parameter.IsRequired}},
                        "{{parameter.Description}}",
                        // TODO,
                        // TODO
                    );
                    """,
            CommandOptionSymbol option =>
            // lang=csharp
            $$"""
                    new CliFx.Schema.CommandOptionSchema<{{Type.FullyQualifiedName}}, {{option
                        .Property
                        .Type
                        .FullyQualifiedName}}>(
                        {{GeneratePropertyBindingInitializationCode(option.Property)}},
                        {{option.IsSequence}},
                        "{{option.Name}}",
                        '{{option.ShortName}}',
                        "{{option.EnvironmentVariable}}",
                        {{option.IsRequired}},
                        "{{option.Description}}",
                        // TODO,
                        // TODO
                    );
                    """,
            _ => throw new ArgumentOutOfRangeException(nameof(input), input, null),
        };

    public string GenerateSchemaInitializationCode() =>
            // lang=csharp
            $$"""
            new CliFx.Schema.CommandSchema<{{Type.FullyQualifiedName}}>(
                "{{Name}}",
                "{{Description}}",
                new CliFx.Schema.CommandInputSchema[]
                {
                    {{Inputs.Select(GenerateSchemaInitializationCode).JoinToString(",\n")}}
                }
            )
            """;
}

internal partial class CommandSymbol : IEquatable<CommandSymbol>
{
    public bool Equals(CommandSymbol? other)
    {
        if (ReferenceEquals(null, other))
            return false;
        if (ReferenceEquals(this, other))
            return true;

        return Type.Equals(other.Type)
            && Name == other.Name
            && Description == other.Description
            && Inputs.SequenceEqual(other.Inputs);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        if (obj.GetType() != GetType())
            return false;

        return Equals((CommandSymbol)obj);
    }

    public override int GetHashCode() => HashCode.Combine(Type, Name, Description, Inputs);
}

internal partial class CommandSymbol
{
    public static CommandSymbol FromSymbol(INamedTypeSymbol symbol, AttributeData attribute)
    {
        var inputs = new List<CommandInputSymbol>();
        foreach (var property in symbol.GetMembers().OfType<IPropertySymbol>())
        {
            var parameterAttribute = property
                .GetAttributes()
                .FirstOrDefault(a =>
                    a.AttributeClass?.Name == KnownSymbolNames.CliFxCommandParameterAttribute
                );

            if (parameterAttribute is not null)
            {
                inputs.Add(CommandParameterSymbol.FromSymbol(property, parameterAttribute));
                continue;
            }

            var optionAttribute = property
                .GetAttributes()
                .FirstOrDefault(a =>
                    a.AttributeClass?.Name == KnownSymbolNames.CliFxCommandOptionAttribute
                );

            if (optionAttribute is not null)
            {
                inputs.Add(CommandOptionSymbol.FromSymbol(property, optionAttribute));
                continue;
            }
        }

        return new CommandSymbol(
            TypeDescriptor.FromSymbol(symbol),
            attribute.ConstructorArguments.FirstOrDefault().Value as string,
            attribute.GetNamedArgumentValue("Description", default(string)),
            inputs
        );
    }
}
