namespace CliFx
{
    /// <summary>
    /// Implements custom conversion logic that maps an argument value to a domain type.
    /// </summary>
    public interface IArgumentValueConverter
    {
        /// <summary>
        /// Converts an input value to object of required type.
        /// </summary>
        public object ConvertFrom(string value);
    }
}
