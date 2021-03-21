namespace CliFx.Extensibility
{
    // Used internally to simplify usage from reflection
    internal interface IBindingConverter
    {
        object? Convert(string? rawValue);
    }

    /// <summary>
    /// Base type for custom converters.
    /// </summary>
    public abstract class BindingConverter<T> : IBindingConverter
    {
        /// <summary>
        /// Parses value from a raw command line argument.
        /// </summary>
        public abstract T Convert(string? rawValue);

        object? IBindingConverter.Convert(string? rawValue) => Convert(rawValue);
    }
}