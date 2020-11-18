namespace CliFx
{
    /// <summary>
    /// Implements custom conversion logic that maps an argument value to a domain type.
    /// </summary>
    /// <remarks>
    /// This type is public for legacy reasons.
    /// Please derive from <see cref="ArgumentValueConverter{T}"/> instead.
    /// </remarks>
    public interface IArgumentValueConverter
    {
        /// <summary>
        /// Converts an input value to object of required type.
        /// </summary>
        public object ConvertFrom(string value);
    }

    /// <summary>
    /// A base type for custom argument converters.
    /// </summary>
    public abstract class ArgumentValueConverter<T> : IArgumentValueConverter
    {
        /// <summary>
        /// Converts an input value to object of required type.
        /// </summary>
        public abstract T ConvertFrom(string value);

        object IArgumentValueConverter.ConvertFrom(string value) => ConvertFrom(value)!;
    }
}