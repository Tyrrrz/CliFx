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

/// <summary>
/// Utilities for creating <see cref="NullableScalarInputConverter{T}" />.
/// </summary>
public static class NullableScalarInputConverter
{
    /// <summary>
    /// Creates a nullable scalar converter.
    /// </summary>
    public static NullableScalarInputConverter<T> Create<T>(ScalarInputConverter<T> innerConverter)
        where T : struct => new(innerConverter);
}
