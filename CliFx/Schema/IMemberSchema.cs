using System;
using System.Collections.Generic;

namespace CliFx.Schema
{
    internal interface IMemberSchema
    {
        IPropertyDescriptor Property { get; }

        Type? ConverterType { get; }

        IReadOnlyList<Type> ValidatorTypes { get; }

        string GetFormattedIdentifier();
    }
}