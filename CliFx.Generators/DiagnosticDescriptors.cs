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
            "Type '{0}' is decorated with [Command] but does not implement 'ICommand'. In order to be recognized as a command, the type must also implement the 'ICommand' interface.",
            "CliFx",
            DiagnosticSeverity.Error,
            true
        );

    // Inputs

    public static DiagnosticDescriptor CommandInputConverterNotInferrable { get; } =
        new(
            $"{nameof(CliFx)}_{nameof(CommandInputConverterNotInferrable)}",
            "Input converter cannot be inferred",
            "Input bound to property '{0}' of type '{1}' has no applicable default converter and no custom converter was specified. Provide a converter via the 'Converter' property of the attribute.",
            "CliFx",
            DiagnosticSeverity.Error,
            true
        );

    public static DiagnosticDescriptor CommandInputElementConverterMustNotBeSequenceBased { get; } =
        new(
            $"{nameof(CliFx)}_{nameof(CommandInputElementConverterMustNotBeSequenceBased)}",
            "Element converter must not be a sequence converter",
            "Input bound to property '{0}' specifies 'IsElementConverter = true' but the provided converter derives from SequenceInputConverter<T>. When using 'IsElementConverter', the converter must be a scalar converter (derive from ScalarInputConverter<T>).",
            "CliFx",
            DiagnosticSeverity.Error,
            true
        );

    public static DiagnosticDescriptor CommandInputElementConverterRequiresSequenceProperty { get; } =
        new(
            $"{nameof(CliFx)}_{nameof(CommandInputElementConverterRequiresSequenceProperty)}",
            "Element converter requires a sequence-based property",
            "Input bound to property '{0}' specifies 'IsElementConverter = true' but the property type '{1}' is not a sequence. 'IsElementConverter' can only be used with sequence-based properties (e.g., arrays, lists, or other enumerable types).",
            "CliFx",
            DiagnosticSeverity.Error,
            true
        );

    // Parameter

    public static DiagnosticDescriptor CommandParameterMustHaveUniqueOrder { get; } =
        new(
            $"{nameof(CliFx)}_{nameof(CommandParameterMustHaveUniqueOrder)}",
            "Parameter must have a unique order value",
            "Order of the parameter bound to property '{0}' is the same as the order of the parameter bound to property '{1}': {2}. Each parameter in a command must have a unique order value.",
            "CliFx",
            DiagnosticSeverity.Error,
            true
        );

    public static DiagnosticDescriptor CommandParameterMustHaveHighestOrderIfNotRequired { get; } =
        new(
            $"{nameof(CliFx)}_{nameof(CommandParameterMustHaveHighestOrderIfNotRequired)}",
            "Non-required parameter must be the last parameter by order",
            "Non-required parameter bound to property '{0}' is followed by the parameter bound to property '{1}'. A non-required parameter must be the last parameter by order. By extension, only one non-required parameter can exist in a command.",
            "CliFx",
            DiagnosticSeverity.Error,
            true
        );

    public static DiagnosticDescriptor CommandParameterMustHaveHighestOrderIfSequenceBased { get; } =
        new(
            $"{nameof(CliFx)}_{nameof(CommandParameterMustHaveHighestOrderIfSequenceBased)}",
            "Sequence-based parameter must be the last parameter by order",
            "Sequence-based parameter bound to property '{0}' is followed by the parameter bound to property '{1}'. A sequence-based parameter must be the last parameter by order. By extension, only one sequence-based parameter can exist in a command.",
            "CliFx",
            DiagnosticSeverity.Error,
            true
        );

    public static DiagnosticDescriptor CommandParameterMustHaveName { get; } =
        new(
            $"{nameof(CliFx)}_{nameof(CommandParameterMustHaveName)}",
            "Parameter name must not be empty",
            "Name of the parameter bound to property '{0}' must not be null or empty. Either specify a valid name or omit the 'Name' property to use the auto-generated name.",
            "CliFx",
            DiagnosticSeverity.Error,
            true
        );

    public static DiagnosticDescriptor CommandParameterMustHaveUniqueName { get; } =
        new(
            $"{nameof(CliFx)}_{nameof(CommandParameterMustHaveUniqueName)}",
            "Parameter must have a unique name",
            "Parameter bound to property '{0}' has the same name as the parameter bound to property '{1}': '{2}'. Each parameter in a command must have a unique name (comparison IS NOT case-sensitive).",
            "CliFx",
            DiagnosticSeverity.Error,
            true
        );

    // Option

    public static DiagnosticDescriptor CommandOptionMustHaveNameOrShortName { get; } =
        new(
            $"{nameof(CliFx)}_{nameof(CommandOptionMustHaveNameOrShortName)}",
            "Option must have a name or short name",
            "Option bound to property '{0}' must have either a name or a short name specified (or both).",
            "CliFx",
            DiagnosticSeverity.Error,
            true
        );

    public static DiagnosticDescriptor CommandOptionMustHaveValidName { get; } =
        new(
            $"{nameof(CliFx)}_{nameof(CommandOptionMustHaveValidName)}",
            "Option name must be valid",
            "Option bound to property '{0}' has an invalid name: '{1}'. Option name must be at least 2 characters long, start with a letter, and not contain whitespace.",
            "CliFx",
            DiagnosticSeverity.Error,
            true
        );

    public static DiagnosticDescriptor CommandOptionMustHaveUniqueName { get; } =
        new(
            $"{nameof(CliFx)}_{nameof(CommandOptionMustHaveUniqueName)}",
            "Option name must be unique",
            "Option bound to property '{0}' has the same name as the option bound to property '{1}': '{2}'. Each option in a command must have a unique name (comparison IS NOT case-sensitive).",
            "CliFx",
            DiagnosticSeverity.Error,
            true
        );

    public static DiagnosticDescriptor CommandOptionMustHaveValidShortName { get; } =
        new(
            $"{nameof(CliFx)}_{nameof(CommandOptionMustHaveValidShortName)}",
            "Option short name must be valid",
            "Option bound to property '{0}' has an invalid short name: '{1}'. Option short name must be a single letter.",
            "CliFx",
            DiagnosticSeverity.Error,
            true
        );

    public static DiagnosticDescriptor CommandOptionMustHaveUniqueShortName { get; } =
        new(
            $"{nameof(CliFx)}_{nameof(CommandOptionMustHaveUniqueShortName)}",
            "Option short name must be unique",
            "Option bound to property '{0}' has the same short name as the option bound to property '{1}': '{2}'. Each option in a command must have a unique short name (comparison IS case-sensitive).",
            "CliFx",
            DiagnosticSeverity.Error,
            true
        );

    public static DiagnosticDescriptor CommandOptionShadowsBuiltInHelpOption { get; } =
        new(
            $"{nameof(CliFx)}_{nameof(CommandOptionShadowsBuiltInHelpOption)}",
            "Option shadows the conventional help option",
            "Option bound to property '{0}' shadows the conventional help option via '{1}'. Consider choosing a different identifier for your option.",
            "CliFx",
            DiagnosticSeverity.Warning,
            true
        );

    public static DiagnosticDescriptor CommandOptionShadowsBuiltInVersionOption { get; } =
        new(
            $"{nameof(CliFx)}_{nameof(CommandOptionShadowsBuiltInVersionOption)}",
            "Option shadows the conventional version option",
            "Option bound to property '{0}' shadows the conventional version option via '{1}'. Consider choosing a different identifier for your option.",
            "CliFx",
            DiagnosticSeverity.Warning,
            true
        );
}
