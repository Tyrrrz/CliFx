using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Extensibility;
using CliFx.Utils.Extensions;

namespace CliFx.Schema;

/// <summary>
/// Describes the schema of a command.
/// </summary>
public partial class CommandSchema(
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        Type type,
    string? name,
    string? description,
    IReadOnlyList<CommandParameterSchema> parameters,
    IReadOnlyList<CommandOptionSchema> options
)
{
    /// <summary>
    /// CLR type of the command.
    /// </summary>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public Type Type { get; } = type;

    /// <summary>
    /// Command name. Null or empty for the default command.
    /// </summary>
    public string? Name { get; } = name;

    /// <summary>
    /// Command description shown in help text.
    /// </summary>
    public string? Description { get; } = description;

    /// <summary>
    /// Parameters of the command.
    /// </summary>
    public IReadOnlyList<CommandParameterSchema> Parameters { get; } = parameters;

    /// <summary>
    /// Options of the command, including implicit ones.
    /// </summary>
    public IReadOnlyList<CommandOptionSchema> Options { get; } = options;

    /// <summary>
    /// Whether this is the default command (no name).
    /// </summary>
    public bool IsDefault => string.IsNullOrWhiteSpace(Name);

    /// <summary>
    /// Whether the implicit --help option is available.
    /// </summary>
    public bool IsImplicitHelpOptionAvailable =>
        Options.Contains(CommandOptionSchema.ImplicitHelpOption);

    /// <summary>
    /// Whether the implicit --version option is available.
    /// </summary>
    public bool IsImplicitVersionOptionAvailable =>
        Options.Contains(CommandOptionSchema.ImplicitVersionOption);

    /// <summary>
    /// Whether this command matches the given name.
    /// </summary>
    public bool MatchesName(string? name) =>
        !string.IsNullOrWhiteSpace(Name)
            ? string.Equals(name, Name, StringComparison.OrdinalIgnoreCase)
            : string.IsNullOrWhiteSpace(name);

    internal IReadOnlyDictionary<CommandInputSchema, object?> GetValues(ICommand instance)
    {
        var result = new Dictionary<CommandInputSchema, object?>();

        foreach (var parameterSchema in Parameters)
        {
            var value = parameterSchema.Property.GetValue(instance);
            result[parameterSchema] = value;
        }

        foreach (var optionSchema in Options)
        {
            if (optionSchema.Property is NullPropertyBinding)
                continue;
            var value = optionSchema.Property.GetValue(instance);
            result[optionSchema] = value;
        }

        return result;
    }
}

public partial class CommandSchema
{
    /// <summary>
    /// Checks whether the type is a valid command type.
    /// </summary>
    [RequiresUnreferencedCode(
        "Uses reflection to inspect type interfaces and attributes. Not compatible with trimming."
    )]
    internal static bool IsCommandType(Type type) =>
        type.Implements(typeof(ICommand))
        && type.IsDefined(typeof(CommandAttribute))
        && type is { IsAbstract: false, IsInterface: false };

    /// <summary>
    /// Tries to resolve the command schema from a CLR type using reflection.
    /// Returns null if the type is not a valid command.
    /// </summary>
    [RequiresUnreferencedCode(
        "Uses reflection to resolve the command schema. Use the source-generated Schema property for AOT compatibility."
    )]
    internal static CommandSchema? TryResolve(Type type)
    {
        if (!IsCommandType(type))
            return null;

        var attribute = type.GetCustomAttribute<CommandAttribute>();

        var name = attribute?.Name?.Trim();
        var description = attribute?.Description?.Trim();

        var properties = type.GetProperties()
            .Union(
                type.GetInterfaces()
                    .Where(i => i != typeof(ICommand) && i.IsAssignableTo(typeof(ICommand)))
                    .SelectMany(i =>
                        i.GetProperties()
                            .Where(p =>
                                p.GetMethod is not null
                                && !p.GetMethod.IsAbstract
                                && p.SetMethod is not null
                                && !p.SetMethod.IsAbstract
                            )
                    )
            )
            .ToArray();

        var parameterSchemas = properties
            .Select(p => TryResolveParameter(p))
            .WhereNotNull()
            .ToArray();

        var optionSchemas = properties.Select(p => TryResolveOption(p)).WhereNotNull().ToList();

        var isImplicitHelpOptionAvailable = !optionSchemas.Any(o =>
            o.MatchesShortName('h') || o.MatchesName("help")
        );

        if (isImplicitHelpOptionAvailable)
            optionSchemas.Add(CommandOptionSchema.ImplicitHelpOption);

        var isImplicitVersionOptionAvailable =
            string.IsNullOrWhiteSpace(name) && !optionSchemas.Any(o => o.MatchesName("version"));

        if (isImplicitVersionOptionAvailable)
            optionSchemas.Add(CommandOptionSchema.ImplicitVersionOption);

        return new CommandSchema(type, name, description, parameterSchemas, optionSchemas);
    }

