namespace CliFx.Analyzers.ObjectModel
{
    internal static class SymbolNames
    {
        public const string CliFxCommandInterface = "CliFx.ICommand";
        public const string CliFxCommandAttribute = "CliFx.Attributes.CommandAttribute";
        public const string CliFxCommandParameterAttribute = "CliFx.Attributes.CommandParameterAttribute";
        public const string CliFxCommandOptionAttribute = "CliFx.Attributes.CommandOptionAttribute";
        public const string CliFxConsoleInterface = "CliFx.Infrastructure.IConsole";
        public const string CliFxArgumentValueConverterInterface = "CliFx.Extensibility.IArgumentValueConverter";
        public const string CliFxArgumentValueConverterClass = "CliFx.Extensibility.ArgumentValueConverter<T>";
        public const string CliFxArgumentValueValidatorInterface = "CliFx.Extensibility.IArgumentValueValidator";
        public const string CliFxArgumentValueValidatorClass = "CliFx.Extensibility.ArgumentValueValidator<T>";
    }
}