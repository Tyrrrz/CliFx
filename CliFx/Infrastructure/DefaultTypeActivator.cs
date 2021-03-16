using System;
using CliFx.Exceptions;

namespace CliFx.Infrastructure
{
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
            catch (Exception ex)
            {
                throw CliFxException.InternalError($@"
Failed to create an instance of type `{type.FullName}`.
The type must have a public parameterless constructor in order to be instantiated by the default activator.
To fix this, either add a parameterless constructor or configure a custom activator.".Trim(),
                    ex
                );
            }
        }
    }
}