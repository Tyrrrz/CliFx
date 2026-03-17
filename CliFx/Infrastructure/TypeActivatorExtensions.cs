using System;
using System.Diagnostics.CodeAnalysis;
using CliFx.Binding;

namespace CliFx.Infrastructure;

internal static class TypeActivatorExtensions
{
    extension(ITypeActivator activator)
    {
        public T CreateInstance<T>(
            [DynamicallyAccessedMembers(
                DynamicallyAccessedMemberTypes.PublicParameterlessConstructor
            )]
                Type type
        )
        {
            if (!typeof(T).IsAssignableFrom(type))
            {
                throw CliFxException.InternalError(
                    $"Type '{type.FullName}' is not assignable to '{typeof(T).FullName}'."
                );
            }

            return (T)activator.CreateInstance(type);
        }

        public ICommand CreateInstance(CommandDescriptor command) =>
            activator.CreateInstance<ICommand>(command.Type);
    }
}
