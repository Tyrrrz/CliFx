using System.Collections.Generic;
using CliFx.Domain;

namespace CliFx.Tests.Internal
{
    internal static class CommandHelper
    {
        public static TCommand ResolveCommand<TCommand>(CommandLineInput input, IReadOnlyDictionary<string, string> environmentVariables)
            where TCommand : ICommand, new()
        {
            var schema = CommandSchema.TryResolve(typeof(TCommand))!;

            var instance = new TCommand();
            schema.Bind(instance, input, environmentVariables);

            return instance;
        }

        public static TCommand ResolveCommand<TCommand>(CommandLineInput input)
            where TCommand : ICommand, new() =>
            ResolveCommand<TCommand>(input, new Dictionary<string, string>());
    }
}