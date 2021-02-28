using CliFx.Analyzers.Utils.Extensions;
using Microsoft.CodeAnalysis;

namespace CliFx.Analyzers.ObjectModel
{
    internal static class KnownSymbols
    {
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
            symbol.DisplayNameMatches("CliFx.IConsole");

        public static bool IsCommandInterface(ISymbol symbol) =>
            symbol.DisplayNameMatches("CliFx.ICommand");

        public static bool IsArgumentValueConverterInterface(ISymbol symbol) =>
            symbol.DisplayNameMatches("CliFx.IArgumentValueConverter");

        public static bool IsArgumentValueValidatorInterface(ISymbol symbol) =>
            symbol.DisplayNameMatches("CliFx.IArgumentValueValidator");

        public static bool IsCommandAttribute(ISymbol symbol) =>
            symbol.DisplayNameMatches("CliFx.Attributes.CommandAttribute");

        public static bool IsCommandParameterAttribute(ISymbol symbol) =>
            symbol.DisplayNameMatches("CliFx.Attributes.CommandParameterAttribute");

        public static bool IsCommandOptionAttribute(ISymbol symbol) =>
            symbol.DisplayNameMatches("CliFx.Attributes.CommandOptionAttribute");
    }
}