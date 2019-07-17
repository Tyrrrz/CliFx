using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Services;

namespace CliFx
{
    public class CliApplication : ICliApplication
    {
        private readonly ICommandOptionParser _commandOptionParser;
        private readonly ICommandResolver _commandResolver;

        public CliApplication(ICommandOptionParser commandOptionParser, ICommandResolver commandResolver)
        {
            _commandOptionParser = commandOptionParser;
            _commandResolver = commandResolver;
        }

        public CliApplication()
            : this(new CommandOptionParser(), new CommandResolver(new CommandOptionConverter()))
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
}