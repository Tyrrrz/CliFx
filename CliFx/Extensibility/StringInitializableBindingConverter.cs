using System;

namespace CliFx.Extensibility;

/// <summary>
/// Converter for types that can be initialized from a string (e.g., via a static <c>Parse</c> method
/// or a constructor that accepts a <see cref="string" />).
/// </summary>
public sealed class StringInitializableBindingConverter<T>(Func<string, T> factory)
    : BindingConverter<T>
{
    /// <inheritdoc />
    public override T Convert(string? rawValue) => factory(rawValue!);
}
