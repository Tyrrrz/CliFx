using System;
using System.Diagnostics.CodeAnalysis;
using CliFx.Exceptions;

namespace CliFx.Infrastructure;

/// <summary>
/// Implementation of <see cref="ITypeActivator" /> that instantiates an object by using a predefined delegate.
/// </summary>
public class DelegateTypeActivator(Func<Type, object> createInstance) : ITypeActivator
{
    /// <inheritdoc />
    public object CreateInstance(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type type
    ) =>
        createInstance(type)
        ?? throw CliFxException.InternalError(
            $"""
            Failed to create an instance of type `{type.FullName}`, received <null> instead.
            To fix this, ensure that the provided type activator is configured correctly, as it's not expected to return <null>.
            If you are relying on a dependency container, this error may indicate that the specified type has not been registered.
            """
        );
}
