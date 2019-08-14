using System;
using CliFx.Internal;

namespace CliFx.Services
{
    /// <summary>
    /// Default implementation of <see cref="ICommandFactory"/>.
    /// </summary>
    public class CommandFactory : ICommandFactory
    {
        /// <inheritdoc />
        public ICommand CreateCommand(Type commandType)
        {
            commandType.GuardNotNull(nameof(commandType));
            return (ICommand) Activator.CreateInstance(commandType);
        }
    }
}