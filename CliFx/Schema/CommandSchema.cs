using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Utils.Extensions;

namespace CliFx.Schema;

internal partial class CommandSchema(
    Type type,
    string? name,
    string? description,
    IReadOnlyList<ParameterSchema> parameters,
    IReadOnlyList<OptionSchema> options
)
{
    public Type Type { get; } = type;

    public string? Name { get; } = name;

    public string? Description { get; } = description;

    public IReadOnlyList<ParameterSchema> Parameters { get; } = parameters;

    public IReadOnlyList<OptionSchema> Options { get; } = options;

    public bool IsDefault => string.IsNullOrWhiteSpace(Name);

    public bool IsImplicitHelpOptionAvailable => Options.Contains(OptionSchema.ImplicitHelpOption);

    public bool IsImplicitVersionOptionAvailable =>
        Options.Contains(OptionSchema.ImplicitVersionOption);

    public bool MatchesName(string? name) =>
        !string.IsNullOrWhiteSpace(Name)
            ? string.Equals(name, Name, StringComparison.OrdinalIgnoreCase)
            : string.IsNullOrWhiteSpace(name);

    public IReadOnlyDictionary<IMemberSchema, object?> GetValues(ICommand instance)
    {
        var result = new Dictionary<IMemberSchema, object?>();

        foreach (var parameterSchema in Parameters)
        {
            var value = parameterSchema.Property.GetValue(instance);
            result[parameterSchema] = value;
        }

        foreach (var optionSchema in Options)
        {
            var value = optionSchema.Property.GetValue(instance);
            result[optionSchema] = value;
        }

        return result;
    }
}

internal partial class CommandSchema
{
    public static bool IsCommandType(Type type) =>
        type.Implements(typeof(ICommand))
        && type.IsDefined(typeof(CommandAttribute))
        && type is { IsAbstract: false, IsInterface: false };

    public static CommandSchema? TryResolve(Type type)
    {
        if (!IsCommandType(type))
            return null;

        var attribute = type.GetCustomAttribute<CommandAttribute>();

        var name = attribute?.Name?.Trim();
        var description = attribute?.Description?.Trim();

        var properties = type
        // Get properties directly on the command type
        .GetProperties()
            // Get non-abstract properties on interfaces (to support default interfaces members)
            .Union(
                type.GetInterfaces()
                    // Only interfaces implementing ICommand for explicitness
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
            .Select(ParameterSchema.TryResolve)
            .WhereNotNull()
            .ToArray();

        var optionSchemas = properties.Select(OptionSchema.TryResolve).WhereNotNull().ToList();

        // Include implicit options, if appropriate
        var isImplicitHelpOptionAvailable =
            // If the command implements its own help option, don't include the implicit one
            !optionSchemas.Any(o => o.MatchesShortName('h') || o.MatchesName("help"));

        if (isImplicitHelpOptionAvailable)
            optionSchemas.Add(OptionSchema.ImplicitHelpOption);

        var isImplicitVersionOptionAvailable =
            // Only the default command can have the version option
            string.IsNullOrWhiteSpace(name)
            &&
            // If the command implements its own version option, don't include the implicit one
            !optionSchemas.Any(o => o.MatchesName("version"));

        if (isImplicitVersionOptionAvailable)
            optionSchemas.Add(OptionSchema.ImplicitVersionOption);

        return new CommandSchema(type, name, description, parameterSchemas, optionSchemas);
    }

    public static CommandSchema Resolve(Type type)
    {
        var schema = TryResolve(type);
        if (schema is null)
        {
            throw CliFxException.InternalError(
                $"""
                Type `{type.FullName}` is not a valid command type.
                In order to be a valid command type, it must:
                - Implement `{typeof(ICommand).FullName}`
                - Be annotated with `{typeof(CommandAttribute).FullName}`
                - Not be an abstract class
                """
            );
        }

        return schema;
    }
}
