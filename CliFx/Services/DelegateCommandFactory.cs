using System;

namespace CliFx.Services
{
    public class DelegateCommandFactory : ICommandFactory
    {
        private readonly Func<Type, ICommand> _factoryMethod;

        public DelegateCommandFactory(Func<Type, ICommand> factoryMethod)
        {
            _factoryMethod = factoryMethod;
        }

        public ICommand CreateCommand(Type commandType) => _factoryMethod(commandType);
    }
}