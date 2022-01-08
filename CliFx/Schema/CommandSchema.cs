﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Utils.Extensions;

namespace CliFx.Schema;

internal partial class CommandSchema
{
    public Type Type { get; }

    public string? Name { get; }

    public string? Description { get; }

    public IReadOnlyList<ParameterSchema> Parameters { get; }

    public IReadOnlyList<OptionSchema> Options { get; }

    public bool IsDefault => string.IsNullOrWhiteSpace(Name);

    public bool IsHelpOptionAvailable => Options.Contains(OptionSchema.HelpOption);

    public bool IsVersionOptionAvailable => Options.Contains(OptionSchema.VersionOption);

    public CommandSchema(
        Type type,
        string? name,
        string? description,
        IReadOnlyList<ParameterSchema> parameters,
        IReadOnlyList<OptionSchema> options)
    {
        Type = type;
        Name = name;
        Description = description;
        Parameters = parameters;
        Options = options;
    }

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
        type.Implements(typeof(ICommand)) &&
        type.IsDefined(typeof(CommandAttribute)) &&
        !type.IsAbstract &&
        !type.IsInterface;

    public static CommandSchema? TryResolve(Type type)
    {
        if (!IsCommandType(type))
            return null;

        var attribute = type.GetCustomAttribute<CommandAttribute>();

        var name = attribute?.Name?.Trim();
        var description = attribute?.Description?.Trim();

        var implicitOptionSchemas = string.IsNullOrWhiteSpace(name)
            ? new[] {OptionSchema.HelpOption, OptionSchema.VersionOption}
            : new[] {OptionSchema.HelpOption};
        
        // Include interface members for multiple inheritance
        // If interface inherits from ICommand, it will be included
        var interfaces = type.GetInterfaces().Where(i => i != typeof(ICommand) && typeof(ICommand).IsAssignableFrom(i) );
        var properties = type.GetProperties().Concat(interfaces.SelectMany(i => i.GetProperties())).ToArray();
      
        var parameterSchemas = properties
            .Select(ParameterSchema.TryResolve)
            .WhereNotNull()
            .ToArray();

        var optionSchemas = properties
            .Select(OptionSchema.TryResolve)
            .WhereNotNull()
            .Concat(implicitOptionSchemas)
            .ToArray();

        return new CommandSchema(
            type,
            name,
            description,
            parameterSchemas,
            optionSchemas
        );
    }

    public static CommandSchema Resolve(Type type)
    {
        var schema = TryResolve(type);

        if (schema is null)
        {
            throw CliFxException.InternalError(
                $"Type `{type.FullName}` is not a valid command type." +
                Environment.NewLine +
                "In order to be a valid command type, it must:" +
                Environment.NewLine +
                $"- Implement `{typeof(ICommand).FullName}`" +
                Environment.NewLine +
                $"- Be annotated with `{typeof(CommandAttribute).FullName}`" +
                Environment.NewLine +
                "- Not be an abstract class"
            );
        }

        return schema;
    }
}