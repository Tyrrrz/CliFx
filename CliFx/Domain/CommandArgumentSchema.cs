using System;
using System.Collections;
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

        public bool IsScalar => TryGetEnumerableArgumentUnderlyingType() == null;

        protected CommandArgumentSchema(PropertyInfo property, string? description)
        {
            Property = property;
            Description = description;
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
                var stringConstructor = targetType.GetConstructor(new[] {typeof(string)});
                if (stringConstructor != null)
                    return stringConstructor.Invoke(new object[] {value!});

                // String-parseable (with format provider)
                var parseMethodWithFormatProvider = targetType.GetStaticParseMethod(true);
                if (parseMethodWithFormatProvider != null)
                    return parseMethodWithFormatProvider.Invoke(null, new object[] {value!, FormatProvider});

                // String-parseable (without format provider)
                var parseMethod = targetType.GetStaticParseMethod();
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

        public void BindOn(ICommand command, IReadOnlyList<string> values) =>
            Property.SetValue(command, Convert(values));

        public void BindOn(ICommand command, params string[] values) =>
            BindOn(command, (IReadOnlyList<string>) values);

        public IReadOnlyList<string> GetValidValues()
        {
            var result = new List<string>();

            // Some arguments may have this as null due to a hack that enables built-in options
            // TODO fix this
            if (Property == null)
                return result;

            var underlyingType =
                Property.PropertyType.GetNullableUnderlyingType() ?? Property.PropertyType;

            // Enum
            if (underlyingType.IsEnum)
                result.AddRange(Enum.GetNames(underlyingType));

            return result;
        }

        public string? TryGetDefaultValue(ICommand instance)
        {
            // Some arguments may have this as null due to a hack that enables built-in options
            // TODO fix this
            if (Property == null)
                return null;

            var rawDefaultValue = Property.GetValue(instance);

            if (!(rawDefaultValue is string) && rawDefaultValue is IEnumerable rawDefaultValues)
            {
                var elementType = rawDefaultValues.GetType().GetEnumerableUnderlyingType() ?? typeof(object);

                return elementType.IsToStringOverriden()
                    ? rawDefaultValues
                        .Cast<object?>()
                        .Where(o => o != null)
                        .Select(o => o!.ToFormattableString(FormatProvider).Quote())
                        .JoinToString(" ")
                    : null;
            }

            if (rawDefaultValue != null && !Equals(rawDefaultValue, rawDefaultValue.GetType().GetDefaultValue()))
            {
                return rawDefaultValue.GetType().IsToStringOverriden()
                    ? rawDefaultValue.ToFormattableString(FormatProvider).Quote()
                    : null;
            }

            return null;
        }
    }

    internal partial class CommandArgumentSchema
    {
        private static readonly IFormatProvider FormatProvider = CultureInfo.InvariantCulture;

        private static readonly IReadOnlyDictionary<Type, Func<string?, object?>> PrimitiveConverters =
            new Dictionary<Type, Func<string?, object?>>
            {
                [typeof(object)] = v => v,
                [typeof(string)] = v => v,
                [typeof(bool)] = v => string.IsNullOrWhiteSpace(v) || bool.Parse(v),
                [typeof(char)] = v => v.Single(),
                [typeof(sbyte)] = v => sbyte.Parse(v, FormatProvider),
                [typeof(byte)] = v => byte.Parse(v, FormatProvider),
                [typeof(short)] = v => short.Parse(v, FormatProvider),
                [typeof(ushort)] = v => ushort.Parse(v, FormatProvider),
                [typeof(int)] = v => int.Parse(v, FormatProvider),
                [typeof(uint)] = v => uint.Parse(v, FormatProvider),
                [typeof(long)] = v => long.Parse(v, FormatProvider),
                [typeof(ulong)] = v => ulong.Parse(v, FormatProvider),
                [typeof(float)] = v => float.Parse(v, FormatProvider),
                [typeof(double)] = v => double.Parse(v, FormatProvider),
                [typeof(decimal)] = v => decimal.Parse(v, FormatProvider),
                [typeof(DateTime)] = v => DateTime.Parse(v, FormatProvider),
                [typeof(DateTimeOffset)] = v => DateTimeOffset.Parse(v, FormatProvider),
                [typeof(TimeSpan)] = v => TimeSpan.Parse(v, FormatProvider),
            };
    }
}