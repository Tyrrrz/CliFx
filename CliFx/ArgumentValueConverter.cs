namespace CliFx
{
    // Used internally to simplify usage from reflection
    internal interface IArgumentValueConverter
    {
        public object ConvertFrom(string value);
    }

    /// <summary>
    /// Base type for custom argument converters.
    /// </summary>
    public abstract class ArgumentValueConverter<T> : IArgumentValueConverter
    {
        /// <summary>
        /// Converts the input value to the target type.
        /// </summary>
        public abstract T ConvertFrom(string value);

        object IArgumentValueConverter.ConvertFrom(string value) => ConvertFrom(value)!;
    }
}