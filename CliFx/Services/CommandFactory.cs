using System;
using CliFx.Internal;
using CliFx.Models;

namespace CliFx.Services
{
    /// <summary>
    /// Default implementation of <see cref="ICommandFactory"/>.
    /// </summary>
    public class CommandFactory : ICommandFactory
    {
        /// <inheritdoc />
        public ICommand CreateCommand(CommandSchema commandSchema)
        {
            commandSchema.GuardNotNull(nameof(commandSchema));
            return (ICommand) Activator.CreateInstance(commandSchema.Type);
        }
    }
}