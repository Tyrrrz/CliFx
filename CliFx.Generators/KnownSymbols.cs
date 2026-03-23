using CliFx.Generators.Binding;

namespace CliFx.Generators;

internal static class KnownSymbols
{
    public static TypeIdentifier CliApplicationBuilder { get; } =
        new("CliFx.CliApplicationBuilder");

    public static TypeIdentifier ICommand { get; } = new("CliFx.ICommand");
    public static TypeIdentifier ICommandWithHelpOption { get; } =
        new("CliFx.ICommandWithHelpOption");
    public static TypeIdentifier ICommandWithVersionOption { get; } =
        new("CliFx.ICommandWithVersionOption");

    public static TypeIdentifier CommandAttribute { get; } = new("CliFx.Binding.CommandAttribute");
    public static TypeIdentifier CommandParameterAttribute { get; } =
        new("CliFx.Binding.CommandParameterAttribute");
    public static TypeIdentifier CommandOptionAttribute { get; } =
        new("CliFx.Binding.CommandOptionAttribute");

    public static TypeIdentifier CommandDescriptor { get; } =
        new("CliFx.Binding.CommandDescriptor");
    public static TypeIdentifier CommandInputDescriptor { get; } =
        new("CliFx.Binding.CommandInputDescriptor");
    public static TypeIdentifier CommandParameterDescriptor { get; } =
        new("CliFx.Binding.CommandParameterDescriptor");
    public static TypeIdentifier CommandOptionDescriptor { get; } =
        new("CliFx.Binding.CommandOptionDescriptor");
    public static TypeIdentifier PropertyDescriptor { get; } =
        new("CliFx.Binding.PropertyDescriptor");

    public static TypeIdentifier InputConverter { get; } = new("CliFx.Activation.InputConverter");
    public static TypeIdentifier InputValidator { get; } = new("CliFx.Activation.InputValidator");
    public static TypeIdentifier StringScalarInputConverter { get; } =
        new("CliFx.Activation.StringScalarInputConverter");
    public static TypeIdentifier ObjectScalarInputConverter { get; } =
        new("CliFx.Activation.ObjectScalarInputConverter");
    public static TypeIdentifier BoolScalarInputConverter { get; } =
        new("CliFx.Activation.BoolScalarInputConverter");
    public static TypeIdentifier DateTimeOffsetScalarInputConverter { get; } =
        new("CliFx.Activation.DateTimeOffsetScalarInputConverter");
    public static TypeIdentifier DateTimeScalarInputConverter { get; } =
        new("CliFx.Activation.DateTimeScalarInputConverter");
    public static TypeIdentifier TimeSpanScalarInputConverter { get; } =
        new("CliFx.Activation.TimeSpanScalarInputConverter");
    public static TypeIdentifier EnumScalarInputConverter { get; } =
        new("CliFx.Activation.EnumScalarInputConverter");
    public static TypeIdentifier NullableScalarInputConverter { get; } =
        new("CliFx.Activation.NullableScalarInputConverter");
    public static TypeIdentifier ConvertibleScalarInputConverter { get; } =
        new("CliFx.Activation.ConvertibleScalarInputConverter");
    public static TypeIdentifier DelegateScalarInputConverter { get; } =
        new("CliFx.Activation.DelegateScalarInputConverter");
    public static TypeIdentifier DelegateSequenceInputConverter { get; } =
        new("CliFx.Activation.DelegateSequenceInputConverter");
    public static TypeIdentifier ArraySequenceInputConverter { get; } =
        new("CliFx.Activation.ArraySequenceInputConverter");
}
