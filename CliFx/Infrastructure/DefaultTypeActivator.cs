using System;
using CliFx.Exceptions;

namespace CliFx.Infrastructure;

/// <summary>
/// Implementation of <see cref="ITypeActivator"/> that instantiates an object
/// by using its parameterless constructor.
/// </summary>
public class DefaultTypeActivator : ITypeActivator
{
    /// <inheritdoc />
    public object CreateInstance(Type type)
    {
        try
        {
            return Activator.CreateInstance(type);
        }
        // Only catch MemberAccessException because the constructor can throw for its own reasons too
        catch (MemberAccessException ex)
        {
            throw CliFxException.InternalError(
                $"Failed to create an instance of type `{type.FullName}`, could not access the constructor." +
                Environment.NewLine +
                "Default type activator is only capable of instantiating a type if it has a public parameterless constructor." +
                Environment.NewLine +
                "To fix this, either add a parameterless constructor to the type or configure a custom activator for the application.",
                ex
            );
        }
    }
}