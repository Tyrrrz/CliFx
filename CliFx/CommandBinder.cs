using CliFx.Infrastructure;

namespace CliFx
{
    internal class CommandBinder
    {
        private readonly ITypeActivator _typeActivator;

        public CommandBinder(ITypeActivator typeActivator)
        {
            _typeActivator = typeActivator;
        }
    }
}