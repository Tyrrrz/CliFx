using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using CliFx.Services;

namespace CliFx
{
    public partial class CliApplication : ICliApplication
    {
        private readonly ICommandOptionParser _commandOptionParser;
        private readonly ICommandResolver _commandResolver;

        public CliApplication(ICommandOptionParser commandOptionParser, ICommandResolver commandResolver)
        {
            _commandOptionParser = commandOptionParser;
            _commandResolver = commandResolver;
        }

        public CliApplication()
            : this(new CommandOptionParser(), GetDefaultCommandResolver(Assembly.GetCallingAssembly()))
        {
        }

        public async Task<int> RunAsync(IReadOnlyList<string> commandLineArguments)
        {
            var optionSet = _commandOptionParser.ParseOptions(commandLineArguments);
            var command = _commandResolver.ResolveCommand(optionSet);

            var exitCode = await command.ExecuteAsync();

            return exitCode.Value;
        }
    }

    public partial class CliApplication
    {
        private static ICommandResolver GetDefaultCommandResolver(Assembly assembly)
        {
            var typeProvider = TypeProvider.FromAssembly(assembly);
            var commandOptionConverter = new CommandOptionConverter();

            return new CommandResolver(typeProvider, commandOptionConverter);
        }
    }
}