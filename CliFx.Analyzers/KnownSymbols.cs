using CliFx.Analyzers.Internal;
using Microsoft.CodeAnalysis;

namespace CliFx.Analyzers
{
    public static class KnownSymbols
    {
        public static bool IsSystemConsole(ISymbol symbol) => symbol.DisplayNameMatches("System.Console");

        public static bool IsCommandAttribute(ISymbol symbol) => symbol.DisplayNameMatches("CliFx.Attributes.CommandAttribute");

        public static bool IsCommandInterface(ISymbol symbol) => symbol.DisplayNameMatches("CliFx.ICommand");

        public static bool IsConsoleInterface(ISymbol symbol) => symbol.DisplayNameMatches("CliFx.IConsole");
    }
}