using CliFx.Models;

namespace CliFx.Services
{
    public interface ICommandResolver
    {
        ICommand ResolveCommand(CommandOptionSet optionSet);
    }
}