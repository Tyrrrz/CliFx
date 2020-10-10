using System;
using System.ComponentModel;
using System.Globalization;

namespace CliFx.Demo
{
    public abstract class GenericConverter<TSource, TDestination> : TypeConverter
    {
        public abstract TDestination Convert(TSource input);

        public sealed override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(TSource))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) =>
            Convert((TSource)value);
    }
}