using System;
using System.ComponentModel;
using System.Globalization;

namespace CliFx.Domain
{
    /// <summary>
    /// Base type for defining custom type converter.
    /// </summary>
    /// <typeparam name="TSource">The Type to convert from.</typeparam>
    /// <typeparam name="TDestination">The Type to convert to.</typeparam>
    public abstract class TypeConverter<TSource, TDestination> : TypeConverter
    {
        /// <summary>
        /// Converts the input object to object of the specified type.
        /// </summary>
        /// <param name="input">The object to convert.</param>
        /// <returns></returns>
        public abstract TDestination Convert(TSource input);

        public sealed override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(TSource))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }

        public sealed override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) =>
            Convert((TSource)value);
    }

    /// <summary>
    /// Base type for defining a custom converter from String to the specified type.
    /// </summary>
    /// <typeparam name="TDestination">The Type to convert to.</typeparam>
    public abstract class StringTypeConverter<TDestination> : TypeConverter<string, TDestination>
    { }
}