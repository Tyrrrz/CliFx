namespace CliFx.Extensibility
{
    // Used internally to simplify usage from reflection
    internal interface IArgumentConverter
    {
        public object? Convert(string? rawValue);
    }

    /// <summary>
    /// Base type for custom converters.
    /// </summary>
    public abstract class ArgumentConverter<T> : IArgumentConverter
    {
        /// <summary>
        /// Parses value from a raw command line argument.
        /// </summary>
        public abstract T Convert(string? rawValue);

        object? IArgumentConverter.Convert(string? rawValue) => Convert(rawValue);
    }
}