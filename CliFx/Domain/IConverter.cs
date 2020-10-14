namespace CliFx.Domain
{
    /// <summary>
    /// Used as an interface for implementing custom parameter/option converters.
    /// </summary>
    public interface IConverter
    {
        /// <summary>
        /// Converts an input value to object of required type.
        /// </summary>
        public object ConvertFrom(string value);
    }
}
