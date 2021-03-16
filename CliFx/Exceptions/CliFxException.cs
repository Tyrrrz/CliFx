using System;
using System.Collections.Generic;
using System.Linq;
using CliFx.Attributes;
using CliFx.Input;
using CliFx.Schema;
using CliFx.Utils.Extensions;

namespace CliFx.Exceptions
{
    /// <summary>
    /// Exception thrown when there is an error during application execution.
    /// </summary>
    public partial class CliFxException : Exception
    {
        internal const int DefaultExitCode = 1;

        // Regular `exception.Message` never returns null, even if
        // it hasn't been set.
        internal string? ActualMessage { get; }

        /// <summary>
        /// Returned exit code.
        /// </summary>
        public int ExitCode { get; }

        /// <summary>
        /// Whether to show the help text before exiting.
        /// </summary>
        public bool ShowHelp { get; }

        /// <summary>
        /// Initializes an instance of <see cref="CliFxException"/>.
        /// </summary>
        public CliFxException(
            string message,
            int exitCode = DefaultExitCode,
            bool showHelp = false,
            Exception? innerException = null)
            : base(message, innerException)
        {
            ActualMessage = message;
            ExitCode = exitCode;
            ShowHelp = showHelp;
        }
    }

    public partial class CliFxException
    {
        // Internal errors don't show help because they're meant for the developer
        // and not the end-user of the application.
        internal static CliFxException InternalError(string message, Exception? innerException = null) =>
            new(message, DefaultExitCode, false, innerException);

        // User errors are typically caused by invalid input and they're meant for
        // the end-user, so we want to show help.
        internal static CliFxException UserError(string message, Exception? innerException = null) =>
            new(message, DefaultExitCode, true, innerException);
    }

    // TODO: refactor
    // End-user-facing exceptions
    // Avoid internal details and fix recommendations here
    public partial class CliFxException
    {
        internal static CliFxException CannotConvertMultipleValuesToNonScalar(
            ParameterSchema parameter,
            IReadOnlyList<string> values)
        {
            var message = $@"
Parameter {parameter.GetUserFacingDisplayString()} expects a single value, but provided with multiple:
{values.Select(v => v.Quote()).JoinToString(" ")}";

            return new CliFxException(message.Trim(), showHelp: true);
        }

        internal static CliFxException CannotConvertMultipleValuesToNonScalar(
            OptionSchema option,
            IReadOnlyList<string> values)
        {
            var message = $@"
Option {option.GetUserFacingDisplayString()} expects a single value, but provided with multiple:
{values.Select(v => v.Quote()).JoinToString(" ")}";

            return new CliFxException(message.Trim(), showHelp: true);
        }

        internal static CliFxException CannotConvertMultipleValuesToNonScalar(
            MemberSchema argument,
            IReadOnlyList<string> values) => argument switch
        {
            ParameterSchema parameter => CannotConvertMultipleValuesToNonScalar(parameter, values),
            OptionSchema option => CannotConvertMultipleValuesToNonScalar(option, values),
            _ => throw new ArgumentOutOfRangeException(nameof(argument))
        };

        internal static CliFxException CannotConvertToType(
            ParameterSchema parameter,
            string? value,
            Type type,
            Exception? innerException = null)
        {
            var message = $@"
Can't convert value ""{value ?? "<null>"}"" to type '{type.Name}' for parameter {parameter.GetUserFacingDisplayString()}.
{innerException?.Message ?? "This type is not supported."}";

            return new CliFxException(message.Trim(), showHelp: true, innerException: innerException);
        }

        internal static CliFxException CannotConvertToType(
            OptionSchema option,
            string? value,
            Type type,
            Exception? innerException = null)
        {
            var message = $@"
Can't convert value ""{value ?? "<null>"}"" to type '{type.Name}' for option {option.GetUserFacingDisplayString()}.
{innerException?.Message ?? "This type is not supported."}";

            return new CliFxException(message.Trim(), showHelp: true, innerException: innerException);
        }

        internal static CliFxException CannotConvertToType(
            MemberSchema argument,
            string? value,
            Type type,
            Exception? innerException = null) => argument switch
        {
            ParameterSchema parameter => CannotConvertToType(parameter, value, type, innerException),
            OptionSchema option => CannotConvertToType(option, value, type, innerException),
            _ => throw new ArgumentOutOfRangeException(nameof(argument))
        };

        internal static CliFxException CannotConvertNonScalar(
            ParameterSchema parameter,
            IReadOnlyList<string> values,
            Type type)
        {
            var message = $@"
Can't convert provided values to type '{type.Name}' for parameter {parameter.GetUserFacingDisplayString()}:
{values.Select(v => v.Quote()).JoinToString(" ")}

Target type is not assignable from array and doesn't have a public constructor that takes an array.";

            return new CliFxException(message.Trim(), showHelp: true);
        }

        internal static CliFxException CannotConvertNonScalar(
            OptionSchema option,
            IReadOnlyList<string> values,
            Type type)
        {
            var message = $@"
Can't convert provided values to type '{type.Name}' for option {option.GetUserFacingDisplayString()}:
{values.Select(v => v.Quote()).JoinToString(" ")}

Target type is not assignable from array and doesn't have a public constructor that takes an array.";

            return new CliFxException(message.Trim(), showHelp: true);
        }

        internal static CliFxException CannotConvertNonScalar(
            MemberSchema argument,
            IReadOnlyList<string> values,
            Type type) => argument switch
        {
            ParameterSchema parameter => CannotConvertNonScalar(parameter, values, type),
            OptionSchema option => CannotConvertNonScalar(option, values, type),
            _ => throw new ArgumentOutOfRangeException(nameof(argument))
        };

        internal static CliFxException ParameterNotSet(ParameterSchema parameter)
        {
            var message = $@"
Missing value for parameter {parameter.GetUserFacingDisplayString()}.";

            return new CliFxException(message.Trim(), showHelp: true);
        }

        internal static CliFxException RequiredOptionsNotSet(IReadOnlyList<OptionSchema> options)
        {
            var message = $@"
Missing values for one or more required options:
{options.Select(o => o.GetUserFacingDisplayString()).JoinToString(Environment.NewLine)}";

            return new CliFxException(message.Trim(), showHelp: true);
        }

        internal static CliFxException UnrecognizedParametersProvided(
            IReadOnlyList<ParameterInput> parameterInputs)
        {
            var message = $@"
Unrecognized parameters provided:
{parameterInputs.Select(p => p.Value).JoinToString(Environment.NewLine)}";

            return new CliFxException(message.Trim(), showHelp: true);
        }

        internal static CliFxException UnrecognizedOptionsProvided(IReadOnlyList<OptionInput> optionInputs)
        {
            var message = $@"
Unrecognized options provided:
{optionInputs.Select(o => o.GetFormattedIdentifier()).JoinToString(Environment.NewLine)}";

            return new CliFxException(message.Trim(), showHelp: true);
        }
    }
}