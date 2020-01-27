using System;

namespace CliFx
{
    /// <summary>
    /// Abstraction for a service can initialize objects at runtime.
    /// </summary>
    public interface ITypeActivator
    {
        /// <summary>
        /// Creates an instance of specified type.
        /// </summary>
        object CreateInstance(Type type);
    }
}