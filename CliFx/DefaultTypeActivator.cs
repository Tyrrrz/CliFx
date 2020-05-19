using System;
using CliFx.Exceptions;

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
                return Activator.CreateInstance(type);
            }
            catch (Exception ex)
            {
                throw CliFxException.DefaultActivatorFailed(type, ex);
            }
        }
    }
}