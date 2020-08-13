namespace CliFx.Analyzers
{
    using CliFx.Analyzers.Internal;
    using Microsoft.CodeAnalysis;

    public static class KnownSymbols
    {
        public static bool IsSystemString(ISymbol symbol)
        {
            return symbol.DisplayNameMatches("string") || symbol.DisplayNameMatches("System.String");
        }

        public static bool IsSystemChar(ISymbol symbol)
        {
            return symbol.DisplayNameMatches("char") || symbol.DisplayNameMatches("System.Char");
        }

        public static bool IsSystemCollectionsGenericIEnumerable(ISymbol symbol)
        {
            return symbol.DisplayNameMatches("System.Collections.Generic.IEnumerable<T>");
        }

        public static bool IsSystemConsole(ISymbol symbol)
        {
            return symbol.DisplayNameMatches("System.Console");
        }

        public static bool IsConsoleInterface(ISymbol symbol)
        {
            return symbol.DisplayNameMatches("CliFx.IConsole");
        }

        public static bool IsCommandInterface(ISymbol symbol)
        {
            return symbol.DisplayNameMatches("CliFx.ICommand");
        }

        public static bool IsCommandAttribute(ISymbol symbol)
        {
            return symbol.DisplayNameMatches("CliFx.Attributes.CommandAttribute");
        }

        public static bool IsCommandParameterAttribute(ISymbol symbol)
        {
            return symbol.DisplayNameMatches("CliFx.Attributes.CommandParameterAttribute");
        }

        public static bool IsCommandOptionAttribute(ISymbol symbol)
        {
            return symbol.DisplayNameMatches("CliFx.Attributes.CommandOptionAttribute");
        }
    }
}