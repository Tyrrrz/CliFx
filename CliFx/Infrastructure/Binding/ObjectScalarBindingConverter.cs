namespace CliFx.Infrastructure.Binding;

/// <summary>
/// Converter for activating command inputs bound to properties of type <see cref="object" />.
/// </summary>
public class ObjectScalarBindingConverter : ScalarBindingConverter<object?>
{
    /// <inheritdoc />
    public override object? Convert(string? rawValue) => rawValue;
}
