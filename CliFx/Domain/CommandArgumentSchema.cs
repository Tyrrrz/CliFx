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
        private IReadOnlyList<string>? _validValues;
        public PropertyInfo Property { get; }

        public string? Description { get; }

        public abstract string DisplayName { get; }

        public bool IsScalar => TryGetEnumerableArgumentUnderlyingType() == null;

        public IReadOnlyList<string> GetValidValues() => _validValues ??
            (_validValues = EnumerateValidValues().ToList().AsReadOnly());

        protected CommandArgumentSchema(PropertyInfo property, string? description)
        {
            Property = property;
            Description = description;
        }

        private IEnumerable<string> EnumerateValidValues()
        {
            var propertyType = Property?.PropertyType;

            // Property can actually be null here due to damn it operators 
            // in CommandOptionSchema lines 103 and 106, so we have to check 
            // for now. In such case that it is null, let's end early.
            if (propertyType is null)
            {
                yield break;
            }

            // If 'propertyType' is nullable, this will return a non-null value.
            var underlyingType = propertyType.GetNullableUnderlyingType();

            // If 'propertyType' is nullable, 'underlying' type will be not null.
            if (underlyingType is object)
            {
                // Handle nullable num.
                if (underlyingType.IsEnum)
                {
                    // Reasign so we can do the 'foreach' over the enum values 
                    // only once at the end of the method.
                    propertyType = underlyingType;
                }
            }

            // Handle non-nullable enums or nullable enums that were "unwrapped".
            if (propertyType.IsEnum)
            {
                foreach (var value in Enum.GetValues(propertyType))
                {
                    yield return value.ToString();
                }
            }
        }

        private Type? TryGetEnumerableArgumentUnderlyingType() =>
            Property.PropertyType != typeof(string)
                ? Property.PropertyType.GetEnumerableUnderlyingType()
                : null;

        private object? ConvertScalar(string? value, Type targetType)
        {
            try
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
                        ? ConvertScalar(value, nullableUnderlyingType)
                        : null;

                // String-constructable
                var stringConstructor = GetStringConstructor(targetType);
                if (stringConstructor != null)
                    return stringConstructor.Invoke(new object[] {value!});

                // String-parseable (with format provider)
                var parseMethodWithFormatProvider = GetStaticParseMethodWithFormatProvider(targetType);
                if (parseMethodWithFormatProvider != null)
                    return parseMethodWithFormatProvider.Invoke(null, new object[] {value!, ConversionFormatProvider});

                // String-parseable (without format provider)
                var parseMethod = GetStaticParseMethod(targetType);
                if (parseMethod != null)
                    return parseMethod.Invoke(null, new object[] {value!});
            }
            catch (Exception ex)
            {
                throw CliFxException.CannotConvertToType(this, value, targetType, ex);
            }

            throw CliFxException.CannotConvertToType(this, value, targetType);
        }

        private object ConvertNonScalar(IReadOnlyList<string> values, Type targetEnumerableType, Type targetElementType)
        {
            var array = values
                .Select(v => ConvertScalar(v, targetElementType))
                .ToNonGenericArray(targetElementType);

            var arrayType = array.GetType();

            // Assignable from an array
            if (targetEnumerableType.IsAssignableFrom(arrayType))
                return array;

            // Constructable from an array
            var arrayConstructor = targetEnumerableType.GetConstructor(new[] {arrayType});
            if (arrayConstructor != null)
                return arrayConstructor.Invoke(new object[] {array});

            throw CliFxException.CannotConvertNonScalar(this, values, targetEnumerableType);
        }

        private object? Convert(IReadOnlyList<string> values)
        {
            var targetType = Property.PropertyType;
            var enumerableUnderlyingType = TryGetEnumerableArgumentUnderlyingType();

            // Scalar
            if (enumerableUnderlyingType == null)
            {
                return values.Count <= 1
                    ? ConvertScalar(values.SingleOrDefault(), targetType)
                    : throw CliFxException.CannotConvertMultipleValuesToNonScalar(this, values);
            }
            // Non-scalar
            else
            {
                return ConvertNonScalar(values, targetType, enumerableUnderlyingType);
            }
        }

        public void Inject(ICommand command, IReadOnlyList<string> values) =>
            Property.SetValue(command, Convert(values));

        public void Inject(ICommand command, params string[] values) =>
            Inject(command, (IReadOnlyList<string>)values);
    }

    internal partial class CommandArgumentSchema
    {
        private static readonly IFormatProvider ConversionFormatProvider = CultureInfo.InvariantCulture;

        private static readonly IReadOnlyDictionary<Type, Func<string?, object?>> PrimitiveConverters =
            new Dictionary<Type, Func<string?, object?>>
            {
                [typeof(object)] = v => v,
                [typeof(string)] = v => v,
                [typeof(bool)] = v => string.IsNullOrWhiteSpace(v) || bool.Parse(v),
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
    }
}