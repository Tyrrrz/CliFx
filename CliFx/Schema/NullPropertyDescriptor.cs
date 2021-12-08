using System;
using System.Collections.Generic;

namespace CliFx.Schema;

internal partial class NullPropertyDescriptor : IPropertyDescriptor
{
    public Type Type { get; } = typeof(object);

    public object? GetValue(ICommand commandInstance) => null;

    public void SetValue(ICommand commandInstance, object? value)
    {
    }

    public IReadOnlyList<object?> GetValidValues() => Array.Empty<object?>();
}

internal partial class NullPropertyDescriptor
{
    public static NullPropertyDescriptor Instance { get; } = new();
}