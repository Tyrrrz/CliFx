using System;
using System.Text;
using CliFx.Exceptions;

namespace CliFx
{
    /// <summary>
    /// Type activator that uses the specified delegate to instantiate objects.
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
            _func(type) ?? throw new CliFxException(new StringBuilder()
                .Append($"Failed to create an instance of type {type.FullName}, received <null> instead.").Append(" ")
                .Append("Make sure that the provided type activator was configured correctly.").Append(" ")
                .Append("If you are using a dependency container, make sure that this type is registered.")
                .ToString());
    }
}