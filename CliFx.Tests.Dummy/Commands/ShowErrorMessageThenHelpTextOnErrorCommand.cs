using CliFx.Attributes;
using CliFx.Exceptions;
using System.Threading.Tasks;

namespace CliFx.Tests.Dummy.Commands
{
    /// <summary>
    /// Demos how to show an error message then help text from an organizational command.
    /// </summary>
    [Command("cmd-err", Description = "This is an organizational command. " +
        "I don't do anything except provide a route to my subcommands. " +
        "If you use just me, I print an error message then the help text " +
        "to remind you of my subcommands.")]
    public class ShowErrorMessageThenHelpTextOnCommandExceptionCommand : ICommand
    {
        public ValueTask ExecuteAsync(IConsole console) =>
            throw new CommandException("It is an error to use me without a subcommand. " +
                "Please refer to the help text below for guidance.", showHelp: true);
    }
}