    [RequiresUnreferencedCode("Uses reflection to build property bindings.")]
    private static PropertyBinding CreatePropertyBinding(PropertyInfo property) =>
        new(
            property.PropertyType,
            instance => property.GetValue(instance),
            (instance, value) => property.SetValue(instance, value)
        );

    [RequiresUnreferencedCode("Uses Activator.CreateInstance to instantiate converters.")]
    private static IBindingConverter? CreateConverter(Type? converterType)
    {
        if (converterType is null)
            return null;

        if (!typeof(IBindingConverter).IsAssignableFrom(converterType))
        {
            throw CliFxException.InternalError(
                $"Type `{converterType.FullName}` does not implement `{typeof(IBindingConverter).FullName}`."
            );
        }

        return (IBindingConverter)Activator.CreateInstance(converterType)!;
    }

    [RequiresUnreferencedCode("Uses Activator.CreateInstance to instantiate validators.")]
    private static IReadOnlyList<IBindingValidator> CreateValidators(
        IReadOnlyList<Type> validatorTypes
    )
    {
        var validators = new IBindingValidator[validatorTypes.Count];
        for (var i = 0; i < validatorTypes.Count; i++)
        {
            var validatorType = validatorTypes[i];
            if (!typeof(IBindingValidator).IsAssignableFrom(validatorType))
            {
                throw CliFxException.InternalError(
                    $"Type `{validatorType.FullName}` does not implement `{typeof(IBindingValidator).FullName}`."
                );
            }

            validators[i] = (IBindingValidator)Activator.CreateInstance(validatorType)!;
        }

        return validators;
    }

    [RequiresUnreferencedCode("Uses reflection to resolve parameter schemas.")]
    private static CommandParameterSchema? TryResolveParameter(PropertyInfo property)
    {
        var attribute = property.GetCustomAttribute<CommandParameterAttribute>();
        if (attribute is null)
            return null;

        var name = attribute.Name?.Trim() ?? property.Name.ToLowerInvariant();
        var isRequired = attribute.IsRequired || property.IsRequired();
        var description = attribute.Description?.Trim();

        var isSequence =
            property.PropertyType != typeof(string)
            && property.PropertyType.TryGetEnumerableUnderlyingType() is not null;

        return new CommandParameterSchema(
            CreatePropertyBinding(property),
            isSequence,
            attribute.Order,
            name,
            isRequired,
            description,
            CreateConverter(attribute.Converter),
            CreateValidators(attribute.Validators)
        );
    }

    [RequiresUnreferencedCode("Uses reflection to resolve option schemas.")]
    private static CommandOptionSchema? TryResolveOption(PropertyInfo property)
    {
        var attribute = property.GetCustomAttribute<CommandOptionAttribute>();
        if (attribute is null)
            return null;

        var name = attribute.Name?.TrimStart('-').Trim();
        var environmentVariable = attribute.EnvironmentVariable?.Trim();
        var isRequired = attribute.IsRequired || property.IsRequired();
        var description = attribute.Description?.Trim();

        var isSequence =
            property.PropertyType != typeof(string)
            && property.PropertyType.TryGetEnumerableUnderlyingType() is not null;

        return new CommandOptionSchema(
            CreatePropertyBinding(property),
            isSequence,
            name,
            attribute.ShortName,
            environmentVariable,
            isRequired,
            description,
            CreateConverter(attribute.Converter),
            CreateValidators(attribute.Validators)
        );
    }
}

/// <inheritdoc cref="CommandSchema" />
/// <remarks>
/// Generic version used by source-generated code for static type references and AOT compatibility.
/// </remarks>
public class CommandSchema<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        TCommand
>(string? name, string? description, IReadOnlyList<CommandInputSchema> inputs)
    : CommandSchema(
        typeof(TCommand),
        name,
        description,
        inputs.OfType<CommandParameterSchema>().ToArray(),
        AddImplicitOptions(name, inputs.OfType<CommandOptionSchema>().ToList())
    )
    where TCommand : ICommand
{
    private static IReadOnlyList<CommandOptionSchema> AddImplicitOptions(
        string? commandName,
        List<CommandOptionSchema> options
    )
    {
        if (!options.Any(o => o.MatchesShortName('h') || o.MatchesName("help")))
            options.Add(CommandOptionSchema.ImplicitHelpOption);

        if (string.IsNullOrWhiteSpace(commandName) && !options.Any(o => o.MatchesName("version")))
            options.Add(CommandOptionSchema.ImplicitVersionOption);

        return options;
    }
}

// Internal null property binding used for implicit options (help, version)
internal sealed class NullPropertyBinding()
    : PropertyBinding(typeof(object), _ => null, (_, _) => { });
