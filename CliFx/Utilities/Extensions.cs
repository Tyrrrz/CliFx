namespace CliFx.Utilities
{
    /// <summary>
    /// Extensions for <see cref="Utilities"/>.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Creates a <see cref="ProgressTicker"/> bound to this console.
        /// </summary>
        public static ProgressTicker CreateProgressTicker(this IConsole console) => new ProgressTicker(console);
    }
}