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

        return bool.Parse(rawValue);
    }
}
