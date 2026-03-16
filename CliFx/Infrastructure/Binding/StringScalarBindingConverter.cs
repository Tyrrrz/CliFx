namespace CliFx.Infrastructure.Binding;

/// <summary>
/// Converter for activating command inputs bound to properties of type <see cref="string" />.
/// </summary>
public class StringScalarBindingConverter : ScalarBindingConverter<string?>
{
    /// <inheritdoc />
    public override string? Convert(string? rawValue) => rawValue;
}
