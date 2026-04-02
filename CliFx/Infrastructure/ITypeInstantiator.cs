using System;
using System.Diagnostics.CodeAnalysis;

namespace CliFx.Infrastructure;

/// <summary>
/// Abstraction for a service that can instantiate objects at run time.
/// </summary>
public interface ITypeInstantiator
{
    /// <summary>
    /// Creates an instance of the specified type.
    /// </summary>
    object CreateInstance(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
            Type type
    );
}
