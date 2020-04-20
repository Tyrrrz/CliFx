using System;
using System.Collections.Generic;
using System.Linq;
using CliFx.Attributes;
using CliFx.Domain;

namespace CliFx.Exceptions
{
    /// <summary>
    /// Domain exception thrown within CliFx.
    /// </summary>
    public partial class CliFxException : Exception
    {
        /// <summary>
        /// Initializes an instance of <see cref="CliFxException"/>.
        /// </summary>
        public CliFxException(string? message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="CliFxException"/>.
        /// </summary>
        public CliFxException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }
    }

    // Mid-user-facing exceptions
    // Provide more diagnostic information here
    public partial class CliFxException
    {
        internal static CliFxException DefaultActivatorFailed(Type type, Exception? innerException = null)
        {
            var configureActivatorMethodName = $"{nameof(CliApplicationBuilder)}.{nameof(CliApplicationBuilder.UseTypeActivator)}(...)";

            var message = $@"
Failed to create an instance of type '{type.FullName}'.
The type must have a public parameterless constructor in order to be instantiated by the default activator.

To fix this, either make sure this type has a public parameterless constructor, or configure a custom activator using {configureActivatorMethodName}. 
Refer to the readme to learn how to integrate a dependency container of your choice to act as a type activator.";

            return new CliFxException(message.Trim(), innerException);
        }

        internal static CliFxException DelegateActivatorReceivedNull(Type type)
        {
            var message = $@"
Failed to create an instance of type '{type.FullName}', received <null> instead.

To fix this, ensure that the provided type activator was configured correctly, as it's not expected to return <null>.
If you are using a dependency container, ensure this type is registered, because it may return <null> otherwise.";

            return new CliFxException(message.Trim());
        }

        internal static CliFxException InvalidCommandType(Type type)
        {
            var message = $@"
Command '{type.FullName}' is not a valid command type.

In order to be a valid command type, it must:
- Not be an abstract class
- Implement {typeof(ICommand).FullName}
- Be annotated with {typeof(CommandAttribute).FullName}

To fix this, ensure that the command adheres to these constraints.
If you're experiencing problems, please refer to readme for a quickstart example.";

            return new CliFxException(message.Trim());
        }

        internal static CliFxException CommandsNotRegistered()
        {
            var message = $@"
There are no commands configured in the application.

To fix this, ensure that at least one command is added through one of the methods on {nameof(CliApplicationBuilder)}.
If you're experiencing problems, please refer to readme for a quickstart example.";

            return new CliFxException(message.Trim());
        }

        internal static CliFxException CommandsTooManyDefaults(
            IReadOnlyList<CommandSchema> invalidCommands)
        {
            var message = $@"
Application configuration is invalid because there are {invalidCommands.Count} default commands:
{string.Join(Environment.NewLine, invalidCommands.Select(p => p.Type.FullName))}

There can only be one default command (i.e. command with no name) in an application.
Other commands must have unique non-empty names that identify them.

To fix this, ensure that all extra commands have different names.";

            return new CliFxException(message.Trim());
        }

        internal static CliFxException CommandsDuplicateName(
            string name,
            IReadOnlyList<CommandSchema> invalidCommands)
        {
            var message = $@"
Application configuration is invalid because there are {invalidCommands.Count} commands with the same name ('{name}'):
{string.Join(Environment.NewLine, invalidCommands.Select(p => p.Type.FullName))}

Commands must have unique names, because that's what identifies them.
Names are not case-sensitive.

To fix this, ensure that all commands have different names.";

            return new CliFxException(message.Trim());
        }

        internal static CliFxException CommandParametersDuplicateOrder(
            CommandSchema command,
            int order,
            IReadOnlyList<CommandParameterSchema> invalidParameters)
        {
            var message = $@"
Command '{command.Type.FullName}' is invalid because it contains {invalidParameters.Count} parameters with the same order ({order}):
{string.Join(Environment.NewLine, invalidParameters.Select(p => p.Property.Name))}

Parameters must have unique order, because that's what identifies them.

To fix this, ensure that all parameters have different order.";

            return new CliFxException(message.Trim());
        }

