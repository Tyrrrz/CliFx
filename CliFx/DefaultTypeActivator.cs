using System;
using CliFx.Exceptions;
using CliFx.Internal.Extensions;

namespace CliFx
{
    /// <summary>
    /// Type activator that uses the <see cref="Activator"/> class to instantiate objects.
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