namespace CliFx.Analyzers.ObjectModel
{
    internal static class SymbolNames
    {
        public const string CliFxCommandInterface = "CliFx.ICommand";
        public const string CliFxCommandAttribute = "CliFx.Attributes.CommandAttribute";
        public const string CliFxCommandParameterAttribute = "CliFx.Attributes.CommandParameterAttribute";
        public const string CliFxCommandOptionAttribute = "CliFx.Attributes.CommandOptionAttribute";
        public const string CliFxConsoleInterface = "CliFx.Infrastructure.IConsole";
        public const string CliFxArgumentConverterInterface = "CliFx.Extensibility.IArgumentConverter";
        public const string CliFxArgumentConverterClass = "CliFx.Extensibility.ArgumentConverter<T>";
        public const string CliFxArgumentValidatorInterface = "CliFx.Extensibility.IArgumentValidator";
        public const string CliFxArgumentValidatorClass = "CliFx.Extensibility.ArgumentValidator<T>";
    }
}