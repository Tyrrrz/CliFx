using System;
using CliFx.Exceptions;
using CliFx.Utils.Extensions;

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
                return type.CreateInstance();
            }
            catch (Exception ex)
            {
                throw CliFxException.DefaultActivatorFailed(type, ex);
            }
        }
    }
}