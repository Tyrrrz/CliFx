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

        public bool IsHelpOptionAvailable => Options.Contains(CommandOptionSchema.HelpOption);

        public bool IsVersionOptionAvailable => Options.Contains(CommandOptionSchema.VersionOption);

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
            Parameters = parameters;
            Options = options;
        }

        public bool MatchesName(string? name) =>
            !string.IsNullOrWhiteSpace(Name)
                ? string.Equals(name, Name, StringComparison.OrdinalIgnoreCase)
                : string.IsNullOrWhiteSpace(name);

        public IEnumerable<CommandArgumentSchema> GetArguments()
        {
            foreach (var parameter in Parameters)
                yield return parameter;

            foreach (var option in Options)
                yield return option;
        }

        public IReadOnlyDictionary<CommandArgumentSchema, object?> GetArgumentValues(ICommand instance)
        {
            var result = new Dictionary<CommandArgumentSchema, object?>();

            foreach (var argument in GetArguments())
            {
                // Skip built-in arguments
                if (argument.Property == null)
                    continue;

                var value = argument.Property.GetValue(instance);
                result[argument] = value;
            }

            return result;
        }

        private void BindParameters(ICommand instance, IReadOnlyList<CommandParameterInput> parameterInputs)
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
                var parameter = scalarParameters[i];

                var scalarInput = i < parameterInputs.Count
                    ? parameterInputs[i]
                    : throw CliFxException.ParameterNotSet(parameter);

                parameter.BindOn(instance, scalarInput.Value);
                remainingParameterInputs.Remove(scalarInput);
            }

            // Non-scalar parameter (only one is allowed)
            var nonScalarParameter = Parameters
                .OrderBy(p => p.Order)
                .FirstOrDefault(p => !p.IsScalar);

            if (nonScalarParameter != null)
            {
                // TODO: Should it verify that at least one value is passed?
                var nonScalarValues = parameterInputs
                    .Skip(scalarParameters.Length)
                    .Select(p => p.Value)
                    .ToArray();

                nonScalarParameter.BindOn(instance, nonScalarValues);
                remainingParameterInputs.Clear();
            }

            // Ensure all inputs were bound
            if (remainingParameterInputs.Any())
                throw CliFxException.UnrecognizedParametersProvided(remainingParameterInputs);
        }

        private void BindOptions(
            ICommand instance,
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
                if (option == null)
                    continue;

                var values = option.IsScalar
                    ? new[] {value}
                    : value.Split(Path.PathSeparator);

                option.BindOn(instance, values);
                unsetRequiredOptions.Remove(option);
            }

            // Direct input
            foreach (var option in Options)
            {
                var inputs = optionInputs
                    .Where(i => option.MatchesNameOrShortName(i.Alias))
                    .ToArray();

                // Skip if the inputs weren't provided for this option
                if (!inputs.Any())
                    continue;

                var inputValues = inputs.SelectMany(i => i.Values).ToArray();
                option.BindOn(instance, inputValues);

                remainingOptionInputs.RemoveRange(inputs);

                // Required option implies that the value has to be set and also be non-empty
                if (inputValues.Any())
                    unsetRequiredOptions.Remove(option);
            }

            // Ensure all inputs were bound
            if (remainingOptionInputs.Any())
                throw CliFxException.UnrecognizedOptionsProvided(remainingOptionInputs);

            // Ensure all required options were set
            if (unsetRequiredOptions.Any())
                throw CliFxException.RequiredOptionsNotSet(unsetRequiredOptions);
        }

        public void Bind(
            ICommand instance,
            CommandInput input,
            IReadOnlyDictionary<string, string> environmentVariables)
        {
            BindParameters(instance, input.Parameters);
            BindOptions(instance, input.Options, environmentVariables);
        }

        public string GetInternalDisplayString()
        {
            var buffer = new StringBuilder();

            // Type
            buffer.Append(Type.FullName);

            // Name
            buffer
                .Append(' ')
                .Append('(')
                .Append(IsDefault
                    ? "<default command>"
                    : $"'{Name}'"
                )
                .Append(')');

            return buffer.ToString();
        }

        public override string ToString() => GetInternalDisplayString();
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

            var builtInOptions = string.IsNullOrWhiteSpace(name)
                ? new[] {CommandOptionSchema.HelpOption, CommandOptionSchema.VersionOption}
                : new[] {CommandOptionSchema.HelpOption};

            var parameters = type.GetProperties()
                .Select(CommandParameterSchema.TryResolve)
                .Where(p => p != null)
                .ToArray();

            var options = type.GetProperties()
                .Select(CommandOptionSchema.TryResolve)
                .Where(o => o != null)
                .Concat(builtInOptions)
                .ToArray();

            return new CommandSchema(
                type,
                name,
                attribute?.Description,
                parameters!,
                options!
            );
        }
    }
}