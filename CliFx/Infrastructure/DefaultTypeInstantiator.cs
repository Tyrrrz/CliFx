using System;
using System.Diagnostics.CodeAnalysis;

namespace CliFx.Infrastructure;

/// <summary>
/// Implementation of <see cref="ITypeInstantiator" /> that instantiates a type by using its parameterless constructor.
/// </summary>
public class DefaultTypeInstantiator : ITypeInstantiator
{
    /// <inheritdoc />
    public object CreateInstance(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
            Type type
    )
    {
        try
        {
            return Activator.CreateInstance(type)
                ?? throw CliFxException.InternalError(
                    $"""
                    Failed to create an instance of type `{type.FullName}`, received <null> instead.
                    This may be caused by the type's constructor being trimmed away.
                    """
                );
        }
        // Only catch MemberAccessException because the constructor can throw for its own reasons too
        catch (MemberAccessException ex)
        {
            throw CliFxException.InternalError(
                $"""
                Failed to create an instance of type `{type.FullName}` because an appropriate constructor is not available.
                Default type instantiator is only capable of instantiating a type if it has a public parameterless constructor.
                To fix this, either add a parameterless constructor to the type or configure a custom instantiator for the application.
                """,
                ex
            );
        }
    }
}
