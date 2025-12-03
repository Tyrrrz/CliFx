using System;
using System.Diagnostics.CodeAnalysis;
using CliFx.Exceptions;

namespace CliFx.Infrastructure;

/// <summary>
/// Abstraction for a service that can instantiate types at run-time.
/// </summary>
public interface ITypeActivator
{
    /// <summary>
    /// Creates an instance of the specified type.
    /// </summary>
    object CreateInstance(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type type
    );
}

internal static class TypeActivatorExtensions
{
    extension(ITypeActivator activator)
    {
        public T CreateInstance<T>(
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
                Type type
        )
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
