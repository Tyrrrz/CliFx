using Microsoft.CodeAnalysis;

namespace CliFx.Generators;

internal static class DiagnosticDescriptors
{
    // Command

    public static DiagnosticDescriptor CommandMustBePartial { get; } =
        new(
            $"{nameof(CliFx)}_{nameof(CommandMustBePartial)}",
            "Command class must be declared as partial",
            "Type '{0}' is decorated with [Command] but the source generator cannot access it because it is not partial. Make sure that the type itself, as well as all its containing types, are declared as partial.",
            "CliFx",
            DiagnosticSeverity.Error,
            true
        );

    public static DiagnosticDescriptor CommandMustImplementICommand { get; } =
        new(
            $"{nameof(CliFx)}_{nameof(CommandMustImplementICommand)}",
            "Command class must implement ICommand",
            "Type '{0}' is decorated with [Command] but does not implement 'ICommand'. In order to be recognized as a command, a type must implement the 'ICommand' interface.",
            "CliFx",
            DiagnosticSeverity.Error,
            true
        );

    public static DiagnosticDescriptor CommandInputConverterNotInferrable { get; } =
        new(
            $"{nameof(CliFx)}_{nameof(CommandInputConverterNotInferrable)}",
            "Command input converter cannot be inferred",
            "Property '{0}' of type '{1}' has no applicable default converter and no custom converter was specified. Provide a converter via the 'Converter' property of the attribute.",
            "CliFx",
            DiagnosticSeverity.Error,
            true
        );

    // Parameter

    public static DiagnosticDescriptor CommandParameterMustHaveName { get; } =
        new(
            $"{nameof(CliFx)}_{nameof(CommandParameterMustHaveName)}",
            "Command parameter name must not be empty",
            "Parameter name on property '{0}' must not be null or empty. Either specify a valid name or omit the 'Name' property to use the auto-generated name.",
            "CliFx",
            DiagnosticSeverity.Error,
            true
        );

    public static DiagnosticDescriptor CommandParametersMustHaveUniqueOrder { get; } =
        new(
            $"{nameof(CliFx)}_{nameof(CommandParametersMustHaveUniqueOrder)}",
            "Command parameters must have unique order values",
            "Parameter order on property '{0}' is the same as on '{1}': {2}. Each parameter must have a unique order value.",
            "CliFx",
            DiagnosticSeverity.Error,
            true
        );

    public static DiagnosticDescriptor CommandParametersMustHaveUniqueNames { get; } =
        new(
            $"{nameof(CliFx)}_{nameof(CommandParametersMustHaveUniqueNames)}",
            "Command parameters must have unique names",
            "Parameter name on property '{0}' is the same as on '{1}': '{2}'. Each parameter must have a unique name (comparison IS NOT case-sensitive).",
            "CliFx",
            DiagnosticSeverity.Warning,
            true
        );

    // Option

    public static DiagnosticDescriptor CommandOptionMustHaveNameOrShortName { get; } =
        new(
            $"{nameof(CliFx)}_{nameof(CommandOptionMustHaveNameOrShortName)}",
            "Command option must have a name or short name",
            "Option property '{0}' must have either a name or a short name specified.",
            "CliFx",
            DiagnosticSeverity.Warning,
            true
        );

    public static DiagnosticDescriptor CommandOptionNameInvalid { get; } =
        new(
            $"{nameof(CliFx)}_{nameof(CommandOptionNameInvalid)}",
            "Command option name is invalid",
            "Option name on property '{0}' is invalid: '{1}'. Option names must be at least 2 characters long, must start with a letter, and must not contain whitespace.",
            "CliFx",
            DiagnosticSeverity.Warning,
            true
        );

    public static DiagnosticDescriptor CommandOptionsMustHaveUniqueNames { get; } =
        new(
            $"{nameof(CliFx)}_{nameof(CommandOptionsMustHaveUniqueNames)}",
            "Command options must have unique names",
            "Option name on property '{0}' is the same as on '{1}': '{2}'. Each option must have a unique name (comparison IS NOT case-sensitive).",
            "CliFx",
            DiagnosticSeverity.Warning,
            true
        );

    public static DiagnosticDescriptor CommandOptionsMustHaveUniqueShortNames { get; } =
        new(
            $"{nameof(CliFx)}_{nameof(CommandOptionsMustHaveUniqueShortNames)}",
            "Command options must have unique short names",
            "Option short name on property '{0}' is the same as on '{1}': '{2}'. Each option must have a unique short name (comparison IS case-sensitive).",
            "CliFx",
            DiagnosticSeverity.Warning,
            true
        );
}
