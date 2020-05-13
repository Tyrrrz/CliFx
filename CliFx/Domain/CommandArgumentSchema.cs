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

        public abstract string DisplayName { get; }

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
            Inject(command, (IReadOnlyList<string>) values);
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

    // Default and valid value handling.
    internal partial class CommandArgumentSchema
    {
        public IReadOnlyList<string> GetValidValues()
        {
            var result = new List<string>();

            // Some arguments may have this as null due to a hack that enables built-in options
            if (Property == null)
                return result;

            var underlyingPropertyType =
                Property.PropertyType.GetNullableUnderlyingType() ?? Property.PropertyType;

            // Enum
            if (underlyingPropertyType.IsEnum)
                result.AddRange(Enum.GetNames(underlyingPropertyType));

            return result;
        }

        public string? GetDefaultValue(object? instance)
        {
            if (Property is null || instance is null)
            {
                return null;
            }

            var instanceType = instance.GetType();
            var commandType = Property?.DeclaringType;

            if (instanceType != commandType)
            {
                throw new ArgumentException($"Argument {nameof(instance)} of type {instanceType} " +
                    $"must be the same as this command's type {nameof(commandType)}.");
            }

            var propertyName = Property?.Name;
            string? defaultValue = null;

            if (!string.IsNullOrWhiteSpace(propertyName))
            {
                var instanceProperty = instance.GetType().GetProperty(propertyName);
                var value = instanceProperty.GetValue(instance);
                if (value.OverridesToStringMethod())
                {
                    defaultValue = FormatDefaultString(value.ToString());
                }
                else if (value.IsEnumerable())
                {
                    var values = (IEnumerable)value;
                    var list = new List<string>();
                    foreach (var val in values)
                    {
                        if (val is object)
                        {
                            var finalVal = FormatDefaultString(val.ToString());
                            
                            list.Add(finalVal);
                        }
                    }
                    defaultValue = string.Join(" ", list);
                }
            }

            return defaultValue;
        }

        private static string FormatDefaultString(string value) =>
            value.IsEmptyOrWhiteSpace() ? $"\"{value}\"" : value;
    }
}