        internal static CliFxException CommandParametersDuplicateName(
            CommandSchema command,
            string name,
            IReadOnlyList<CommandParameterSchema> invalidParameters)
        {
            var message = $@"
Command '{command.Type.FullName}' is invalid because it contains {invalidParameters.Count} parameters with the same name ('{name}'):
{string.Join(Environment.NewLine, invalidParameters.Select(p => p.Property.Name))}

Parameters must have unique names to avoid potential confusion in the help text.
Names are not case-sensitive.

To fix this, ensure that all parameters have different names.";

            return new CliFxException(message.Trim());
        }

        internal static CliFxException CommandParametersTooManyNonScalar(
            CommandSchema command,
            IReadOnlyList<CommandParameterSchema> invalidParameters)
        {
            var message = $@"
Command '{command.Type.FullName}' is invalid because it contains {invalidParameters.Count} non-scalar parameters:
{string.Join(Environment.NewLine, invalidParameters.Select(p => p.Property.Name))}

Non-scalar parameter is such that is bound from more than one value (e.g. array or a complex object).
Only one parameter in a command may be non-scalar and it must be the last one in order.

To fix this, ensure there's only a single non-scalar parameter.
If that's not possible, consider converting one or more of the parameters into options, to avoid this limitation.";

            return new CliFxException(message.Trim());
        }

        internal static CliFxException CommandParametersNonLastNonScalar(
            CommandSchema command,
            CommandParameterSchema invalidParameter)
        {
            var message = $@"
Command '{command.Type.FullName}' is invalid because it contains a non-scalar parameter which is not the last in order:
{invalidParameter.Property.Name}

Non-scalar parameter is such that is bound from more than one value (e.g. array or a complex object).
Only one parameter in a command may be non-scalar and it must be the last one in order.

To fix this, ensure that the non-scalar parameter is last in order.
If that's not possible, consider converting the parameter into an option, to avoid this limitation.";

            return new CliFxException(message.Trim());
        }

        internal static CliFxException CommandOptionsNoName(
            CommandSchema command,
            IReadOnlyList<CommandOptionSchema> invalidOptions)
        {
            var message = $@"
Command '{command.Type.FullName}' is invalid because it contains one or more options without a name:
{string.Join(Environment.NewLine, invalidOptions.Select(p => p.Property.Name))}

Options must have either a name or a short name or both, because that's what identifies them.

To fix this, ensure all options have their names or short names set to some values.";

            return new CliFxException(message.Trim());
        }

        internal static CliFxException CommandOptionsInvalidLengthName(
            CommandSchema command,
            IReadOnlyList<CommandOptionSchema> invalidOptions)
        {
            var message = $@"
Command '{command.Type.FullName}' is invalid because it contains one or more options whose names are too short:
{string.Join(Environment.NewLine, invalidOptions.Select(p => $"{p.Property.Name} ('{p.DisplayName}')"))}

Option names must be at least 2 characters long to avoid confusion with short names.
If you intended to set the short name instead, use the corresponding attribute overload.

To fix this, ensure all option names are at least 2 characters long.";

            return new CliFxException(message.Trim());
        }

        internal static CliFxException CommandOptionsDuplicateName(
            CommandSchema command,
            string name,
            IReadOnlyList<CommandOptionSchema> invalidOptions)
        {
            var message = $@"
Command '{command.Type.FullName}' is invalid because it contains {invalidOptions.Count} options with the same name ('{name}'):
{string.Join(Environment.NewLine, invalidOptions.Select(p => p.Property.Name))}

Options must have unique names, because that's what identifies them.
Names are not case-sensitive.

To fix this, ensure that all options have different names.";

            return new CliFxException(message.Trim());
        }

        internal static CliFxException CommandOptionsDuplicateShortName(
            CommandSchema command,
            char shortName,
            IReadOnlyList<CommandOptionSchema> invalidOptions)
        {
            var message = $@"
Command '{command.Type.FullName}' is invalid because it contains {invalidOptions.Count} options with the same short name ('{shortName}'):
{string.Join(Environment.NewLine, invalidOptions.Select(p => p.Property.Name))}

Options must have unique short names, because that's what identifies them.
Short names are case-sensitive (i.e. 'a' and 'A' are different short names).

To fix this, ensure that all options have different short names.";

            return new CliFxException(message.Trim());
        }

