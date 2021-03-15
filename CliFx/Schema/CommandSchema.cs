using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Utils.Extensions;

namespace CliFx.Schema
{
    internal partial class CommandSchema
    {
        public Type Type { get; }

        public string? Name { get; }

        public bool IsDefault => string.IsNullOrWhiteSpace(Name);

        public string? Description { get; }

        public IReadOnlyList<ParameterSchema> Parameters { get; }

        public IReadOnlyList<OptionSchema> Options { get; }

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

        public IReadOnlyDictionary<MemberSchema, object?> GetValues(ICommand instance)
        {
            var result = new Dictionary<MemberSchema, object?>();

            foreach (var parameterSchema in Parameters)
            {
                // Skip implicit parameters
                if (parameterSchema.Property is null)
                    continue;

                var value = parameterSchema.Property.GetValue(instance);
                result[parameterSchema] = value;
            }

            foreach (var optionSchema in Options)
            {
                // Skip implicit parameters
                if (optionSchema.Property is null)
                    continue;

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

            var name = attribute?.Name;

            var implicitOptions = string.IsNullOrWhiteSpace(name)
                ? new[] {OptionSchema.HelpOption, OptionSchema.VersionOption}
                : new[] {OptionSchema.HelpOption};

            var parameters = type.GetProperties()
                .Select(ParameterSchema.TryResolve)
                .Where(p => p is not null)
                .ToArray();

            var options = type.GetProperties()
                .Select(OptionSchema.TryResolve)
                .Where(o => o is not null)
                .Concat(implicitOptions)
                .ToArray();

            return new CommandSchema(
                type,
                name,
                attribute?.Description,
                parameters!,
                options!
            );
        }

        public static CommandSchema Resolve(Type type) =>
            TryResolve(type) ?? throw CliFxException.InvalidCommandType(type);
    }
}