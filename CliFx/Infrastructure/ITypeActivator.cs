using System;
using CliFx.Exceptions;

namespace CliFx.Infrastructure;

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

internal static class TypeActivatorExtensions
{
    extension(ITypeActivator activator)
    {
        public T CreateInstance<T>(Type type)
        {
            if (!typeof(T).IsAssignableFrom(type))
            {
                throw CliFxException.InternalError(
                    $"Type '{type.FullName}' is not assignable to '{typeof(T).FullName}'."
                );
            }

            return (T)activator.CreateInstance(type);
        }
    }
}
