using System;
using CliFx.Exceptions;

namespace CliFx.Infrastructure
{
    /// <summary>
    /// Implementation of <see cref="ITypeActivator"/> that uses the specified delegate to instantiate objects.
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