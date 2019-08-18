using System;
using CliFx.Internal;
using CliFx.Models;

namespace CliFx.Services
{
    /// <summary>
    /// Implementation of <see cref="ICommandFactory"/> that uses a factory method to create commands.
    /// </summary>
    public class DelegateCommandFactory : ICommandFactory
    {
        private readonly Func<CommandSchema, ICommand> _factoryMethod;

        /// <summary>
        /// Initializes an instance of <see cref="DelegateCommandFactory"/>.
        /// </summary>
        public DelegateCommandFactory(Func<CommandSchema, ICommand> factoryMethod)
        {
            _factoryMethod = factoryMethod.GuardNotNull(nameof(factoryMethod));
        }

        /// <inheritdoc />
        public ICommand CreateCommand(CommandSchema commandSchema)
        {
            commandSchema.GuardNotNull(nameof(commandSchema));
            return _factoryMethod(commandSchema);
        }
    }
}