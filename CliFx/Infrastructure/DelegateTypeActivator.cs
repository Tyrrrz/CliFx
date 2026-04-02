using System;
using System.Diagnostics.CodeAnalysis;

namespace CliFx.Infrastructure;

/// <summary>
/// Implementation of <see cref="ITypeInstantiator" /> that instantiates an object by using a predefined delegate.
/// </summary>
public class DelegateTypeInstantiator(Func<Type, object> createInstance) : ITypeInstantiator
{
    /// <inheritdoc />
    public object CreateInstance(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
            Type type
    ) =>
        createInstance(type)
        ?? throw CliFxException.InternalError(
            $"""
            Failed to create an instance of type `{type.FullName}`, received <null> instead.
            To fix this, ensure that the provided type instantiator is configured correctly, as it's not expected to return <null>.
            If you are relying on a dependency container, this error may indicate that the specified type has not been registered.
            """
        );
}
