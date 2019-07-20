using System;

namespace CliFx.Services
{
    public class TypeActivator : ITypeActivator
    {
        public object Activate(Type type) => Activator.CreateInstance(type);
    }
}