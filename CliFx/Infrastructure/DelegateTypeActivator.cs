using System;
using CliFx.Exceptions;

namespace CliFx.Infrastructure;

/// <summary>
/// Implementation of <see cref="ITypeActivator" /> that instantiates an object by using a predefined delegate.
/// </summary>
public class DelegateTypeActivator : ITypeActivator
{
    private readonly Func<Type, object> _createInstance;

    /// <summary>
    /// Initializes an instance of <see cref="DelegateTypeActivator" />.
    /// </summary>
    public DelegateTypeActivator(Func<Type, object> createInstance) =>
        _createInstance = createInstance;

    /// <inheritdoc />
    public object CreateInstance(Type type)
    {
        var instance = _createInstance(type);

        if (instance is null)
        {
            throw CliFxException.InternalError(
                $"""
                Failed to create an instance of type `{type.FullName}`, received <null> instead.
                To fix this, ensure that the provided type activator is configured correctly, as it's not expected to return <null>.
                If you are relying on a dependency container, this error may indicate that the specified type has not been registered.
                """
            );
        }

        return instance;
    }
}
