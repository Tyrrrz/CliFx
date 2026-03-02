namespace CliFx.Extensibility;

/// <summary>
/// Converter for activating command inputs bound to properties of type <see cref="System.Nullable{T}" />.
/// </summary>
public class NullableBindingConverter<T>(BindingConverter<T> innerConverter) : BindingConverter<T?>
    where T : struct
{
    /// <inheritdoc />
    public override T? Convert(string? rawValue) =>
        !string.IsNullOrWhiteSpace(rawValue) ? innerConverter.Convert(rawValue) : null;
}
