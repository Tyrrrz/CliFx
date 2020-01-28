using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using CliFx.Exceptions;
using CliFx.Internal;

namespace CliFx.Domain
{
    internal abstract partial class CommandArgumentSchema
    {
        public PropertyInfo Property { get; }

        public string? Description { get; }

        public bool IsScalar => GetEnumerableArgumentUnderlyingType() == null;

        protected CommandArgumentSchema(PropertyInfo property, string? description)
        {
            Property = property;
            Description = description;
        }

        private Type? GetEnumerableArgumentUnderlyingType() =>
            Property.PropertyType != typeof(string)
                ? Property.PropertyType.GetEnumerableUnderlyingType()
                : null;

        private object Convert(IReadOnlyList<string> values)
        {
            var targetType = Property.PropertyType;
            var enumerableUnderlyingType = GetEnumerableArgumentUnderlyingType();

            // Scalar
            if (enumerableUnderlyingType == null)
            {
                if (values.Count > 1)
                {
                    throw new CliFxException(new StringBuilder()
                        .AppendLine($"Can't convert a sequence of values [{string.Join(", ", values)}] to type {targetType.FullName}.")
                        .Append("Target type is not enumerable and can't accept more than one value.")
                        .ToString());
                }

                return ConvertScalar(values.SingleOrDefault(), targetType);
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

        private static readonly IReadOnlyDictionary<Type, Func<string, object>> PrimitiveConverters =
            new Dictionary<Type, Func<string?, object>>
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

        private static object ConvertScalar(string? value, Type targetType)
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
                    return stringConstructor.Invoke(new object[] {value});

                // String-parseable (with format provider)
                var parseMethodWithFormatProvider = GetStaticParseMethodWithFormatProvider(targetType);
                if (parseMethodWithFormatProvider != null)
                    return parseMethodWithFormatProvider.Invoke(null, new object[] {value, ConversionFormatProvider});

                // String-parseable (without format provider)
                var parseMethod = GetStaticParseMethod(targetType);
                if (parseMethod != null)
                    return parseMethod.Invoke(null, new object[] {value});
            }
            catch (Exception ex)
            {
                throw new CliFxException(new StringBuilder()
                    .AppendLine($"Failed to convert value '{value ?? "<null>"}' to type {targetType.FullName}.")
                    .Append(ex.Message)
                    .ToString(), ex);
            }

            throw new CliFxException(new StringBuilder()
                .AppendLine($"Can't convert value '{value ?? "<null>"}' to type {targetType.FullName}.")
                .Append("Target type is not supported by CliFx.")
                .ToString());
        }

        private static object ConvertNonScalar(IReadOnlyList<string> values, Type targetEnumerableType, Type targetElementType)
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

            throw new CliFxException(new StringBuilder()
                .AppendLine($"Can't convert a sequence of values [{string.Join(", ", values)}] to type {targetEnumerableType.FullName}.")
                .AppendLine($"Underlying element type is [{targetElementType.FullName}].")
                .Append("Target type must either be assignable from an array or have a public constructor that takes a single array argument.")
                .ToString());
        }
    }
}