        internal static CliFxException CommandOptionsDuplicateEnvironmentVariableName(
            CommandSchema command,
            string environmentVariableName,
            IReadOnlyList<CommandOptionSchema> invalidOptions)
        {
            var message = $@"
Command '{command.Type.FullName}' is invalid because it contains {invalidOptions.Count} options with the same fallback environment variable name ('{environmentVariableName}'):
{string.Join(Environment.NewLine, invalidOptions.Select(p => p.Property.Name))}

Options cannot share the same environment variable as a fallback.
Environment variable names are not case-sensitive.

To fix this, ensure that all options have different fallback environment variables.";

            return new CliFxException(message.Trim());
        }
    }

    // End-user-facing exceptions
    // Avoid internal details and fix recommendations here
    public partial class CliFxException
    {
        internal static CliFxException CannotFindMatchingCommand(CommandLineInput input)
        {
            var message = $@"
Can't find a command that matches the following arguments:
{string.Join(" ", input.UnboundArguments.Select(a => a.Value))}";

            return new CliFxException(message.Trim());
        }

        internal static CliFxException CannotConvertMultipleValuesToNonScalar(
            CommandArgumentSchema argument,
            IReadOnlyList<string> values)
        {
            var argumentDisplayText = argument is CommandParameterSchema
                ? $"Parameter <{argument.DisplayName}>"
                : $"Option '{argument.DisplayName}'";

            var message = $@"
{argumentDisplayText} expects a single value, but provided with multiple:
{string.Join(", ", values.Select(v => $"'{v}'"))}";

            return new CliFxException(message.Trim());
        }

        internal static CliFxException CannotConvertToType(
            CommandArgumentSchema argument,
            string? value,
            Type type,
            Exception? innerException = null)
        {
            var argumentDisplayText = argument is CommandParameterSchema
                ? $"parameter <{argument.DisplayName}>"
                : $"option '{argument.DisplayName}'";

            var message = $@"
Can't convert value '{value ?? "<null>"}' to type '{type.FullName}' for {argumentDisplayText}.
{innerException?.Message ?? "This type is not supported."}";

            return new CliFxException(message.Trim(), innerException);
        }

        internal static CliFxException CannotConvertNonScalar(
            CommandArgumentSchema argument,
            IReadOnlyList<string> values,
            Type type)
        {
            var argumentDisplayText = argument is CommandParameterSchema
                ? $"parameter <{argument.DisplayName}>"
                : $"option '{argument.DisplayName}'";

            var message = $@"
Can't convert provided values to type '{type.FullName}' for {argumentDisplayText}:
{string.Join(", ", values.Select(v => $"'{v}'"))}

Target type is not assignable from array and doesn't have a public constructor that takes an array.";

            return new CliFxException(message.Trim());
        }

        internal static CliFxException ParameterNotSet(CommandParameterSchema parameter)
        {
            var message = $@"
Missing value for parameter <{parameter.DisplayName}>.";

            return new CliFxException(message.Trim());
        }

        internal static CliFxException RequiredOptionsNotSet(IReadOnlyList<CommandOptionSchema> options)
        {
            var message = $@"
Missing values for one or more required options:
{string.Join(Environment.NewLine, options.Select(o => o.DisplayName))}";

            return new CliFxException(message.Trim());
        }

        internal static CliFxException UnrecognizedParametersProvided(IReadOnlyList<CommandUnboundArgumentInput> inputs)
        {
            var message = $@"
Unrecognized parameters provided:
{string.Join(Environment.NewLine, inputs.Select(i => $"<{i.Value}>"))}";

            return new CliFxException(message.Trim());
        }

        internal static CliFxException UnrecognizedOptionsProvided(IReadOnlyList<CommandOptionInput> inputs)
        {
            var message = $@"
Unrecognized options provided:
{string.Join(Environment.NewLine, inputs.Select(i => i.DisplayAlias))}";

            return new CliFxException(message.Trim());
        }
    }
}