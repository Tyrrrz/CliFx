using System;
using System.Collections.Generic;
using System.Reflection;
using CliFx.Utils.Extensions;

namespace CliFx.Schema
{
    internal class BindablePropertyDescriptor : IPropertyDescriptor
    {
        private readonly PropertyInfo _property;

        public Type Type => _property.PropertyType;

        public BindablePropertyDescriptor(PropertyInfo property) => _property = property;

        public object? GetValue(ICommand commandInstance) =>
            _property.GetValue(commandInstance);

        public void SetValue(ICommand commandInstance, object? value) =>
            _property.SetValue(commandInstance, value);

        public IReadOnlyList<object?> GetValidValues()
        {
            var underlyingType = Type.TryGetNullableUnderlyingType() ?? Type;

            // We can only get valid values for enums
            if (underlyingType.IsEnum)
                return Enum.GetNames(underlyingType);

            return Array.Empty<object?>();
        }
    }
}