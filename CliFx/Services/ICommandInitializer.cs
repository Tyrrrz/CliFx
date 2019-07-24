using CliFx.Models;

namespace CliFx.Services
{
    public interface ICommandInitializer
    {
        void InitializeCommand(ICommand command, CommandSchema schema, CommandInput input);
    }
}