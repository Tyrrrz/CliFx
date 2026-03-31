using System;
using System.Globalization;

namespace CliFx.Activation;

/// <summary>
/// Converter for activating command inputs bound to properties of type <see cref="Enum" />.
/// </summary>
public class EnumScalarInputConverter<T> : ScalarInputConverter<T>
    where T : struct, Enum
{
    /// <inheritdoc />
    public override T Convert(string? rawValue)
    {
        // If the value is purely numeric, activate by underlying value.
        // Try long first (covers all signed and most unsigned underlying types),
        // then fall back to ulong for unsigned values beyond long.MaxValue.
        if (
            long.TryParse(
                rawValue,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var longValue
            )
        )
            return (T)Enum.ToObject(typeof(T), longValue);

        if (
            ulong.TryParse(
                rawValue,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var ulongValue
            )
        )
            return (T)Enum.ToObject(typeof(T), ulongValue);

        return Enum.Parse<T>(rawValue!, true);
    }
}
