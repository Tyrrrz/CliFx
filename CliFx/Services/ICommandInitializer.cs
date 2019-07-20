using CliFx.Models;

namespace CliFx.Services
{
    public interface ICommandInitializer
    {
        ICommand InitializeCommand(CommandInput input);
    }
}