namespace CliFx.Infrastructure.Binding;

/// <summary>
/// Converter for activating command inputs bound to properties of type <see cref="bool" />.
/// </summary>
public class BoolScalarBindingConverter : ScalarBindingConverter<bool>
{
    /// <inheritdoc />
    public override bool Convert(string? rawValue) =>
        string.IsNullOrWhiteSpace(rawValue) || bool.Parse(rawValue);
}
