namespace CliFx.Extensibility;

/// <summary>
/// Converter for activating command inputs bound to string properties, without performing any conversion.
/// </summary>
public class NoopBindingConverter : BindingConverter<string>
{
    /// <inheritdoc />
    public override string? Convert(string? rawValue) => rawValue;
}
