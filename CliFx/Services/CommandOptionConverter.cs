using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using CliFx.Exceptions;
using CliFx.Internal;

namespace CliFx.Services
{
    public class CommandOptionConverter : ICommandOptionConverter
    {
        private readonly IFormatProvider _formatProvider;

        public CommandOptionConverter(IFormatProvider formatProvider)
        {
            _formatProvider = formatProvider;
        }

        public CommandOptionConverter()
            : this(CultureInfo.InvariantCulture)
        {
        }

        public object ConvertOption(string value, Type targetType)
        {
            // String or object
            if (targetType == typeof(string) || targetType == typeof(object))
            {
                return value;
            }

            // Bool
            if (targetType == typeof(bool))
            {
                if (value.IsNullOrWhiteSpace())
                    return true;

                if (bool.TryParse(value, out var result))
                    return result;

                throw new CommandOptionConvertException($"Can't convert value [{value}] to boolean.");
            }

            // Char
            if (targetType == typeof(char))
            {
                if (value.Length == 1)
                    return value[0];

                throw new CommandOptionConvertException(
                    $"Can't convert value [{value}] to char. The value is either empty or longer than one character.");
            }

            // Sbyte
            if (targetType == typeof(sbyte))
            {
                if (sbyte.TryParse(value, NumberStyles.Integer, _formatProvider, out var result))
                    return result;

                throw new CommandOptionConvertException($"Can't convert value [{value}] to sbyte.");
            }

            // Byte
            if (targetType == typeof(byte))
            {
                if (byte.TryParse(value, NumberStyles.Integer, _formatProvider, out var result))
                    return result;

                throw new CommandOptionConvertException($"Can't convert value [{value}] to byte.");
            }

            // Short
            if (targetType == typeof(short))
            {
                if (short.TryParse(value, NumberStyles.Integer, _formatProvider, out var result))
                    return result;

                throw new CommandOptionConvertException($"Can't convert value [{value}] to short.");
            }

            // Ushort
            if (targetType == typeof(ushort))
            {
                if (ushort.TryParse(value, NumberStyles.Integer, _formatProvider, out var result))
                    return result;

                throw new CommandOptionConvertException($"Can't convert value [{value}] to ushort.");
            }

            // Int
            if (targetType == typeof(int))
            {
                if (int.TryParse(value, NumberStyles.Integer, _formatProvider, out var result))
                    return result;

                throw new CommandOptionConvertException($"Can't convert value [{value}] to int.");
            }

            // Uint
            if (targetType == typeof(uint))
            {
                if (uint.TryParse(value, NumberStyles.Integer, _formatProvider, out var result))
                    return result;

                throw new CommandOptionConvertException($"Can't convert value [{value}] to uint.");
            }

            // Long
            if (targetType == typeof(long))
            {
                if (long.TryParse(value, NumberStyles.Integer, _formatProvider, out var result))
                    return result;

                throw new CommandOptionConvertException($"Can't convert value [{value}] to long.");
            }

            // Ulong
            if (targetType == typeof(ulong))
            {
                if (ulong.TryParse(value, NumberStyles.Integer, _formatProvider, out var result))
                    return result;

                throw new CommandOptionConvertException($"Can't convert value [{value}] to ulong.");
            }

            // Float
            if (targetType == typeof(float))
            {
                if (float.TryParse(value, NumberStyles.Float | NumberStyles.AllowThousands, _formatProvider, out var result))
                    return result;

                throw new CommandOptionConvertException($"Can't convert value [{value}] to float.");
            }

            // Double
            if (targetType == typeof(double))
            {
                if (double.TryParse(value, NumberStyles.Float | NumberStyles.AllowThousands, _formatProvider, out var result))
                    return result;

                throw new CommandOptionConvertException($"Can't convert value [{value}] to double.");
            }

            // Decimal
            if (targetType == typeof(decimal))
            {
                if (decimal.TryParse(value, NumberStyles.Number, _formatProvider, out var result))
                    return result;

                throw new CommandOptionConvertException($"Can't convert value [{value}] to decimal.");
            }

            // DateTime
            if (targetType == typeof(DateTime))
            {
                if (DateTime.TryParse(value, _formatProvider, DateTimeStyles.None, out var result))
                    return result;

                throw new CommandOptionConvertException($"Can't convert value [{value}] to DateTime.");
            }

            // DateTimeOffset
            if (targetType == typeof(DateTimeOffset))
            {
                if (DateTimeOffset.TryParse(value, _formatProvider, DateTimeStyles.None, out var result))
                    return result;

                throw new CommandOptionConvertException($"Can't convert value [{value}] to DateTimeOffset.");
            }

            // TimeSpan
            if (targetType == typeof(TimeSpan))
            {
                if (TimeSpan.TryParse(value, _formatProvider, out var result))
                    return result;

                throw new CommandOptionConvertException($"Can't convert value [{value}] to TimeSpan.");
            }

            // Enum
            if (targetType.IsEnum)
            {
                if (Enum.GetNames(targetType).Contains(value, StringComparer.OrdinalIgnoreCase))
                    return Enum.Parse(targetType, value, true);

                throw new CommandOptionConvertException(
                    $"Can't convert value [{value}] to [{targetType}]. The value is not defined on the enum.");
            }

            // Nullable
            var nullableUnderlyingType = Nullable.GetUnderlyingType(targetType);
            if (nullableUnderlyingType != null)
            {
                if (value.IsNullOrWhiteSpace())
                    return null;

                return ConvertOption(value, nullableUnderlyingType);
            }

            // Has a constructor that accepts a single string
            var stringConstructor = targetType.GetConstructor(new[] {typeof(string)});
            if (stringConstructor != null)
            {
                return stringConstructor.Invoke(new object[] {value});
            }

            // Has a static parse method that accepts a single string
            var parseMethod = targetType.GetMethod("Parse", BindingFlags.Public | BindingFlags.Static, null, new[] {typeof(string)}, null);
            if (parseMethod != null)
            {
                return parseMethod.Invoke(null, new object[] {value});
            }

            // Unknown type
            throw new CommandOptionConvertException($"Can't convert value [{value}] to unrecognized type [{targetType}].");
        }
    }
}