namespace CliFx.Activation;

/// <summary>
/// Converter for activating command inputs bound to properties of type <see cref="bool" />.
/// </summary>
public class BoolScalarInputConverter : ScalarInputConverter<bool>
{
    /// <inheritdoc />
    public override bool Convert(string? rawValue) =>
        string.IsNullOrWhiteSpace(rawValue) || bool.Parse(rawValue);
}
