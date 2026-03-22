using CliFx.Infrastructure;

namespace CliFx.Formatting;

internal static class HelpConsoleFormatterExtensions
{
    extension(ConsoleWriter consoleWriter)
    {
        public void WriteHelpText(HelpContext context) =>
            new HelpConsoleFormatter(consoleWriter, context).WriteHelpText();
    }

    extension(IConsole console)
    {
        public void WriteHelpText(HelpContext context) => console.Output.WriteHelpText(context);
    }
}
