using Microsoft.CodeAnalysis;

namespace CliFx.Generators;

public static class DiagnosticDescriptors
{
    // Command

    public static DiagnosticDescriptor CommandMustBePartial { get; } =
        new(
            $"{nameof(CliFx)}_{nameof(CommandMustBePartial)}",
            "Command class must be declared as partial",
            "Type '{0}' is decorated with [Command] but it is not partial. Make sure that the type itself, as well as all its containing types, are declared as partial, so that the source generator can extend it.",
            "CliFx",
            DiagnosticSeverity.Error,
            true
        );

    public static DiagnosticDescriptor CommandMustImplementICommand { get; } =
        new(
            $"{nameof(CliFx)}_{nameof(CommandMustImplementICommand)}",
            "Command class must implement ICommand",
            "Type '{0}' is decorated with [Command] but does not implement 'ICommand'. In order to be recognized as a command, the type must implement the 'ICommand' interface.",
            "CliFx",
            DiagnosticSeverity.Error,
            true
        );

    public static DiagnosticDescriptor CommandInputConverterNotInferrable { get; } =
        new(
            $"{nameof(CliFx)}_{nameof(CommandInputConverterNotInferrable)}",
            "Command input converter cannot be inferred",
            "Input bound to property '{0}' of type '{1}' has no applicable default converter and no custom converter was specified. Provide a converter via the 'Converter' property of the attribute.",
            "CliFx",
            DiagnosticSeverity.Error,
            true
        );

    // Parameter

    public static DiagnosticDescriptor CommandParameterMustHaveName { get; } =
        new(
            $"{nameof(CliFx)}_{nameof(CommandParameterMustHaveName)}",
            "Command parameter name must not be empty",
            "Name of the parameter bound to property '{0}' must not be null or empty. Either specify a valid name or omit the 'Name' property to use the auto-generated name.",
            "CliFx",
            DiagnosticSeverity.Error,
            true
        );

    public static DiagnosticDescriptor CommandParameterMustHaveUniqueOrder { get; } =
        new(
            $"{nameof(CliFx)}_{nameof(CommandParameterMustHaveUniqueOrder)}",
            "Command parameter must have a unique order value",
            "Order of the parameter bound to property '{0}' is the same as the order of the parameter bound to property '{1}': {2}. Each parameter must have a unique order value.",
            "CliFx",
            DiagnosticSeverity.Error,
            true
        );

    public static DiagnosticDescriptor CommandParameterMustHaveHighestOrderIfNotRequired { get; } =
        new(
            $"{nameof(CliFx)}_{nameof(CommandParameterMustHaveHighestOrderIfNotRequired)}",
            "Non-required command parameter must be the last parameter in order",
            "Non-required parameter bound to property '{0}' is followed by the parameter bound to property '{1}'. A non-required parameter must be the last parameter in order. By extension, only one parameter in a command can be non-required.",
            "CliFx",
            DiagnosticSeverity.Error,
            true
        );

    public static DiagnosticDescriptor CommandParameterMustHaveHighestOrderIfSequenceBased { get; } =
        new(
            $"{nameof(CliFx)}_{nameof(CommandParameterMustHaveHighestOrderIfSequenceBased)}",
            "Sequence-based command parameter must be the last parameter in order",
            "Sequence-based parameter bound to property '{0}' is followed by the parameter bound to property '{1}'. A sequence-based parameter must be the last parameter in order. By extension, only one parameter in a command can be sequence-based.",
            "CliFx",
            DiagnosticSeverity.Error,
            true
        );

    public static DiagnosticDescriptor CommandParameterMustHaveUniqueName { get; } =
        new(
            $"{nameof(CliFx)}_{nameof(CommandParameterMustHaveUniqueName)}",
            "Command parameter must have a unique name",
            "Name of the parameter bound to property '{0}' is the same as the name of the parameter bound to property '{1}': '{2}'. Each parameter must have a unique name (comparison IS NOT case-sensitive).",
            "CliFx",
            DiagnosticSeverity.Warning,
            true
        );

    // Option

    public static DiagnosticDescriptor CommandOptionMustHaveNameOrShortName { get; } =
        new(
            $"{nameof(CliFx)}_{nameof(CommandOptionMustHaveNameOrShortName)}",
            "Command option must have a name or short name",
            "Option bound to property '{0}' must have either a name or a short name specified.",
            "CliFx",
            DiagnosticSeverity.Warning,
            true
        );

    public static DiagnosticDescriptor CommandOptionNameMustBeValid { get; } =
        new(
            $"{nameof(CliFx)}_{nameof(CommandOptionNameMustBeValid)}",
            "Command option name must be valid",
            "Name of the option bound to property '{0}' is invalid: '{1}'. Option names must be at least 2 characters long, must start with a letter, and must not contain whitespace.",
            "CliFx",
            DiagnosticSeverity.Warning,
            true
        );

    public static DiagnosticDescriptor CommandOptionMustHaveUniqueName { get; } =
        new(
            $"{nameof(CliFx)}_{nameof(CommandOptionMustHaveUniqueName)}",
            "Command option must have a unique name",
            "Name of the option bound to property '{0}' is the same as the name of the option bound to property '{1}': '{2}'. Each option must have a unique name (comparison IS NOT case-sensitive).",
            "CliFx",
            DiagnosticSeverity.Warning,
            true
        );

    public static DiagnosticDescriptor CommandOptionMustHaveUniqueShortName { get; } =
        new(
            $"{nameof(CliFx)}_{nameof(CommandOptionMustHaveUniqueShortName)}",
            "Command option must have a unique short name",
            "Short name of the option bound to property '{0}' is the same as the short name of the option bound to property '{1}': '{2}'. Each option must have a unique short name (comparison IS case-sensitive).",
            "CliFx",
            DiagnosticSeverity.Warning,
            true
        );
}
