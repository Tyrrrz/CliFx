using CliFx.Models;

namespace CliFx.Services
{
    /// <summary>
    /// Renders help text to the console.
    /// </summary>
    public interface IHelpTextRenderer
    {
        /// <summary>
        /// Renders help text using specified console and source information.
        /// </summary>
        void RenderHelpText(IConsole console, HelpTextSource source);
    }
}