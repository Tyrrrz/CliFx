using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using CliFx.Exceptions;
using CliFx.Internal;
using CliFx.Models;

namespace CliFx.Services
{
    /// <summary>
    /// Default implementation of <see cref="ICommandOptionInputConverter"/>.
    /// </summary>
    public partial class CommandOptionInputConverter : ICommandOptionInputConverter
    {
        private readonly IFormatProvider _formatProvider;

        /// <summary>
        /// Initializes an instance of <see cref="CommandOptionInputConverter"/>.
        /// </summary>
        public CommandOptionInputConverter(IFormatProvider formatProvider)
        {
            _formatProvider = formatProvider.GuardNotNull(nameof(formatProvider));
        }

        /// <summary>
        /// Initializes an instance of <see cref="CommandOptionInputConverter"/>.
        /// </summary>
        public CommandOptionInputConverter()
            : this(CultureInfo.InvariantCulture)
        {
        }

        private object ConvertValue(string value, Type targetType)
        {
            try
            {
                // String or object
                if (targetType == typeof(string) || targetType == typeof(object))
                    return value;

                // Bool
                if (targetType == typeof(bool))
                    return value.IsNullOrWhiteSpace() || bool.Parse(value);

                // Char
                if (targetType == typeof(char))
                    return value.Single();

                // Sbyte
                if (targetType == typeof(sbyte))
                    return sbyte.Parse(value, _formatProvider);

                // Byte
                if (targetType == typeof(byte))
                    return byte.Parse(value, _formatProvider);

                // Short
                if (targetType == typeof(short))
                    return short.Parse(value, _formatProvider);

                // Ushort
                if (targetType == typeof(ushort))
                    return ushort.Parse(value, _formatProvider);

                // Int
                if (targetType == typeof(int))
                    return int.Parse(value, _formatProvider);

                // Uint
                if (targetType == typeof(uint))
                    return uint.Parse(value, _formatProvider);

                // Long
                if (targetType == typeof(long))
                    return long.Parse(value, _formatProvider);

                // Ulong
                if (targetType == typeof(ulong))
                    return ulong.Parse(value, _formatProvider);

                // Float
                if (targetType == typeof(float))
                    return float.Parse(value, _formatProvider);

                // Double
                if (targetType == typeof(double))
                    return double.Parse(value, _formatProvider);

                // Decimal
                if (targetType == typeof(decimal))
                    return decimal.Parse(value, _formatProvider);

                // DateTime
                if (targetType == typeof(DateTime))
                    return DateTime.Parse(value, _formatProvider);

                // DateTimeOffset
                if (targetType == typeof(DateTimeOffset))
                    return DateTimeOffset.Parse(value, _formatProvider);

                // TimeSpan
                if (targetType == typeof(TimeSpan))
                    return TimeSpan.Parse(value, _formatProvider);

                // Enum
                if (targetType.IsEnum)
                    return Enum.Parse(targetType, value, true);

                // Nullable
                var nullableUnderlyingType = Nullable.GetUnderlyingType(targetType);
                if (nullableUnderlyingType != null)
                    return !value.IsNullOrWhiteSpace() ? ConvertValue(value, nullableUnderlyingType) : null;

                // Has a constructor that accepts a single string
                var stringConstructor = GetStringConstructor(targetType);
                if (stringConstructor != null)
                    return stringConstructor.Invoke(new object[] {value});

                // Has a static parse method that accepts a single string and a format provider
                var parseMethodWithFormatProvider = GetStaticParseMethodWithFormatProvider(targetType);
                if (parseMethodWithFormatProvider != null)
                    return parseMethodWithFormatProvider.Invoke(null, new object[] {value, _formatProvider});

                // Has a static parse method that accepts a single string
                var parseMethod = GetStaticParseMethod(targetType);
                if (parseMethod != null)
                    return parseMethod.Invoke(null, new object[] {value});

                throw new InvalidCommandOptionInputException($"Can't convert value [{value}] to type [{targetType}].");
            }
            catch (Exception ex)
            {
                throw new InvalidCommandOptionInputException($"Can't convert value [{value}] to type [{targetType}].", ex);
            }
        }

        /// <inheritdoc />
        public object ConvertOptionInput(CommandOptionInput optionInput, Type targetType)
        {
            optionInput.GuardNotNull(nameof(optionInput));
            targetType.GuardNotNull(nameof(targetType));

            // Single value
            if (optionInput.Values.Count <= 1)
            {
                var value = optionInput.Values.SingleOrDefault();
                return ConvertValue(value, targetType);
            }
            // Multiple values
            else
            {
                // Determine underlying type of elements inside the target collection type
                var underlyingType = targetType.GetEnumerableUnderlyingType() ?? typeof(object);

                // Convert values to that type
                var convertedValues = optionInput.Values.Select(v => ConvertValue(v, underlyingType)).ToNonGenericArray(underlyingType);
                var convertedValuesType = convertedValues.GetType();

                // Assignable from array of values (e.g. T[], IReadOnlyList<T>, IEnumerable<T>)
                if (targetType.IsAssignableFrom(convertedValuesType))
                    return convertedValues;

                // Has a constructor that accepts an array of values (e.g. HashSet<T>, List<T>)
                var arrayConstructor = targetType.GetConstructor(new[] {convertedValuesType});
                if (arrayConstructor != null)
                    return arrayConstructor.Invoke(new object[] {convertedValues});

                throw new InvalidCommandOptionInputException(
                    $"Can't convert sequence of values [{optionInput.Values.JoinToString(", ")}] to type [{targetType}].");
            }
        }
    }

    public partial class CommandOptionInputConverter
    {
        private static ConstructorInfo GetStringConstructor(Type type) => type.GetConstructor(new[] {typeof(string)});

        private static MethodInfo GetStaticParseMethod(Type type) =>
            type.GetMethod("Parse", BindingFlags.Public | BindingFlags.Static, null, new[] {typeof(string)}, null);

        private static MethodInfo GetStaticParseMethodWithFormatProvider(Type type) =>
            type.GetMethod("Parse", BindingFlags.Public | BindingFlags.Static, null, new[] {typeof(string), typeof(IFormatProvider)}, null);
    }
}