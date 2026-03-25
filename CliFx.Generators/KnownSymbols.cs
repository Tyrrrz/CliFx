using CliFx.Generators.Binding;

namespace CliFx.Generators;

internal static class KnownSymbols
{
    private static TypeIdentifier Create(string fullyQualifiedName) =>
        fullyQualifiedName.LastIndexOf('.') is var index and >= 0
            ? new(
                fullyQualifiedName[..index],
                fullyQualifiedName,
                fullyQualifiedName[(index + 1)..]
            )
            : new(null, fullyQualifiedName, fullyQualifiedName);

    public static TypeIdentifier CommandLineApplicationBuilder { get; } =
        Create("CliFx.CommandLineApplicationBuilder");

    public static TypeIdentifier ICommand { get; } = Create("CliFx.ICommand");
    public static TypeIdentifier ICommandWithHelpOption { get; } =
        Create("CliFx.ICommandWithHelpOption");
    public static TypeIdentifier ICommandWithVersionOption { get; } =
        Create("CliFx.ICommandWithVersionOption");

    public static TypeIdentifier CommandAttribute { get; } =
        Create("CliFx.Binding.CommandAttribute");
    public static TypeIdentifier CommandParameterAttribute { get; } =
        Create("CliFx.Binding.CommandParameterAttribute");
    public static TypeIdentifier CommandOptionAttribute { get; } =
        Create("CliFx.Binding.CommandOptionAttribute");

    public static TypeIdentifier CommandDescriptor { get; } =
        Create("CliFx.Binding.CommandDescriptor");
    public static TypeIdentifier CommandInputDescriptor { get; } =
        Create("CliFx.Binding.CommandInputDescriptor");
    public static TypeIdentifier CommandParameterDescriptor { get; } =
        Create("CliFx.Binding.CommandParameterDescriptor");
    public static TypeIdentifier CommandOptionDescriptor { get; } =
        Create("CliFx.Binding.CommandOptionDescriptor");
    public static TypeIdentifier PropertyDescriptor { get; } =
        Create("CliFx.Binding.PropertyDescriptor");

    public static TypeIdentifier InputConverter { get; } =
        Create("CliFx.Activation.InputConverter");
    public static TypeIdentifier SequenceInputConverter { get; } =
        Create("CliFx.Activation.SequenceInputConverter");
    public static TypeIdentifier StringScalarInputConverter { get; } =
        Create("CliFx.Activation.StringScalarInputConverter");
    public static TypeIdentifier ObjectScalarInputConverter { get; } =
        Create("CliFx.Activation.ObjectScalarInputConverter");
    public static TypeIdentifier BoolScalarInputConverter { get; } =
        Create("CliFx.Activation.BoolScalarInputConverter");
    public static TypeIdentifier DateTimeOffsetScalarInputConverter { get; } =
        Create("CliFx.Activation.DateTimeOffsetScalarInputConverter");
    public static TypeIdentifier DateTimeScalarInputConverter { get; } =
        Create("CliFx.Activation.DateTimeScalarInputConverter");
    public static TypeIdentifier TimeSpanScalarInputConverter { get; } =
        Create("CliFx.Activation.TimeSpanScalarInputConverter");
    public static TypeIdentifier EnumScalarInputConverter { get; } =
        Create("CliFx.Activation.EnumScalarInputConverter");
    public static TypeIdentifier NullableScalarInputConverter { get; } =
        Create("CliFx.Activation.NullableScalarInputConverter");
    public static TypeIdentifier ConvertibleScalarInputConverter { get; } =
        Create("CliFx.Activation.ConvertibleScalarInputConverter");
    public static TypeIdentifier DelegateScalarInputConverter { get; } =
        Create("CliFx.Activation.DelegateScalarInputConverter");
    public static TypeIdentifier DelegateSequenceInputConverter { get; } =
        Create("CliFx.Activation.DelegateSequenceInputConverter");
    public static TypeIdentifier ArraySequenceInputConverter { get; } =
        Create("CliFx.Activation.ArraySequenceInputConverter");
    public static TypeIdentifier InputValidator { get; } =
        Create("CliFx.Activation.InputValidator");
}
