using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using CliFx.Exceptions;
using CliFx.Internal.Extensions;

namespace CliFx.Domain
{
    internal abstract partial class CommandArgumentSchema
    {
        // Property can be null on built-in arguments (help and version options)
        public PropertyInfo? Property { get; }

        public string? Description { get; }

        public bool IsScalar => TryGetEnumerableArgumentUnderlyingType() == null;

        public Type? ConverterType { get; }

        public Type[] ValidatorTypes { get; }

        protected CommandArgumentSchema(
            PropertyInfo? property,
            string? description,
            Type? converterType,
            Type[] validatorTypes)
        {
            Property = property;
            Description = description;
            ConverterType = converterType;
            ValidatorTypes = validatorTypes;
        }

        private Type? TryGetEnumerableArgumentUnderlyingType() =>
            Property != null && Property.PropertyType != typeof(string)
                ? Property.PropertyType.TryGetEnumerableUnderlyingType()
                : null;

        private object? ConvertScalar(string? value, Type targetType)
        {
            try
            {
                // Custom conversion (always takes highest priority)
                if (ConverterType != null)
                    return ConverterType.CreateInstance<IArgumentValueConverter>().ConvertFrom(value!);

                // No conversion necessary
                if (targetType == typeof(object) || targetType == typeof(string))
                    return value;

                // Bool conversion (special case)
                if (targetType == typeof(bool))
                    return string.IsNullOrWhiteSpace(value) || bool.Parse(value);

                // Primitive conversion
                var primitiveConverter = PrimitiveConverters.GetValueOrDefault(targetType);
                if (primitiveConverter != null && !string.IsNullOrWhiteSpace(value))
                    return primitiveConverter(value);

                // Enum conversion
                if (targetType.IsEnum && !string.IsNullOrWhiteSpace(value))
                    return Enum.Parse(targetType, value, true);

                // Nullable<T> conversion
                var nullableUnderlyingType = targetType.TryGetNullableUnderlyingType();
                if (nullableUnderlyingType != null)
                    return !string.IsNullOrWhiteSpace(value)
                        ? ConvertScalar(value, nullableUnderlyingType)
                        : null;

                // String-constructible conversion
                var stringConstructor = targetType.GetConstructor(new[] {typeof(string)});
                if (stringConstructor != null)
                    return stringConstructor.Invoke(new object[] {value!});

                // String-parseable conversion (with format provider)
                var parseMethodWithFormatProvider = targetType.TryGetStaticParseMethod(true);
                if (parseMethodWithFormatProvider != null)
                    return parseMethodWithFormatProvider.Invoke(null, new object[] {value!, FormatProvider});

                // String-parseable conversion (without format provider)
                var parseMethod = targetType.TryGetStaticParseMethod();
                if (parseMethod != null)
                    return parseMethod.Invoke(null, new object[] {value!});
            }
            catch (Exception ex)
            {
                throw CliFxException.CannotConvertToType(this, value, targetType, ex);
            }

            throw CliFxException.CannotConvertToType(this, value, targetType);
        }

        private object ConvertNonScalar(
            IReadOnlyList<string> values,
            Type targetEnumerableType,
            Type targetElementType)
        {
            var array = values
                .Select(v => ConvertScalar(v, targetElementType))
                .ToNonGenericArray(targetElementType);

            var arrayType = array.GetType();

            // Assignable from an array
            if (targetEnumerableType.IsAssignableFrom(arrayType))
                return array;

            // Constructible from an array
            var arrayConstructor = targetEnumerableType.GetConstructor(new[] {arrayType});
            if (arrayConstructor != null)
                return arrayConstructor.Invoke(new object[] {array});

            throw CliFxException.CannotConvertNonScalar(this, values, targetEnumerableType);
        }

        private object? Convert(IReadOnlyList<string> values)
        {
            // Short-circuit built-in arguments
            if (Property == null)
                return null;

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

        private void Validate(object? value)
        {
            if (value is null)
                return;

            var validators = ValidatorTypes
                .Select(t => t.CreateInstance<IArgumentValueValidator>())
                .ToArray();

            var failedValidations = validators
                .Select(v => v.Validate(value))
                .Where(result => !result.IsValid)
                .ToArray();

            if (failedValidations.Any())
                throw CliFxException.ValidationFailed(this, failedValidations);
        }

        public void BindOn(ICommand command, IReadOnlyList<string> values)
        {
            var value = Convert(values);
            Validate(value);

            Property?.SetValue(command, value);
        }

        public void BindOn(ICommand command, params string[] values) =>
            BindOn(command, (IReadOnlyList<string>) values);

        public IReadOnlyList<string> GetValidValues()
        {
            if (Property == null)
                return Array.Empty<string>();

            var underlyingType =
                Property.PropertyType.TryGetNullableUnderlyingType() ??
                Property.PropertyType;

            // Enum
            if (underlyingType.IsEnum)
                return Enum.GetNames(underlyingType);

            return Array.Empty<string>();
        }
    }

    internal partial class CommandArgumentSchema
    {
        private static readonly IFormatProvider FormatProvider = CultureInfo.InvariantCulture;

        private static readonly IReadOnlyDictionary<Type, Func<string, object?>> PrimitiveConverters =
            new Dictionary<Type, Func<string, object?>>
            {
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