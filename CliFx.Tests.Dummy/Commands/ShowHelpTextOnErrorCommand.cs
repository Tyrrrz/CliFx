using CliFx.Attributes;
using CliFx.Exceptions;
using System.Threading.Tasks;

namespace CliFx.Tests.Dummy.Commands
{
    /// <summary>
    /// Demos how to show help text from an organizational command.
    /// </summary>
    [Command("cmd", Description = "This is an organizational command. " +
        "I don't do anything except provide a route to my subcommands. " +
        "If you use just me, I print the help text to remind you of my subcommands.")]
    public class ShowHelpTextOnCommandExceptionCommand : ICommand
    {
        public ValueTask ExecuteAsync(IConsole console) =>
            throw new CommandException(null, showHelp: false);
    }
}