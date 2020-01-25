﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using CliFx.Exceptions;
using CliFx.Internal;

namespace CliFx.Domain
{
    internal abstract partial class CommandArgumentSchema
    {
        public PropertyInfo Property { get; }

        public string? Description { get; }

        public bool IsScalar => GetEnumerableArgumentUnderlyingType() != null;

        protected CommandArgumentSchema(PropertyInfo property, string? description)
        {
            Property = property;
            Description = description;
        }

        private Type? GetEnumerableArgumentUnderlyingType() =>
            Property.PropertyType != typeof(string)
                ? Property.PropertyType.GetEnumerableUnderlyingType()
                : null;

        public void Project(ICommand target, string value)
        {
            var convertedValue = Convert(value, Property.PropertyType);
            Property.SetValue(target, convertedValue);
        }

        public void Project(ICommand target, IReadOnlyList<string> values)
        {
            var convertedValue = Convert(values, Property.PropertyType);
            Property.SetValue(target, convertedValue);
        }
    }

    internal partial class CommandArgumentSchema
    {
        private static readonly IFormatProvider ConversionFormatProvider = CultureInfo.InvariantCulture;

        private static readonly IReadOnlyDictionary<Type, Func<string, object>> PrimitiveConverters =
            new Dictionary<Type, Func<string, object>>
            {
                [typeof(object)] = v => v,
                [typeof(string)] = v => v,
                [typeof(char)] = v => v.Single(),
                [typeof(sbyte)] = v => sbyte.Parse(v, ConversionFormatProvider),
                [typeof(byte)] = v => byte.Parse(v, ConversionFormatProvider),
                [typeof(short)] = v => short.Parse(v, ConversionFormatProvider),
                [typeof(ushort)] = v => ushort.Parse(v, ConversionFormatProvider),
                [typeof(int)] = v => int.Parse(v, ConversionFormatProvider),
                [typeof(uint)] = v => uint.Parse(v, ConversionFormatProvider),
                [typeof(long)] = v => long.Parse(v, ConversionFormatProvider),
                [typeof(ulong)] = v => ulong.Parse(v, ConversionFormatProvider),
                [typeof(float)] = v => float.Parse(v, ConversionFormatProvider),
                [typeof(double)] = v => double.Parse(v, ConversionFormatProvider),
                [typeof(decimal)] = v => decimal.Parse(v, ConversionFormatProvider),
                [typeof(DateTime)] = v => DateTime.Parse(v, ConversionFormatProvider),
                [typeof(DateTimeOffset)] = v => DateTimeOffset.Parse(v, ConversionFormatProvider),
                [typeof(TimeSpan)] = v => TimeSpan.Parse(v, ConversionFormatProvider),
            };

        private static ConstructorInfo? GetStringConstructor(Type type) =>
            type.GetConstructor(new[] {typeof(string)});

        private static MethodInfo? GetStaticParseMethod(Type type) =>
            type.GetMethod("Parse",
                BindingFlags.Public | BindingFlags.Static,
                null, new[] {typeof(string)}, null);

        private static MethodInfo? GetStaticParseMethodWithFormatProvider(Type type) =>
            type.GetMethod("Parse",
                BindingFlags.Public | BindingFlags.Static,
                null, new[] {typeof(string), typeof(IFormatProvider)}, null);

        private static object Convert(string value, Type targetType)
        {
            // Primitive
            var primitiveConverter = PrimitiveConverters.GetValueOrDefault(targetType);
            if (primitiveConverter != null)
                return primitiveConverter(value);

            // Enum
            if (targetType.IsEnum)
                return Enum.Parse(targetType, value, true);

            // Nullable
            var nullableUnderlyingType = targetType.GetNullableUnderlyingType();
            if (nullableUnderlyingType != null)
                return !string.IsNullOrWhiteSpace(value)
                    ? Convert(value, nullableUnderlyingType)
                    : null;

            // String-constructable
            var stringConstructor = GetStringConstructor(targetType);
            if (stringConstructor != null)
                return stringConstructor.Invoke(new object[] {value});

            // String-parseable (with format provider)
            var parseMethodWithFormatProvider = GetStaticParseMethodWithFormatProvider(targetType);
            if (parseMethodWithFormatProvider != null)
                return parseMethodWithFormatProvider.Invoke(null, new object[] {value, ConversionFormatProvider});

            // String-parseable (without format provider)
            var parseMethod = GetStaticParseMethod(targetType);
            if (parseMethod != null)
                return parseMethod.Invoke(null, new object[] {value});

            throw new CliFxException(
                $"Can't find a way to convert user input to type [{targetType}]. " +
                "This type is not among the list of types supported by this library.");
        }

        private static object Convert(IReadOnlyList<string> values, Type targetType)
        {
            var targetEnumerableType = targetType.GetEnumerableUnderlyingType();

            var convertedValues = values
                .Select(v => Convert(v, targetEnumerableType))
                .ToNonGenericArray(targetEnumerableType);

            var convertedValuesType = convertedValues.GetType();

            if (targetEnumerableType.IsAssignableFrom(convertedValuesType))
                return convertedValues;

            var arrayConstructor = targetType.GetConstructor(new[] {convertedValuesType});
            if (arrayConstructor != null)
                return arrayConstructor.Invoke(new object[] {convertedValues});

            throw new CliFxException(
                $"Can't convert a sequence of values [{values.JoinToString(", ")}] " +
                $"to type [{targetType}].");
        }
    }
}