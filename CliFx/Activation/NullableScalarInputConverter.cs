namespace CliFx.Activation;

/// <summary>
/// Converter for activating command inputs bound to properties of type <see cref="System.Nullable{T}" />.
/// </summary>
public class NullableScalarInputConverter<T>(ScalarInputConverter<T> innerConverter)
    : ScalarInputConverter<T?>
    where T : struct
{
    /// <inheritdoc />
    public override T? Convert(string? rawValue) =>
        !string.IsNullOrEmpty(rawValue) ? innerConverter.Convert(rawValue) : null;
}
