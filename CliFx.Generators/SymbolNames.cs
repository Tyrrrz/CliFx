namespace CliFx.SourceGeneration;

internal static class SymbolNames
{
    public const string CliFxCommandInterface = "CliFx.ICommand";
    public const string CliFxCommandAttribute = "CliFx.Attributes.CommandAttribute";
    public const string CliFxCommandParameterAttribute =
        "CliFx.Attributes.CommandParameterAttribute";
    public const string CliFxCommandOptionAttribute = "CliFx.Attributes.CommandOptionAttribute";
    public const string CliFxBindingConverterClass =
        "CliFx.Infrastructure.Binding.BindingConverter<T>";
    public const string CliFxBindingValidatorClass =
        "CliFx.Infrastructure.Binding.BindingValidator<T>";
}
