namespace CliFx.Activation;

/// <summary>
/// Converter for activating command inputs bound to properties of type <see cref="object" />.
/// </summary>
public class ObjectScalarInputConverter : ScalarInputConverter<object?>
{
    /// <inheritdoc />
    public override object? Convert(string? rawValue) => rawValue;
}
