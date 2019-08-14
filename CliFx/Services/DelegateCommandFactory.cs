using System;
using CliFx.Internal;

namespace CliFx.Services
{
    /// <summary>
    /// Implementation of <see cref="ICommandFactory"/> that uses a factory method to create commands.
    /// </summary>
    public class DelegateCommandFactory : ICommandFactory
    {
        private readonly Func<Type, ICommand> _factoryMethod;

        /// <summary>
        /// Initializes an instance of <see cref="DelegateCommandFactory"/>.
        /// </summary>
        public DelegateCommandFactory(Func<Type, ICommand> factoryMethod)
        {
            _factoryMethod = factoryMethod.GuardNotNull(nameof(factoryMethod));
        }

        /// <inheritdoc />
        public ICommand CreateCommand(Type commandType)
        {
            commandType.GuardNotNull(nameof(commandType));
            return _factoryMethod(commandType);
        }
    }
}