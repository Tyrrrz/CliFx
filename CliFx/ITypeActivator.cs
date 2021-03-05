using System;

namespace CliFx
{
    /// <summary>
    /// Abstraction for a service that can instantiate objects at runtime.
    /// </summary>
    public interface ITypeActivator
    {
        /// <summary>
        /// Creates an instance of the specified type.
        /// </summary>
        object CreateInstance(Type type);
    }
}