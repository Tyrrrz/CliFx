using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;

namespace CliFx.Domain
{
    /// <summary>
    /// Base type for defining custom type converter.
    /// </summary>
    /// <typeparam name="TSource">The Type to convert from.</typeparam>
    /// <typeparam name="TDestination">The Type to convert to.</typeparam>
    public abstract class CustomTypeConverter<TSource, TDestination> : TypeConverter
    {
        /// <summary>
        /// Checks that the provided value is valid for conversion.
        /// </summary>
        /// <param name="input">The object to convert from.</param>
        /// <returns></returns>
        public abstract bool IsValid(TSource input);

        /// <summary>
        /// Converts the input object to object of the specified type.
        /// </summary>
        /// <param name="input">The object to convert from.</param>
        /// <returns></returns>
        public abstract TDestination Convert(TSource input);

        public sealed override bool IsValid(ITypeDescriptorContext context, object value) =>
            IsValid((TSource)value);

        public sealed override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(TSource))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }

        public sealed override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) =>
            Convert((TSource)value)!;

        // Hide unused interface members
        #region TypeConverter

        public sealed override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) => base.CanConvertTo(context, destinationType);

        public sealed override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) => base.ConvertTo(context, culture, value, destinationType);

        public sealed override object CreateInstance(ITypeDescriptorContext context, IDictionary propertyValues) => base.CreateInstance(context, propertyValues);

        public sealed override bool GetCreateInstanceSupported(ITypeDescriptorContext context) => base.GetCreateInstanceSupported(context);

        public sealed override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes) => base.GetProperties(context, value, attributes);

        public sealed override bool GetPropertiesSupported(ITypeDescriptorContext context) => base.GetPropertiesSupported(context);

        public sealed override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context) => base.GetStandardValues(context);

        public sealed override bool GetStandardValuesExclusive(ITypeDescriptorContext context) => base.GetStandardValuesExclusive(context);

        public sealed override bool GetStandardValuesSupported(ITypeDescriptorContext context) => base.GetStandardValuesSupported(context);

        #endregion
    }

    /// <summary>
    /// Base type for defining a custom converter from String to the specified type.
    /// </summary>
    /// <typeparam name="TDestination">The Type to convert to.</typeparam>
    public abstract class StringTypeConverter<TDestination> : CustomTypeConverter<string, TDestination>
    {
        public override bool IsValid(string input) =>
            !string.IsNullOrEmpty(input);
    }
}