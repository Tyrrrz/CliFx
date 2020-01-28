using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Internal;

namespace CliFx.Domain
{
    internal partial class CommandSchema
    {
        public Type Type { get; }

        public string? Name { get; }

        public bool IsDefault => string.IsNullOrWhiteSpace(Name);

        public string? Description { get; }

        public IReadOnlyList<CommandParameterSchema> Parameters { get; }

        public IReadOnlyList<CommandOptionSchema> Options { get; }

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

        public bool MatchesName(string? name) => string.Equals(name, Name, StringComparison.OrdinalIgnoreCase);

        private void InjectParameters(ICommand command, IReadOnlyList<string> parameterInputs)
        {
            // Scalar parameters
            var scalarParameters = Parameters
                .OrderBy(p => p.Order)
                .TakeWhile(p => p.IsScalar)
                .ToArray();

            for (var i = 0; i < scalarParameters.Length; i++)
            {
                var scalarParameter = scalarParameters[i];

                var scalarParameterInput = i < parameterInputs.Count
                    ? parameterInputs[i]
                    : throw new CliFxException($"Missing value for parameter <{scalarParameter.DisplayName}>.");

                scalarParameter.Inject(command, scalarParameterInput);
            }

            // Non-scalar parameter (only one is allowed)
            var nonScalarParameter = Parameters
                .OrderBy(p => p.Order)
                .FirstOrDefault(p => !p.IsScalar);

            if (nonScalarParameter != null)
            {
                var nonScalarParameterInputs = parameterInputs.Skip(scalarParameters.Length).ToArray();
                nonScalarParameter.Inject(command, nonScalarParameterInputs);
            }
        }

        private void InjectOptions(
            ICommand command,
            IReadOnlyList<CommandOptionInput> optionInputs,
            IReadOnlyDictionary<string, string> environmentVariables)
        {
            // Keep track of required options so that we can raise an error if any of them are not set
            var unsetRequiredOptions = Options.Where(o => o.IsRequired).ToList();

            // Environment variables
            foreach (var environmentVariable in environmentVariables)
            {
                var option = Options.FirstOrDefault(o => o.MatchesEnvironmentVariableName(environmentVariable.Key));

                if (option != null)
                {
                    var values = option.IsScalar
                        ? new[] {environmentVariable.Value}
                        : environmentVariable.Value.Split(Path.PathSeparator);

                    option.Inject(command, values);
                    unsetRequiredOptions.Remove(option);
                }
            }

            // Direct input
            foreach (var optionInput in optionInputs)
            {
                var option = Options.FirstOrDefault(o => o.MatchesNameOrShortName(optionInput.Alias));

                if (option != null)
                {
                    option.Inject(command, optionInput.Values);
                    unsetRequiredOptions.Remove(option);
                }
            }

            if (unsetRequiredOptions.Any())
            {
                throw new CliFxException(new StringBuilder()
                    .AppendLine("Missing values for some of the required options:")
                    .AppendBulletList(unsetRequiredOptions.Select(o => o.DisplayName))
                    .ToString());
            }
        }

        public ICommand CreateInstance(
            IReadOnlyList<string> parameterInputs,
            IReadOnlyList<CommandOptionInput> optionInputs,
            IReadOnlyDictionary<string, string> environmentVariables,
            ITypeActivator activator)
        {
            var command = (ICommand) activator.CreateInstance(Type);

            InjectParameters(command, parameterInputs);
            InjectOptions(command, optionInputs, environmentVariables);

            return command;
        }

        public override string ToString()
        {
            var buffer = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(Name))
                buffer.Append(Name);

            foreach (var parameter in Parameters)
            {
                buffer.AppendIfNotEmpty(' ');
                buffer.Append(parameter);
            }

            foreach (var option in Options)
            {
                buffer.AppendIfNotEmpty(' ');
                buffer.Append(option);
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
                attribute?.Name,
                attribute?.Description,
                parameters,
                options
            );
        }
    }

    internal partial class CommandSchema
    {
        public static CommandSchema StubDefaultCommand { get; } =
            new CommandSchema(null!, null, null, new CommandParameterSchema[0], new CommandOptionSchema[0]);
    }
}