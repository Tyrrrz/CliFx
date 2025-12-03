using System;
using System.Collections.Generic;
using CliFx.Utils.Extensions;

namespace CliFx.Schema;

internal interface IPropertyDescriptor
{
    Type Type { get; }

    object? GetValue(ICommand commandInstance);

    void SetValue(ICommand commandInstance, object? value);

    IReadOnlyList<object?> GetValidValues();
}

internal static class PropertyDescriptorExtensions
{
    extension(IPropertyDescriptor propertyDescriptor)
    {
        public bool IsScalar() =>
            propertyDescriptor.Type == typeof(string)
            || propertyDescriptor.Type.TryGetEnumerableUnderlyingType() is null;
    }
}
