using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using CliFx.Services;

namespace CliFx
{
    public partial class CliApplication : ICliApplication
    {
        private readonly ICommandResolver _commandResolver;

        public CliApplication(ICommandResolver commandResolver)
        {
            _commandResolver = commandResolver;
        }

        public CliApplication()
            : this(GetDefaultCommandResolver(Assembly.GetCallingAssembly()))
        {
        }

        public async Task<int> RunAsync(IReadOnlyList<string> commandLineArguments)
        {
            // Resolve and execute command
            var command = _commandResolver.ResolveCommand(commandLineArguments);
            var exitCode = await command.ExecuteAsync();

            // TODO: print message if error?

            return exitCode.Value;
        }
    }

    public partial class CliApplication
    {
        private static ICommandResolver GetDefaultCommandResolver(Assembly assembly)
        {
            var typeProvider = TypeProvider.FromAssembly(assembly);
            var commandOptionParser = new CommandOptionParser();
            var commandOptionConverter = new CommandOptionConverter();

            return new CommandResolver(typeProvider, commandOptionParser, commandOptionConverter);
        }
    }
}