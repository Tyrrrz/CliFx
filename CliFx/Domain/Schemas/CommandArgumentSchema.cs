using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using CliFx.Exceptions;
using CliFx.Internal.Extensions;

namespace CliFx.Domain
{
    /// <summary>
    /// Abstract command argument schema used in <see cref="CommandParameterSchema"/> and <see cref="CommandOptionSchema"/>
    /// </summary>
    public abstract partial class CommandArgumentSchema
    {
        // Property can be null on built-in arguments (help and version options)
        internal PropertyInfo? Property { get; }

        /// <summary>
        /// Command argument description, which is used in help text.
        /// </summary>
        public string? Description { get; }

        /// <summary>
        /// Whetehr command argument is scalar.
        /// </summary>
        public bool IsScalar => TryGetEnumerableArgumentUnderlyingType() == null;

        /// <summary>
        /// Initializes an instance of <see cref="CommandArgumentSchema"/>.
        /// </summary>
        protected CommandArgumentSchema(PropertyInfo? property, string? description)
        {
            Property = property;
            Description = description;
        }

        private Type? TryGetEnumerableArgumentUnderlyingType()
        {
            return Property != null && Property.PropertyType != typeof(string)
                   ? Property.PropertyType.GetEnumerableUnderlyingType()
                   : null;
        }

        private object? ConvertScalar(string? value, Type targetType)
        {
            try
            {
                // Primitive
                Func<string?, object?>? primitiveConverter = PrimitiveConverters.GetValueOrDefault(targetType);
                if (primitiveConverter != null)
                    return primitiveConverter(value);

                // Enum
                if (targetType.IsEnum)
                    return Enum.Parse(targetType, value, true);

                // Nullable
                Type? nullableUnderlyingType = targetType.GetNullableUnderlyingType();
                if (nullableUnderlyingType != null)
                {
                    return !string.IsNullOrWhiteSpace(value)
                        ? ConvertScalar(value, nullableUnderlyingType)
                        : null;
                }

                // String-constructable
                ConstructorInfo? stringConstructor = targetType.GetConstructor(new[] { typeof(string) });
                if (stringConstructor != null)
                    return stringConstructor.Invoke(new object[] { value! });

                // String-parseable (with format provider)
                MethodInfo? parseMethodWithFormatProvider = targetType.GetStaticParseMethod(true);
                if (parseMethodWithFormatProvider != null)
                    return parseMethodWithFormatProvider.Invoke(null, new object[] { value!, FormatProvider });

                // String-parseable (without format provider)
                MethodInfo? parseMethod = targetType.GetStaticParseMethod();
                if (parseMethod != null)
                    return parseMethod.Invoke(null, new object[] { value! });
            }
            catch (Exception ex)
            {
                throw CliFxException.CannotConvertToType(this, value, targetType, ex);
            }

            throw CliFxException.CannotConvertToType(this, value, targetType);
        }

        private object ConvertNonScalar(IReadOnlyList<string> values, Type targetEnumerableType, Type targetElementType)
        {
            Array array = values.Select(v => ConvertScalar(v, targetElementType))
                                .ToNonGenericArray(targetElementType);

            Type arrayType = array.GetType();

            // Assignable from an array
            if (targetEnumerableType.IsAssignableFrom(arrayType))
                return array;

            // Constructable from an array
            ConstructorInfo? arrayConstructor = targetEnumerableType.GetConstructor(new[] { arrayType });
            if (arrayConstructor != null)
                return arrayConstructor.Invoke(new object[] { array });

            throw CliFxException.CannotConvertNonScalar(this, values, targetEnumerableType);
        }

        private object? Convert(IReadOnlyList<string> values)
        {
            // Short-circuit built-in arguments
            if (Property == null)
                return null;

            Type targetType = Property.PropertyType;
            Type? enumerableUnderlyingType = TryGetEnumerableArgumentUnderlyingType();

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

        internal void BindOn(ICommand command, IReadOnlyList<string> values)
        {
            Property?.SetValue(command, Convert(values));
        }

        internal void BindOn(ICommand command, params string[] values)
        {
            BindOn(command, (IReadOnlyList<string>)values);
        }

        internal IReadOnlyList<string> GetValidValues()
        {
            if (Property == null)
                return Array.Empty<string>();

            Type underlyingType = Property.PropertyType.GetNullableUnderlyingType() ?? Property.PropertyType;

            // Enum
            if (underlyingType.IsEnum)
                return Enum.GetNames(underlyingType);

            return Array.Empty<string>();
        }
    }

    public partial class CommandArgumentSchema
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