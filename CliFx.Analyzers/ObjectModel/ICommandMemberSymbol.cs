using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace CliFx.Analyzers.ObjectModel
{
    internal interface ICommandMemberSymbol
    {
        ITypeSymbol? ConverterType { get; }

        IReadOnlyList<ITypeSymbol> ValidatorTypes { get; }
    }
}