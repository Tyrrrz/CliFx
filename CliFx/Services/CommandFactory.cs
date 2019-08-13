using System;

namespace CliFx.Services
{
    /// <summary>
    /// Default implementation of <see cref="ICommandFactory"/>.
    /// </summary>
    public class CommandFactory : ICommandFactory
    {
        /// <inheritdoc />
        public ICommand CreateCommand(Type commandType) => (ICommand) Activator.CreateInstance(commandType);
    }
}