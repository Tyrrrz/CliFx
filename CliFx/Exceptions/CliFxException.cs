using System;
using System.Collections.Generic;
using System.Linq;
using CliFx.Attributes;
using CliFx.Domain;
using CliFx.Internal;

namespace CliFx.Exceptions
{
    /// <summary>
    /// Domain exception thrown within CliFx.
    /// </summary>
    public partial class CliFxException : Exception
    {
        private const int DefaultExitCode = -100;

        private readonly bool _isMessageSet;

        /// <summary>
        /// Returns an exit code associated with this exception.
        /// </summary>
        public int ExitCode { get; }

        /// <summary>
        /// Whether to show the help text after handling this exception.
        /// </summary>
        public bool ShowHelp { get; }

        /// <summary>
        /// Initializes an instance of <see cref="CliFxException"/>.
        /// </summary>
        public CliFxException(string? message, Exception? innerException, int exitCode = DefaultExitCode, bool showHelp = false)
            : base(message, innerException)
        {
            // Message property has a fallback so it's never empty, hence why we need this check
            _isMessageSet = !string.IsNullOrWhiteSpace(message);

            ExitCode = exitCode;
            ShowHelp = showHelp;
        }

        /// <summary>
        /// Initializes an instance of <see cref="CliFxException"/>.
        /// </summary>
        public CliFxException(string? message, int exitCode = DefaultExitCode, bool showHelp = false)
            : this(message, null, exitCode, showHelp)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="CliFxException"/>.
        /// </summary>
        public CliFxException(int exitCode = DefaultExitCode, bool showHelp = false)
            : this(null, exitCode, showHelp)
        {
        }

        internal string GetConciseMessage() => _isMessageSet
            ? Message
            : ToString();
    }

    // Internal exceptions
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

        internal static CliFxException DelegateActivatorReturnedNull(Type type)
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

If you're experiencing problems, please refer to the readme for a quickstart example.";

