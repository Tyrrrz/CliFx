namespace CliFx.Activation;

/// <summary>
/// Converter for activating command inputs bound to properties of type <see cref="string" />.
/// </summary>
public class StringScalarInputConverter : ScalarInputConverter<string?>
{
    /// <inheritdoc />
    public override string? Convert(string? rawValue) => rawValue;
}
