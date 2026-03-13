using Microsoft.CodeAnalysis;

namespace CliFx.Generators;

internal static class DiagnosticDescriptors
{
    public static DiagnosticDescriptor OptionMustHaveNameOrShortName { get; } =
        new(
            $"{nameof(CliFx)}_{nameof(OptionMustHaveNameOrShortName)}",
            "Command option must have a name or short name",
            "Option property '{0}' must have either a name or a short name specified.",
            "CliFx",
            DiagnosticSeverity.Warning,
            true
        );

    public static DiagnosticDescriptor OptionNameInvalid { get; } =
        new(
            $"{nameof(CliFx)}_{nameof(OptionNameInvalid)}",
            "Command option name is invalid",
            "Option name '{0}' on property '{1}' is invalid. Option names must be at least 2 characters long, must not start with a dash, and must not contain whitespace.",
            "CliFx",
            DiagnosticSeverity.Warning,
            true
        );

    public static DiagnosticDescriptor ParameterMustHaveName { get; } =
        new(
            $"{nameof(CliFx)}_{nameof(ParameterMustHaveName)}",
            "Command parameter name must not be empty",
            "Parameter name on property '{0}' must not be null or empty.",
            "CliFx",
            DiagnosticSeverity.Warning,
            true
        );

    public static DiagnosticDescriptor OptionsMustHaveUniqueNames { get; } =
        new(
            $"{nameof(CliFx)}_{nameof(OptionsMustHaveUniqueNames)}",
            "Command options must have unique names and short names",
            "Option '{0}' on type '{1}' has a duplicate {2} '{3}' (also used by '{4}').",
            "CliFx",
            DiagnosticSeverity.Warning,
            true
        );

    public static DiagnosticDescriptor ParametersMustHaveUniqueOrder { get; } =
        new(
            $"{nameof(CliFx)}_{nameof(ParametersMustHaveUniqueOrder)}",
            "Command parameters must have unique order values",
            "Parameter '{0}' on type '{1}' has a duplicate order value {2} (also used by '{3}').",
            "CliFx",
            DiagnosticSeverity.Warning,
            true
        );

    public static DiagnosticDescriptor ParametersMustHaveUniqueNames { get; } =
        new(
            $"{nameof(CliFx)}_{nameof(ParametersMustHaveUniqueNames)}",
            "Command parameters must have unique names",
            "Parameter '{0}' on type '{1}' has a duplicate name '{2}' (also used by '{3}').",
            "CliFx",
            DiagnosticSeverity.Warning,
            true
        );

    public static DiagnosticDescriptor CommandMustBePartial { get; } =
        new(
            $"{nameof(CliFx)}_{nameof(CommandMustBePartial)}",
            "Command class must be declared as partial",
            "Type '{0}' is decorated with [Command] but is not declared as 'partial'. Declare it as 'partial' to enable source generation.",
            "CliFx",
            DiagnosticSeverity.Error,
            true
        );

    public static DiagnosticDescriptor CommandMustImplementICommand { get; } =
        new(
            $"{nameof(CliFx)}_{nameof(CommandMustImplementICommand)}",
            "Command class must implement ICommand",
            "Type '{0}' is decorated with [Command] but does not implement 'ICommand'. Implement 'ICommand' to enable source generation.",
            "CliFx",
            DiagnosticSeverity.Error,
            true
        );
}
