using System;
using System.Collections.Generic;
using System.Reflection;
using CliFx.Utils.Extensions;

namespace CliFx.Schema;

internal class BindablePropertyDescriptor(PropertyInfo property) : IPropertyDescriptor
{
    public Type Type => property.PropertyType;

    public object? GetValue(ICommand commandInstance) => property.GetValue(commandInstance);

    public void SetValue(ICommand commandInstance, object? value) =>
        property.SetValue(commandInstance, value);

    public IReadOnlyList<object?> GetValidValues()
    {
        static Type GetUnderlyingType(Type type)
        {
            var enumerableUnderlyingType = type.TryGetEnumerableUnderlyingType();
            if (enumerableUnderlyingType is not null)
                return GetUnderlyingType(enumerableUnderlyingType);

            var nullableUnderlyingType = type.TryGetNullableUnderlyingType();
            if (nullableUnderlyingType is not null)
                return GetUnderlyingType(nullableUnderlyingType);

            return type;
        }

        var underlyingType = GetUnderlyingType(Type);

        // We can only get valid values for enums
        if (underlyingType.IsEnum)
            return Enum.GetNames(underlyingType);

        return Array.Empty<object?>();
    }
}
