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

        private void InjectParameters(ICommand command, IReadOnlyList<CommandUnboundArgumentInput> parameterInputs)
        {
            // All inputs must be bound
            var remainingParameterInputs = parameterInputs.ToList();

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
                    : throw CliFxException.ParameterNotSet(scalarParameter);

                scalarParameter.Inject(command, scalarParameterInput.Value);
                remainingParameterInputs.Remove(scalarParameterInput);
            }

            // Non-scalar parameter (only one is allowed)
            var nonScalarParameter = Parameters
                .OrderBy(p => p.Order)
                .FirstOrDefault(p => !p.IsScalar);

            if (nonScalarParameter != null)
            {
                var nonScalarParameterValues = parameterInputs.Skip(scalarParameters.Length).Select(i => i.Value).ToArray();

                nonScalarParameter.Inject(command, nonScalarParameterValues);
                remainingParameterInputs.Clear();
            }

            // Ensure all inputs were bound
            if (remainingParameterInputs.Any())
            {
                throw CliFxException.UnrecognizedParametersProvided(remainingParameterInputs);
            }
        }

        private void InjectOptions(
            ICommand command,
            IReadOnlyList<CommandOptionInput> optionInputs,
            IReadOnlyDictionary<string, string> environmentVariables)
        {
            // All inputs must be bound
            var remainingOptionInputs = optionInputs.ToList();

            // All required options must be set
            var unsetRequiredOptions = Options.Where(o => o.IsRequired).ToList();

            // Environment variables
            foreach (var (name, value) in environmentVariables)
            {
                var option = Options.FirstOrDefault(o => o.MatchesEnvironmentVariableName(name));

                if (option != null)
                {
                    var values = option.IsScalar
                        ? new[] {value}
                        : value.Split(Path.PathSeparator);

                    option.Inject(command, values);
                    unsetRequiredOptions.Remove(option);
                }
            }

            // TODO: refactor this part? I wrote this while sick
            // Direct input
            foreach (var option in Options)
            {
                var inputs = optionInputs
                    .Where(i => option.MatchesNameOrShortName(i.Alias))
                    .ToArray();

                if (inputs.Any())
                {
                    var inputValues = inputs.SelectMany(i => i.Values).ToArray();
                    option.Inject(command, inputValues);

                    foreach (var input in inputs)
                        remainingOptionInputs.Remove(input);

                    if (inputValues.Any())
                        unsetRequiredOptions.Remove(option);
                }
            }

            // Ensure all required options were set
            if (unsetRequiredOptions.Any())
            {
                throw CliFxException.RequiredOptionsNotSet(unsetRequiredOptions);
            }

            // Ensure all inputs were bound
            if (remainingOptionInputs.Any())
            {
                throw CliFxException.UnrecognizedOptionsProvided(remainingOptionInputs);
            }
        }

        public ICommand CreateInstance(
            IReadOnlyList<CommandUnboundArgumentInput> parameterInputs,
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
                parameters!,
                options!
            );
        }
    }

    internal partial class CommandSchema
    {
        public static CommandSchema StubDefaultCommand { get; } =
            new CommandSchema(null!, null, null, new CommandParameterSchema[0], new CommandOptionSchema[0]);
    }
}