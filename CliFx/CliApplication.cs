using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CliFx.Exceptions;
using CliFx.Internal;
using CliFx.Models;
using CliFx.Services;

namespace CliFx
{
    public partial class CliApplication : ICliApplication
    {
        private readonly IReadOnlyList<Type> _commandTypes;
        private readonly ICommandInputParser _commandInputParser;
        private readonly ICommandSchemaResolver _commandSchemaResolver;
        private readonly ICommandFactory _commandFactory;
        private readonly ICommandInitializer _commandInitializer;
        private readonly ICommandHelpTextBuilder _commandHelpTextBuilder;

        public CliApplication(IReadOnlyList<Type> commandTypes,
            ICommandInputParser commandInputParser, ICommandSchemaResolver commandSchemaResolver,
            ICommandFactory commandFactory, ICommandInitializer commandInitializer, ICommandHelpTextBuilder commandHelpTextBuilder)
        {
            _commandTypes = commandTypes;
            _commandInputParser = commandInputParser;
            _commandSchemaResolver = commandSchemaResolver;
            _commandFactory = commandFactory;
            _commandInitializer = commandInitializer;
            _commandHelpTextBuilder = commandHelpTextBuilder;
        }

        public CliApplication(IReadOnlyList<Type> commandTypes)
            : this(commandTypes,
                new CommandInputParser(), new CommandSchemaResolver(), new CommandFactory(),
                new CommandInitializer(), new CommandHelpTextBuilder())
        {
        }

        public CliApplication()
            : this(GetDefaultCommandTypes())
        {
        }

        public async Task<int> RunAsync(IReadOnlyList<string> commandLineArguments)
        {
            var stdOut = ConsoleWriter.GetStandardOutput();
            var stdErr = ConsoleWriter.GetStandardError();

            try
            {
                var commandInput = _commandInputParser.ParseInput(commandLineArguments);

                var availableCommandSchemas = _commandSchemaResolver.GetCommandSchemas(_commandTypes);
                var matchingCommandSchema = availableCommandSchemas.FindByNameOrNull(commandInput.CommandName);

                // Fail if there are no commands defined
                if (!availableCommandSchemas.Any())
                {
                    stdErr.WriteLine("There are no commands defined in this application.");
                    return -1;
                }

                // Fail if specified a command which is not defined
                if (commandInput.IsCommandSpecified() && matchingCommandSchema == null)
                {
                    stdErr.WriteLine($"Specified command [{commandInput.CommandName}] is not defined.");
                    return -1;
                }

                // Use a stub if command was not specified but there is no default command defined
                if (matchingCommandSchema == null)
                {
                    matchingCommandSchema = _commandSchemaResolver.GetCommandSchema(typeof(StubDefaultCommand));
                }

                // Show version if it was requested without specifying a command
                if (commandInput.IsVersionRequested() && !commandInput.IsCommandSpecified())
                {
                    var versionText = Assembly.GetEntryAssembly()?.GetName().Version.ToString();
                    stdOut.WriteLine(versionText);
                    return 0;
                }

                // Show help if it was requested
                if (commandInput.IsHelpRequested())
                {
                    var helpText = _commandHelpTextBuilder.Build(availableCommandSchemas, matchingCommandSchema);
                    stdOut.WriteLine(helpText);
                    return 0;
                }

                // Create an instance of the command
                var command = matchingCommandSchema.Type == typeof(StubDefaultCommand)
                    ? new StubDefaultCommand(_commandHelpTextBuilder)
                    : _commandFactory.CreateCommand(matchingCommandSchema);

                // Populate command with options according to its schema
                _commandInitializer.InitializeCommand(command, matchingCommandSchema, commandInput);

                // Create context and execute command
                var commandContext = new CommandContext(commandInput, availableCommandSchemas, matchingCommandSchema, stdOut, stdErr);
                await command.ExecuteAsync(commandContext);

                return 0;
            }
            catch (Exception ex)
            {
                stdErr.WriteLine(ex.ToString());
                return ex is CommandErrorException errorException ? errorException.ExitCode : -1;
            }
            finally
            {
                stdOut.Dispose();
                stdErr.Dispose();
            }
        }
    }

    public partial class CliApplication
    {
        private static IReadOnlyList<Type> GetDefaultCommandTypes() =>
            Assembly.GetEntryAssembly()?.ExportedTypes.Where(t => t.Implements(typeof(ICommand))).ToArray() ??
            Type.EmptyTypes;

        private sealed class StubDefaultCommand : ICommand
        {
            private readonly ICommandHelpTextBuilder _commandHelpTextBuilder;

            public StubDefaultCommand(ICommandHelpTextBuilder commandHelpTextBuilder)
            {
                _commandHelpTextBuilder = commandHelpTextBuilder;
            }

            public Task ExecuteAsync(CommandContext context)
            {
                var helpText = _commandHelpTextBuilder.Build(context.AvailableCommandSchemas, context.MatchingCommandSchema);
                context.Output.WriteLine(helpText);
                return Task.CompletedTask;
            }
        }
    }
}