using CliFx.Services;

namespace CliFx.Utilities
{
    /// <summary>
    /// Extensions for <see cref="Utilities"/>.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Creates a progress reporter for this console.
        /// </summary>
        public static ProgressReporter CreateProgressReporter(this IConsole console) => new ProgressReporter(console);
    }
}