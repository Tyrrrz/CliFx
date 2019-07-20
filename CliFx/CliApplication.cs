using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Services;

namespace CliFx
{
    public class CliApplication : ICliApplication
    {
        private readonly ICommandInputParser _commandInputParser;
        private readonly ICommandInitializer _commandInitializer;

        public CliApplication(ICommandInputParser commandInputParser, ICommandInitializer commandInitializer)
        {
            _commandInputParser = commandInputParser;
            _commandInitializer = commandInitializer;
        }

        public CliApplication()
            : this(new CommandInputParser(), new CommandInitializer())
        {
        }

        public async Task<int> RunAsync(IReadOnlyList<string> commandLineArguments)
        {
            var input = _commandInputParser.ParseInput(commandLineArguments);
            var command = _commandInitializer.InitializeCommand(input);

            var exitCode = await command.ExecuteAsync();

            return exitCode.Value;
        }
    }
}