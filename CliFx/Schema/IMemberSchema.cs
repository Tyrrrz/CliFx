﻿using System;
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

    internal static class MemberSchemaExtensions
    {
        public static string GetKind(this IMemberSchema memberSchema) => memberSchema switch
        {
            ParameterSchema => "Parameter",
            OptionSchema => "Option",
            _ => throw new ArgumentOutOfRangeException(nameof(memberSchema))
        };
    }
}