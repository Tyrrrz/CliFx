namespace CliFx.Generators;

internal static class KnownTypes
{
    public const string CommandLineApplicationBuilderNamespace = "CliFx";
    public const string CommandLineApplicationBuilder =
        CommandLineApplicationBuilderNamespace + ".CommandLineApplicationBuilder";

    public const string ICommand = "CliFx.ICommand";
    public const string ICommandWithHelpOption = "CliFx.ICommandWithHelpOption";
    public const string ICommandWithVersionOption = "CliFx.ICommandWithVersionOption";

    public const string CommandAttribute = "CliFx.Binding.CommandAttribute";
    public const string CommandInputAttribute = "CliFx.Binding.CommandInputAttribute";
    public const string CommandParameterAttribute = "CliFx.Binding.CommandParameterAttribute";
    public const string CommandOptionAttribute = "CliFx.Binding.CommandOptionAttribute";

    public const string CommandDescriptor = "CliFx.Binding.CommandDescriptor";
    public const string CommandInputDescriptor = "CliFx.Binding.CommandInputDescriptor";
    public const string CommandParameterDescriptor = "CliFx.Binding.CommandParameterDescriptor";
    public const string CommandOptionDescriptor = "CliFx.Binding.CommandOptionDescriptor";
    public const string PropertyDescriptor = "CliFx.Binding.PropertyDescriptor";

    public const string InputConverter = "CliFx.Activation.InputConverter";
    public const string SequenceInputConverter = "CliFx.Activation.SequenceInputConverter";
    public const string StringScalarInputConverter = "CliFx.Activation.StringScalarInputConverter";
    public const string BoolScalarInputConverter = "CliFx.Activation.BoolScalarInputConverter";
    public const string EnumScalarInputConverter = "CliFx.Activation.EnumScalarInputConverter";
    public const string NullableScalarInputConverter =
        "CliFx.Activation.NullableScalarInputConverter";
    public const string ConvertibleScalarInputConverter =
        "CliFx.Activation.ConvertibleScalarInputConverter";
    public const string DelegateScalarInputConverter =
        "CliFx.Activation.DelegateScalarInputConverter";
    public const string DelegateSequenceInputConverter =
        "CliFx.Activation.DelegateSequenceInputConverter";
    public const string ArraySequenceInputConverter =
        "CliFx.Activation.ArraySequenceInputConverter";
    public const string InputValidator = "CliFx.Activation.InputValidator";
}
