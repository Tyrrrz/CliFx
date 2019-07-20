using System;

namespace CliFx.Services
{
    public interface ITypeActivator
    {
        object Activate(Type type);
    }
}