            return new CliFxException(message.Trim());
        }

        internal static CliFxException CommandsNotRegistered()
        {
            var message = $@"
There are no commands configured in the application.

To fix this, ensure that at least one command is added through one of the methods on {nameof(CliApplicationBuilder)}.
If you're experiencing problems, please refer to the readme for a quickstart example.";

            return new CliFxException(message.Trim());
        }

        internal static CliFxException CommandsTooManyDefaults(IReadOnlyList<CommandSchema> invalidCommandSchemas)
        {
            var message = $@"
Application configuration is invalid because there are {invalidCommandSchemas.Count} default commands:
{invalidCommandSchemas.JoinToString(Environment.NewLine)}

There can only be one default command (i.e. command with no name) in an application.
Other commands must have unique non-empty names that identify them.";

            return new CliFxException(message.Trim());
        }

        internal static CliFxException CommandsDuplicateName(
            string name,
            IReadOnlyList<CommandSchema> invalidCommandSchemas)
        {
            var message = $@"
Application configuration is invalid because there are {invalidCommandSchemas.Count} commands with the same name ('{name}'):
{invalidCommandSchemas.JoinToString(Environment.NewLine)}

Commands must have unique names.
Names are not case-sensitive.";

            return new CliFxException(message.Trim());
        }

        internal static CliFxException CommandParametersDuplicateOrder(
            CommandSchema commandSchema,
            int order,
            IReadOnlyList<CommandParameterSchema> invalidParameterSchemas)
        {
            var message = $@"
Command '{commandSchema.Type.FullName}' is invalid because it contains {invalidParameterSchemas.Count} parameters with the same order ({order}):
{invalidParameterSchemas.JoinToString(Environment.NewLine)}

Parameters must have unique order.";

            return new CliFxException(message.Trim());
        }

        internal static CliFxException CommandParametersDuplicateName(
            CommandSchema commandSchema,
            string name,
            IReadOnlyList<CommandParameterSchema> invalidParameterSchemas)
        {
            var message = $@"
Command '{commandSchema.Type.FullName}' is invalid because it contains {invalidParameterSchemas.Count} parameters with the same name ('{name}'):
{invalidParameterSchemas.JoinToString(Environment.NewLine)}

Parameters must have unique names to avoid potential confusion in the help text.
Names are not case-sensitive.";

            return new CliFxException(message.Trim());
        }

        internal static CliFxException CommandParametersTooManyNonScalar(
            CommandSchema commandSchema,
            IReadOnlyList<CommandParameterSchema> invalidParameterSchemas)
        {
            var message = $@"
Command '{commandSchema.Type.FullName}' is invalid because it contains {invalidParameterSchemas.Count} non-scalar parameters:
{invalidParameterSchemas.JoinToString(Environment.NewLine)}

Non-scalar parameter is such that is bound from more than one value (e.g. array or a complex object).
Only one parameter in a command may be non-scalar and it must be the last one in order.

If it's not feasible to fit into these constraints, consider using options instead as they don't have these limitations.";

            return new CliFxException(message.Trim());
        }

        internal static CliFxException CommandParametersNonLastNonScalar(
            CommandSchema commandSchema,
            CommandParameterSchema invalidParameterSchema)
        {
            var message = $@"
Command '{commandSchema.Type.FullName}' is invalid because it contains a non-scalar parameter which is not the last in order:
{invalidParameterSchema}

Non-scalar parameter is such that is bound from more than one value (e.g. array or a complex object).
Only one parameter in a command may be non-scalar and it must be the last one in order.

If it's not feasible to fit into these constraints, consider using options instead as they don't have these limitations.";

            return new CliFxException(message.Trim());
        }

        internal static CliFxException CommandOptionsNoName(
            CommandSchema commandSchema,
            IReadOnlyList<CommandOptionSchema> invalidOptionSchemas)
        {
            var message = $@"
Command '{commandSchema.Type.FullName}' is invalid because it contains one or more options without a name:
{invalidOptionSchemas.JoinToString(Environment.NewLine)}

Options must have either a name or a short name or both.";

            return new CliFxException(message.Trim());
        }

        internal static CliFxException CommandOptionsInvalidLengthName(
            CommandSchema commandSchema,
            IReadOnlyList<CommandOptionSchema> invalidOptionSchemas)
        {
            var message = $@"
Command '{commandSchema.Type.FullName}' is invalid because it contains one or more options whose names are too short:
{invalidOptionSchemas.JoinToString(Environment.NewLine)}

Option names must be at least 2 characters long to avoid confusion with short names.
If you intended to set the short name instead, use the attribute overload that accepts a char.";

            return new CliFxException(message.Trim());
        }

        internal static CliFxException CommandOptionsDuplicateName(
            CommandSchema commandSchema,
            string name,
            IReadOnlyList<CommandOptionSchema> invalidOptionSchemas)
        {
            var message = $@"
Command '{commandSchema.Type.FullName}' is invalid because it contains {invalidOptionSchemas.Count} options with the same name ('{name}'):
{invalidOptionSchemas.JoinToString(Environment.NewLine)}

Options must have unique names.
Names are not case-sensitive.";

            return new CliFxException(message.Trim());
        }

        internal static CliFxException CommandOptionsDuplicateShortName(
            CommandSchema commandSchema,
            char shortName,
            IReadOnlyList<CommandOptionSchema> invalidOptionSchemas)
        {
            var message = $@"
Command '{commandSchema.Type.FullName}' is invalid because it contains {invalidOptionSchemas.Count} options with the same short name ('{shortName}'):
{invalidOptionSchemas.JoinToString(Environment.NewLine)}

Options must have unique short names.
Short names are case-sensitive (i.e. 'a' and 'A' are different short names).";

            return new CliFxException(message.Trim());
        }

        internal static CliFxException CommandOptionsDuplicateEnvironmentVariableName(
            CommandSchema commandSchema,
            string environmentVariableName,
            IReadOnlyList<CommandOptionSchema> invalidOptionSchemas)
        {
            var message = $@"
Command '{commandSchema.Type.FullName}' is invalid because it contains {invalidOptionSchemas.Count} options with the same fallback environment variable name ('{environmentVariableName}'):
{invalidOptionSchemas.JoinToString(Environment.NewLine)}

Options cannot share the same environment variable as a fallback.
Environment variable names are not case-sensitive.";

            return new CliFxException(message.Trim());
        }
    }

    // End-user-facing exceptions
    // Avoid internal details and fix recommendations here
    public partial class CliFxException
    {
        internal static CliFxException CannotConvertMultipleValuesToNonScalar(
            CommandParameterSchema parameterSchema,
            IReadOnlyList<string> values)
        {
            var message = $@"
Parameter {parameterSchema.GetUserFacingDisplayString()} expects a single value, but provided with multiple:
{values.Select(v => v.Quote()).JoinToString(" ")}";

            return new CliFxException(message.Trim(), showHelp: true);
        }

        internal static CliFxException CannotConvertMultipleValuesToNonScalar(
            CommandOptionSchema optionSchema,
            IReadOnlyList<string> values)
        {
            var message = $@"
Option {optionSchema.GetUserFacingDisplayString()} expects a single value, but provided with multiple:
{values.Select(v => v.Quote()).JoinToString(" ")}";

            return new CliFxException(message.Trim(), showHelp: true);
        }

        internal static CliFxException CannotConvertMultipleValuesToNonScalar(
            CommandArgumentSchema argumentSchema,
            IReadOnlyList<string> values) => argumentSchema switch
        {
            CommandParameterSchema parameterSchema => CannotConvertMultipleValuesToNonScalar(parameterSchema, values),
            CommandOptionSchema optionSchema => CannotConvertMultipleValuesToNonScalar(optionSchema, values),
            _ => throw new ArgumentOutOfRangeException(nameof(argumentSchema))
        };

        internal static CliFxException CannotConvertToType(
            CommandParameterSchema parameterSchema,
            string? value,
            Type type,
            Exception? innerException = null)
        {
            var message = $@"
Can't convert value ""{value ?? "<null>"}"" to type '{type.Name}' for parameter {parameterSchema.GetUserFacingDisplayString()}.
{innerException?.Message ?? "This type is not supported."}";

            return new CliFxException(message.Trim(), innerException, showHelp: true);
        }

        internal static CliFxException CannotConvertToType(
            CommandOptionSchema optionSchema,
            string? value,
            Type type,
            Exception? innerException = null)
        {
            var message = $@"
Can't convert value ""{value ?? "<null>"}"" to type '{type.Name}' for option {optionSchema.GetUserFacingDisplayString()}.
{innerException?.Message ?? "This type is not supported."}";

            return new CliFxException(message.Trim(), innerException, showHelp: true);
        }

        internal static CliFxException CannotConvertToType(
            CommandArgumentSchema argumentSchema,
            string? value,
            Type type,
            Exception? innerException = null) => argumentSchema switch
        {
            CommandParameterSchema parameterSchema => CannotConvertToType(parameterSchema, value, type, innerException),
            CommandOptionSchema optionSchema => CannotConvertToType(optionSchema, value, type, innerException),
            _ => throw new ArgumentOutOfRangeException(nameof(argumentSchema))
        };

        internal static CliFxException CannotConvertNonScalar(
            CommandParameterSchema parameterSchema,
            IReadOnlyList<string> values,
            Type type)
        {
            var message = $@"
Can't convert provided values to type '{type.Name}' for parameter {parameterSchema.GetUserFacingDisplayString()}:
{values.Select(v => v.Quote()).JoinToString(" ")}

Target type is not assignable from array and doesn't have a public constructor that takes an array.";

            return new CliFxException(message.Trim(), showHelp: true);
        }

        internal static CliFxException CannotConvertNonScalar(
            CommandOptionSchema optionSchema,
            IReadOnlyList<string> values,
            Type type)
        {
            var message = $@"
Can't convert provided values to type '{type.Name}' for option {optionSchema.GetUserFacingDisplayString()}:
{values.Select(v => v.Quote()).JoinToString(" ")}

Target type is not assignable from array and doesn't have a public constructor that takes an array.";

            return new CliFxException(message.Trim(), showHelp: true);
        }

        internal static CliFxException CannotConvertNonScalar(
            CommandArgumentSchema argumentSchema,
            IReadOnlyList<string> values,
            Type type) => argumentSchema switch
        {
            CommandParameterSchema parameterSchema => CannotConvertNonScalar(parameterSchema, values, type),
            CommandOptionSchema optionSchema => CannotConvertNonScalar(optionSchema, values, type),
            _ => throw new ArgumentOutOfRangeException(nameof(argumentSchema))
        };

        internal static CliFxException ParameterNotSet(CommandParameterSchema parameterSchema)
        {
            var message = $@"
Missing value for parameter {parameterSchema.GetUserFacingDisplayString()}.";

            return new CliFxException(message.Trim(), showHelp: true);
        }

        internal static CliFxException RequiredOptionsNotSet(IReadOnlyList<CommandOptionSchema> optionSchemas)
        {
            var message = $@"
Missing values for one or more required options:
{optionSchemas.Select(o => o.GetUserFacingDisplayString()).JoinToString(Environment.NewLine)}";

            return new CliFxException(message.Trim(), showHelp: true);
        }

        internal static CliFxException UnrecognizedParametersProvided(IReadOnlyList<string> parameterInputs)
        {
            var message = $@"
Unrecognized parameters provided:
{parameterInputs.Select(p => p.Quote()).JoinToString(" ")}";

            return new CliFxException(message.Trim(), showHelp: true);
        }

        internal static CliFxException UnrecognizedOptionsProvided(IReadOnlyList<KeyValuePair<string, IReadOnlyList<string>>> optionInputs)
        {
            var message = $@"
Unrecognized options provided:
{optionInputs.Select(o => o.Key.PrefixDashes()).JoinToString(Environment.NewLine)}";

            return new CliFxException(message.Trim(), showHelp: true);
        }
    }
}