using System;
using System.Diagnostics.CodeAnalysis;
using CliFx.Binding;

namespace CliFx.Infrastructure;

internal static class TypeInstantiatorExtensions
{
    extension(ITypeInstantiator instantiator)
    {
        public T CreateInstance<T>(
            [DynamicallyAccessedMembers(
                DynamicallyAccessedMemberTypes.PublicParameterlessConstructor
            )]
                Type type
        )
        {
            if (!type.IsAssignableTo(typeof(T)))
            {
                throw CliFxException.InternalError(
                    $"Type '{type.FullName}' is not assignable to '{typeof(T).FullName}'."
                );
            }

            return (T)instantiator.CreateInstance(type);
        }

        public ICommand CreateInstance(CommandDescriptor command) =>
            instantiator.CreateInstance<ICommand>(command.Type);
    }
}
