using System;
using CliFx.Models;

namespace CliFx.Services
{
    /// <summary>
    /// Default implementation of <see cref="ICommandFactory"/>.
    /// </summary>
    public class CommandFactory : ICommandFactory
    {
        /// <inheritdoc />
        public ICommand CreateCommand(ICommandSchema commandSchema) => (ICommand) Activator.CreateInstance(commandSchema.Type);
    }
}