using System;
using System.Globalization;
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
                return value;
            
            // Bool
            if (targetType == typeof(bool))
                return value.IsNullOrWhiteSpace() || bool.Parse(value);

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
                return !value.IsNullOrWhiteSpace() ? ConvertOption(value, nullableUnderlyingType) : null;

            // All other types
            return Convert.ChangeType(value, targetType, _formatProvider);
        }
    }
}