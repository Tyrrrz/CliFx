using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CliFx.Attributes;
using CliFx.Domain.Input;
using CliFx.Exceptions;
using CliFx.Internal.Extensions;

namespace CliFx.Domain
{
    /// <summary>
    /// Stores command schema.
    /// </summary>
    public partial class CommandSchema
    {
        /// <summary>
        /// Command name.
        /// If the name is not set, the command is treated as a default command, i.e. the one that gets executed when the user
        /// does not specify a command name in the arguments.
        /// All commands in an application must have different names. Likewise, only one command without a name is allowed.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Whether command is default.
        /// </summary>
        public string? Name { get; }

        /// <summary>
        /// Command description, which is used in help text.
        /// </summary>
        public bool IsDefault => string.IsNullOrWhiteSpace(Name);

        /// <summary>
        /// Command description, which is used in help text.
        /// </summary>
        public string? Description { get; }

        /// <summary>
        /// Command manual text, which is used in help text.
        /// </summary>
        public string? Manual { get; }

        /// <summary>
        /// Whether command can run only in interactive mode.
        /// </summary>
        public bool InteractiveModeOnly { get; }

        /// <summary>
        /// List of parameters.
        /// </summary>
        public IReadOnlyList<CommandParameterSchema> Parameters { get; }

        /// <summary>
        /// List of options.
        /// </summary>
        public IReadOnlyList<CommandOptionSchema> Options { get; }

        /// <summary>
        /// Whether help option is available for this command.
        /// </summary>
        public bool IsHelpOptionAvailable => Options.Contains(CommandOptionSchema.HelpOption);

        /// <summary>
        /// Whether version option is available for this command.
        /// </summary>
        public bool IsVersionOptionAvailable => Options.Contains(CommandOptionSchema.VersionOption);

        internal CommandSchema(Type type,
                               string? name,
                               string? description,
                               string? manual,
                               bool interactiveModeOnly,
                               IReadOnlyList<CommandParameterSchema> parameters,
                               IReadOnlyList<CommandOptionSchema> options)
        {
            Type = type;
            Name = name;
            Description = description;
            Parameters = parameters;
            Options = options;
            Manual = manual;
            InteractiveModeOnly = interactiveModeOnly;
        }

        /// <summary>
        /// Enumerates through parameters and options.
        /// </summary>
        public IEnumerable<CommandArgumentSchema> GetArguments()
        {
            foreach (var parameter in Parameters)
                yield return parameter;

            foreach (var option in Options)
                yield return option;
        }

        /// <summary>
        /// Returns dictionary of arguments and its values.
        /// </summary>
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
                var nonScalarValues = parameterInputs
                    .Skip(scalarParameters.Length)
                    .Select(p => p.Value)
                    .ToArray();

                // Parameters are required by default and so a non-scalar parameter must
                // be bound to at least one value
                if (!nonScalarValues.Any())
                    throw CliFxException.ParameterNotSet(nonScalarParameter);

                nonScalarParameter.BindOn(instance, nonScalarValues);
                remainingParameterInputs.Clear();
            }

            // Ensure all inputs were bound
            if (remainingParameterInputs.Any())
                throw CliFxException.UnrecognizedParametersProvided(remainingParameterInputs);
        }

        private void BindOptions(ICommand instance,
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
                    ? new[] { value }
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

        internal void Bind(ICommand instance,
                           CommandInput input,
                           IReadOnlyDictionary<string, string> environmentVariables)
        {
            BindParameters(instance, input.Parameters);
            BindOptions(instance, input.Options, environmentVariables);
        }

        internal string GetInternalDisplayString()
        {
            var buffer = new StringBuilder();

            // Type
            buffer.Append(Type.FullName);

            // Name
            buffer.Append(' ')
                  .Append('(')
                  .Append(IsDefault ? "<default command>" : $"'{Name}'")
                  .Append(')');

            return buffer.ToString();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return GetInternalDisplayString();
        }
    }

    public partial class CommandSchema
    {
        internal static bool IsCommandType(Type type)
        {
            return type.Implements(typeof(ICommand)) &&
                   type.IsDefined(typeof(CommandAttribute)) &&
                   !type.IsAbstract &&
                   !type.IsInterface;
        }

        internal static CommandSchema? TryResolve(Type type)
        {
            if (!IsCommandType(type))
                return null;

            CommandAttribute? attribute = type.GetCustomAttribute<CommandAttribute>();

            var name = attribute?.Name;

            var builtInOptions = string.IsNullOrWhiteSpace(name)
                ? new[] { CommandOptionSchema.HelpOption, CommandOptionSchema.VersionOption }
                : new[] { CommandOptionSchema.HelpOption };

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
                attribute?.Manual,
                attribute?.InteractiveModeOnly ?? false,
                parameters!,
                options!
            );
        }
    }
}