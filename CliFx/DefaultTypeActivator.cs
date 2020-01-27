using System;
using System.Text;
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
                throw new CliFxException(new StringBuilder()
                    .Append($"Failed to create an instance of {type.FullName}.").Append(" ")
                    .AppendLine("The type must have a public parameter-less constructor in order to be instantiated by the default activator.")
                    .Append($"To supply a custom activator (for example when using dependency injection), call {nameof(CliApplicationBuilder)}.{nameof(CliApplicationBuilder.UseTypeActivator)}(...).")
                    .ToString(), ex);
            }
        }
    }
}