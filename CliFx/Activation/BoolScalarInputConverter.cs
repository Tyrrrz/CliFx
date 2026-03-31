using System;

namespace CliFx.Activation;

/// <summary>
/// Converter for activating command inputs bound to properties of type <see cref="bool" />.
/// </summary>
public class BoolScalarInputConverter(
    // This makes sense for options but maybe not for parameters (?)
    bool valueWhenEmpty = true
) : ScalarInputConverter<bool>
{
    /// <inheritdoc />
    public override bool Convert(string? rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
            return valueWhenEmpty;

        if (
            string.Equals(rawValue, "on", StringComparison.OrdinalIgnoreCase)
            || string.Equals(rawValue, "yes", StringComparison.OrdinalIgnoreCase)
        )
            return true;

        if (
            string.Equals(rawValue, "off", StringComparison.OrdinalIgnoreCase)
            || string.Equals(rawValue, "no", StringComparison.OrdinalIgnoreCase)
        )
            return false;

        return bool.Parse(rawValue);
    }
}
