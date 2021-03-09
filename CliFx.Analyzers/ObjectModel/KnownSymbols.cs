using CliFx.Analyzers.Utils.Extensions;
using Microsoft.CodeAnalysis;

namespace CliFx.Analyzers.ObjectModel
{
    // TODO: move this out?
    internal static class KnownSymbols
    {
        public const string CliFxConsoleInterface = "CliFx.Infrastructure.IConsole";
        public const string CliFxCommandAttribute = "CliFx.Attributes.CommandAttribute";
        public const string CliFxCommandParameterAttribute = "CliFx.Attributes.CommandParameterAttribute";
        public const string CliFxCommandOptionAttribute = "CliFx.Attributes.CommandOptionAttribute";
        public const string CliFxCommandInterface = "CliFx.ICommand";
        public const string CliFxArgumentValueConverterClass = "CliFx.Extensibility.ArgumentValueConverter<T>";
        public const string CliFxArgumentValueValidatorClass = "CliFx.Extensibility.IArgumentValueValidator";

        public static bool IsSystemString(ISymbol symbol) =>
            symbol.DisplayNameMatches("string") ||
            symbol.DisplayNameMatches("System.String");

        public static bool IsSystemChar(ISymbol symbol) =>
            symbol.DisplayNameMatches("char") ||
            symbol.DisplayNameMatches("System.Char");

        public static bool IsSystemCollectionsGenericIEnumerable(ISymbol symbol) =>
            symbol.DisplayNameMatches("System.Collections.Generic.IEnumerable<T>");

        public static bool IsSystemConsole(ISymbol symbol) =>
            symbol.DisplayNameMatches("System.Console");

        public static bool IsCliFxConsoleInterface(ISymbol symbol) =>
            symbol.DisplayNameMatches(CliFxConsoleInterface);

        public static bool IsCommandAttribute(ISymbol symbol) =>
            symbol.DisplayNameMatches(CliFxCommandAttribute);

        public static bool IsCommandParameterAttribute(ISymbol symbol) =>
            symbol.DisplayNameMatches(CliFxCommandParameterAttribute);

        public static bool IsCommandOptionAttribute(ISymbol symbol) =>
            symbol.DisplayNameMatches(CliFxCommandOptionAttribute);

        public static bool IsCommandInterface(ISymbol symbol) =>
            symbol.DisplayNameMatches(CliFxCommandInterface);

        public static bool IsArgumentValueConverterInterface(ISymbol symbol) =>
            symbol.DisplayNameMatches("CliFx.Extensibility.IArgumentValueConverter");

        public static bool IsArgumentValueValidatorInterface(ISymbol symbol) =>
            symbol.DisplayNameMatches("CliFx.Extensibility.IArgumentValueValidator");
    }
}