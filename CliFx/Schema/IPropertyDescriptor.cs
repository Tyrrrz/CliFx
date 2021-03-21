using System;
using System.Collections.Generic;
using CliFx.Utils.Extensions;

namespace CliFx.Schema
{
    internal interface IPropertyDescriptor
    {
        Type Type { get; }

        object? GetValue(ICommand commandInstance);

        void SetValue(ICommand commandInstance, object? value);

        IReadOnlyList<object?> GetValidValues();
    }

    internal static class PropertyDescriptorExtensions
    {
        public static bool IsScalar(this IPropertyDescriptor propertyDescriptor) =>
            propertyDescriptor.Type == typeof(string) ||
            propertyDescriptor.Type.TryGetEnumerableUnderlyingType() is null;
    }
}