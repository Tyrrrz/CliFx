using System.Collections.Generic;
using CliFx.Analyzers.Utils.Extensions;
using Microsoft.CodeAnalysis;

namespace CliFx.Analyzers.ObjectModel;

internal interface ICommandMemberSymbol
{
    IPropertySymbol Property { get; }

    ITypeSymbol? ConverterType { get; }

    IReadOnlyList<ITypeSymbol> ValidatorTypes { get; }
}

internal static class CommandMemberSymbolExtensions
{
    public static bool IsScalar(this ICommandMemberSymbol member) =>
        member.Property.Type.SpecialType == SpecialType.System_String
        || member.Property.Type.TryGetEnumerableUnderlyingType() is null;
}
