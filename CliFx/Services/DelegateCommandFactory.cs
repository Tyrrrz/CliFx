using System;
using CliFx.Models;

namespace CliFx.Services
{
    /// <summary>
    /// Implementation of <see cref="ICommandFactory"/> that uses a factory method to create commands.
    /// </summary>
    public class DelegateCommandFactory : ICommandFactory
    {
        private readonly Func<ICommandSchema, ICommand> _factoryMethod;

        /// <summary>
        /// Initializes an instance of <see cref="DelegateCommandFactory"/>.
        /// </summary>
        public DelegateCommandFactory(Func<ICommandSchema, ICommand> factoryMethod)
        {
            _factoryMethod = factoryMethod;
        }

        /// <inheritdoc />
        public ICommand CreateCommand(ICommandSchema commandSchema) => _factoryMethod(commandSchema);
    }
}