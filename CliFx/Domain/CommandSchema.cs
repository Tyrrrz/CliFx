using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using CliFx.Attributes;
using CliFx.Internal;

namespace CliFx.Domain
{
    internal partial class CommandSchema
    {
        public Type Type { get; }

        public string? Name { get; }

        public string? Description { get; }

        public IReadOnlyList<CommandParameterSchema> Parameters { get; }

        public IReadOnlyList<CommandOptionSchema> Options { get; }

        public bool IsDefault => string.IsNullOrWhiteSpace(Name);

        public CommandSchema(
            Type type,
            string? name,
            string? description,
            IReadOnlyList<CommandParameterSchema> parameters,
            IReadOnlyList<CommandOptionSchema> options)
        {
            Type = type;
            Name = name;
            Description = description;
            Options = options;
            Parameters = parameters;
        }

        public bool MatchesName(string name) => string.Equals(name, Name, StringComparison.OrdinalIgnoreCase);

        public void Project(
            ICommand target,
            IReadOnlyList<string> parameterInputs,
            IReadOnlyList<CommandOptionInput> optionInputs,
            IReadOnlyDictionary<string, string> environmentVariables)
        {
            // Scalar parameters
            var scalarParameters = Parameters
                .OrderBy(p => p.Order)
                .TakeWhile(p => p.IsScalar)
                .ToArray();

            for (var i = 0; i < scalarParameters.Length; i++)
                scalarParameters[i].Project(target, parameterInputs[i]);

            // Non-scalar parameter (only one is allowed)
            var nonScalarParameter = Parameters
                .OrderBy(p => p.Order)
                .FirstOrDefault(p => !p.IsScalar);

            var nonScalarParameterValues = parameterInputs.Skip(scalarParameters.Length).ToArray();
            nonScalarParameter?.Project(target, nonScalarParameterValues);

            // Options
            foreach (var option in Options)
            {
                var optionValues = optionInputs
                    .Where(o => option.MatchesNameOrShortName(o.Alias))
                    .SelectMany(o => o.Values)
                    .ToArray();

                // TODO: check required
                // TODO: env vars

                option.Project(target, optionValues);
            }
        }

        public override string ToString()
        {
            var buffer = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(Name))
                buffer.Append(Name);

            if (Options != null)
            {
                foreach (var option in Options)
                {
                    buffer.AppendIfNotEmpty(' ');
                    buffer.Append('[');
                    buffer.Append(option);
                    buffer.Append(']');
                }
            }
            return buffer.ToString();
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
            if (attribute == null)
                return null;

            var parameters = type.GetProperties()
                .Select(CommandParameterSchema.TryResolve)
                .Where(p => p != null)
                .ToArray();

            var options = type.GetProperties()
                .Select(CommandOptionSchema.TryResolve)
                .Where(o => o != null)
                .ToArray();

            return new CommandSchema(
                type,
                attribute.Name,
                attribute.Description,
                parameters,
                options
            );
        }
    }
}