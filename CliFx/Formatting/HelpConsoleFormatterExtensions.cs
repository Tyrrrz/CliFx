using System.Diagnostics.CodeAnalysis;
using CliFx.Infrastructure;

namespace CliFx.Formatting;

internal static class HelpConsoleFormatterExtensions
{
    extension(ConsoleWriter consoleWriter)
    {
        [RequiresUnreferencedCode("Displays default values using runtime type reflection.")]
        public void WriteHelpText(HelpContext context) =>
            new HelpConsoleFormatter(consoleWriter, context).WriteHelpText();
    }

    extension(IConsole console)
    {
        [RequiresUnreferencedCode("Displays default values using runtime type reflection.")]
        public void WriteHelpText(HelpContext context) => console.Output.WriteHelpText(context);
    }
}
