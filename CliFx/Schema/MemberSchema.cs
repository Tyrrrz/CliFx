using System;
using System.Collections.Generic;
using System.Reflection;
using CliFx.Utils.Extensions;

namespace CliFx.Schema
{
    // TODO: come up with a better name
    // Base type for parameter and option schemas
    internal abstract class MemberSchema
    {
        // Property can be null on the implicit help and version options
        // TODO: do something about it?
        public PropertyInfo? Property { get; }

        public string? Description { get; }

        public bool IsScalar =>
            Property is null ||
            Property.PropertyType == typeof(string) ||
            Property.PropertyType.TryGetEnumerableUnderlyingType() is null;

        public Type? ConverterType { get; }

        public IReadOnlyList<Type> ValidatorTypes { get; }

        protected MemberSchema(
            PropertyInfo? property,
            string? description,
            Type? converterType,
            IReadOnlyList<Type> validatorTypes)
        {
            Property = property;
            Description = description;
            ConverterType = converterType;
            ValidatorTypes = validatorTypes;
        }

        public IReadOnlyList<object?> GetValidValues()
        {
            if (Property is null)
                return Array.Empty<object?>();

            var underlyingType =
                Property.PropertyType.TryGetNullableUnderlyingType() ??
                Property.PropertyType;

            // Only works for enums now
            if (underlyingType.IsEnum)
                return Enum.GetNames(underlyingType);

            return Array.Empty<object?>();
        }
    }
}