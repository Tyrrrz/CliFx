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

    public bool IsHelpOptionAvailable => Options.Contains(OptionSchema.HelpOption);

    public bool IsVersionOptionAvailable => Options.Contains(OptionSchema.VersionOption);

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

        var implicitOptionSchemas = string.IsNullOrWhiteSpace(name)
            ? new[] { OptionSchema.HelpOption, OptionSchema.VersionOption }
            : new[] { OptionSchema.HelpOption };

        var properties = type
        // Get properties directly on the command type
        .GetProperties()
            // Get non-abstract properties on interfaces (to support default interfaces members)
            .Union(
                type.GetInterfaces()
                    // Only interfaces implementing ICommand for explicitness
                    .Where(i => i != typeof(ICommand) && i.IsAssignableTo(typeof(ICommand)))
                    .SelectMany(
                        i =>
                            i.GetProperties()
                                .Where(p => !p.GetMethod.IsAbstract && !p.SetMethod.IsAbstract)
                    )
            )
            .ToArray();

        var parameterSchemas = properties
            .Select(ParameterSchema.TryResolve)
            .WhereNotNull()
            .ToArray();

        var optionSchemas = properties
            .Select(OptionSchema.TryResolve)
            .WhereNotNull()
            .Concat(implicitOptionSchemas)
            .ToArray();

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
