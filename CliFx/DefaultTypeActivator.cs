using System;

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
            // TODO: better error on fail with instructions how to set up custom activator
            return Activator.CreateInstance(type);
        }
    }
}