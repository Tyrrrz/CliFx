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
                throw CliFxException.DefaultActivatorFailed(type, ex);
            }
        }
    }
}