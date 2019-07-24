using System;
using CliFx.Models;

namespace CliFx.Services
{
    public class CommandFactory : ICommandFactory
    {
        public ICommand CreateCommand(CommandSchema schema) => (ICommand) Activator.CreateInstance(schema.Type);
    }
}