using System;
using CliFx.Exceptions;

namespace CliFx.Infrastructure;

/// <summary>
/// Implementation of <see cref="ITypeActivator" /> that instantiates an object
/// by using a predefined function.
/// </summary>
public class DelegateTypeActivator : ITypeActivator
{
    private readonly Func<Type, object> _func;

    /// <summary>
    /// Initializes an instance of <see cref="DelegateTypeActivator" />.
    /// </summary>
    public DelegateTypeActivator(Func<Type, object> func) => _func = func;

    /// <inheritdoc />
    public object CreateInstance(Type type)
    {
        var instance = _func(type);

        if (instance is null)
        {
            throw CliFxException.InternalError(
                $"Failed to create an instance of type `{type.FullName}`, received <null> instead." +
                Environment.NewLine +
                "To fix this, ensure that the provided type activator is configured correctly, as it's not expected to return <null>." +
                Environment.NewLine +
                "If you are relying on a dependency container, this error may indicate that the specified type has not been registered."
            );
        }

        return instance;
    }
}