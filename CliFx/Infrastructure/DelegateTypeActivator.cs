using System;
using CliFx.Exceptions;

namespace CliFx.Infrastructure
{
    /// <summary>
    /// Implementation of <see cref="ITypeActivator"/> that instantiates an object
    /// by using a predefined function.
    /// </summary>
    public class DelegateTypeActivator : ITypeActivator
    {
        private readonly Func<Type, object> _func;

        /// <summary>
        /// Initializes an instance of <see cref="DelegateTypeActivator"/>.
        /// </summary>
        public DelegateTypeActivator(Func<Type, object> func) => _func = func;

        /// <inheritdoc />
        public object CreateInstance(Type type) =>
            _func(type) ?? throw CliFxException.DelegateActivatorReturnedNull(type);
    }
}