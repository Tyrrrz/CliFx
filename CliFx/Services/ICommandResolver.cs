using CliFx.Models;

namespace CliFx.Services
{
    public interface ICommandResolver
    {
        Command ResolveCommand(CommandOptionSet optionSet);
    }
}