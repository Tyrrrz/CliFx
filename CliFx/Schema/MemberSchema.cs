using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using CliFx.Exceptions;
using CliFx.Extensibility;
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

        private object? ConvertScalar(string? value, Type targetType)
        {
            var formatProvider = CultureInfo.InvariantCulture;

            return 0 switch
            {
                // TODO: Use main type activator here
                // Custom converter
                _ when ConverterType is not null =>
                    ConverterType.CreateInstance<IArgumentConverter>().Convert(value),

                // Assignable from string (string, object, etc)
                _ when targetType.IsAssignableFrom(typeof(string)) => value,

                // Boolean (special case)
                _ when targetType == typeof(bool) =>
                    string.IsNullOrWhiteSpace(value) || bool.Parse(value),

                // Primitive types (allow null reference exceptions here, they will be caught)
                _ when targetType.IsConvertible() => System.Convert.ChangeType(value, targetType, formatProvider),
                _ when targetType == typeof(DateTimeOffset) => DateTimeOffset.Parse(value, formatProvider),
                _ when targetType == typeof(TimeSpan) => TimeSpan.Parse(value, formatProvider),

                // Enum types (allow null reference exceptions here, they will be caught)
                _ when targetType.IsEnum => Enum.Parse(targetType, value!, true),

                // Nullable value types
                _ when targetType.TryGetNullableUnderlyingType() is { } nullableUnderlyingType =>
                    !string.IsNullOrWhiteSpace(value)
                        ? ConvertScalar(value, nullableUnderlyingType)
                        : null,

                // String-constructible (FileInfo, DirectoryInfo, etc)
                _ when targetType.GetConstructor(new[] {typeof(string)}) is { } stringConstructor =>
                    stringConstructor.Invoke(new object?[] {value}),

                // String-parseable (Guid, IpAddress, etc)
                _ when targetType.TryGetStaticParseMethod(true) is { } parseMethod =>
                    parseMethod.Invoke(null, new object?[] {value, formatProvider}),
                _ when targetType.TryGetStaticParseMethod() is { } parseMethod =>
                    parseMethod.Invoke(null, new object?[] {value}),

                // No conversion available
                _ => throw CliFxException.CannotConvertToType(this, value, targetType)
            };
        }

        private object ConvertNonScalar(
            IReadOnlyList<string> values,
            Type targetEnumerableType,
            Type targetElementType)
        {
            var array = values
                .Select(v => ConvertScalar(v, targetElementType))
                .ToNonGenericArray(targetElementType);

            var arrayType = array.GetType();

            return 0 switch
            {
                // Assignable from an array (T[], IReadOnlyList<T>, etc)
                _ when targetEnumerableType.IsAssignableFrom(arrayType) => array,

                // Array-constructible (List<T>, HashSet<T>, etc)
                _ when targetEnumerableType.GetConstructor(new[] {arrayType}) is { } arrayConstructor =>
                    arrayConstructor.Invoke(new object?[] {array}),

                // No conversion available
                _ => throw CliFxException.CannotConvertNonScalar(this, values, targetEnumerableType)
            };
        }

        private object? Convert(IReadOnlyList<string> values)
        {
            // Short-circuit implicit members
            if (Property is null)
                return null;

            var targetType = Property.PropertyType;

            try
            {
                // Non-scalar
                if (targetType != typeof(string) &&
                    targetType.TryGetEnumerableUnderlyingType() is { } enumerableUnderlyingType)
                {
                    return ConvertNonScalar(values, targetType, enumerableUnderlyingType);
                }
                // Scalar
                else
                {
                    return values.Count <= 1
                        ? ConvertScalar(values.SingleOrDefault(), targetType)
                        : throw CliFxException.CannotConvertMultipleValuesToNonScalar(this, values);
                }
            }
            catch (Exception ex)
            {
                // TODO
                throw;
                //throw CliFxException.CannotConvertToType(this, value, targetType, ex);
            }
        }

        private void Validate(object? value)
        {
            foreach (var validatorType in ValidatorTypes)
            {
                // TODO: use main type activator here
                var validator = validatorType.CreateInstance<IArgumentValidator>();
                validator.Validate(value);
            }
        }

        public void BindOn(ICommand command, IReadOnlyList<string> values)
        {
            if (Property is null)
                return;

            var value = Convert(values);
            Validate(value);

            Property.SetValue(command, value);
        }

        public void BindOn(ICommand command, params string[] values) =>
            BindOn(command, (IReadOnlyList<string>) values);

        public IReadOnlyList<string> GetValidValues()
        {
            if (Property is null)
                return Array.Empty<string>();

            var underlyingType =
                Property.PropertyType.TryGetNullableUnderlyingType() ??
                Property.PropertyType;

            return 0 switch
            {
                _ when underlyingType.IsEnum => Enum.GetNames(underlyingType),
                _ => Array.Empty<string>()
            };
        }
    }
}