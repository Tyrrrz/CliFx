using System;

namespace CliFx.Services
{
    public class CommandFactory : ICommandFactory
    {
        public ICommand CreateCommand(Type commandType) => (ICommand) Activator.CreateInstance(commandType);
    }
}