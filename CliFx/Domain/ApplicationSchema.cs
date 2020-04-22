using System;
using System.Collections.Generic;
using System.Linq;
using CliFx.Exceptions;

namespace CliFx.Domain
{
    internal partial class ApplicationSchema
    {
        public IReadOnlyList<CommandSchema> Commands { get; }

        public ApplicationSchema(IReadOnlyList<CommandSchema> commands)
        {
            Commands = commands;
        }

        public CommandSchema? TryFindParentCommand(string? childCommandName)
        {
            // Default command has no parent
            if (string.IsNullOrWhiteSpace(childCommandName))
                return null;

            // Try to find the parent command by repeatedly biting off chunks of its name
            var route = childCommandName.Split(' ');
            for (var i = route.Length - 1; i >= 1; i--)
            {
                var potentialParentCommandName = string.Join(" ", route.Take(i));
                var matchingParentCommand = Commands.FirstOrDefault(c => c.MatchesName(potentialParentCommandName));

                if (matchingParentCommand != null)
                    return matchingParentCommand;
            }

            // If there's no parent - fall back to default command
            return Commands.FirstOrDefault(c => c.IsDefault);
        }

        public IReadOnlyList<CommandSchema> GetChildCommands(string? parentCommandName) =>
            !string.IsNullOrWhiteSpace(parentCommandName) || Commands.Any(c => c.IsDefault)
                ? Commands.Where(c => TryFindParentCommand(c.Name)?.MatchesName(parentCommandName) == true).ToArray()
                : Commands.Where(c => !string.IsNullOrWhiteSpace(c.Name) && TryFindParentCommand(c.Name) == null).ToArray();

        // TODO: this out parameter is not a really nice design
        public CommandSchema? TryFindCommand(CommandLineInput commandLineInput, out int argumentOffset)
        {
            // Try to find the command that contains the most of the input arguments in its name
            for (var i = commandLineInput.UnboundArguments.Count; i >= 0; i--)
            {
                var potentialCommandName = string.Join(" ", commandLineInput.UnboundArguments.Take(i));
                var matchingCommand = Commands.FirstOrDefault(c => c.MatchesName(potentialCommandName));

                if (matchingCommand != null)
                {
                    argumentOffset = i;
                    return matchingCommand;
                }
            }

            argumentOffset = 0;
            return Commands.FirstOrDefault(c => c.IsDefault);
        }

        public CommandSchema? TryFindCommand(CommandLineInput commandLineInput) =>
            TryFindCommand(commandLineInput, out _);

        public ICommand InitializeEntryPoint(
            CommandLineInput commandLineInput,
            IReadOnlyDictionary<string, string> environmentVariables,
            ITypeActivator activator)
        {
            var command = TryFindCommand(commandLineInput, out var argumentOffset) ??
                          throw CliFxException.CannotFindMatchingCommand(commandLineInput);

            var parameterInputs = argumentOffset == 0
                ? commandLineInput.UnboundArguments.ToArray()
                : commandLineInput.UnboundArguments.Skip(argumentOffset).ToArray();

            return command.CreateInstance(parameterInputs, commandLineInput.Options, environmentVariables, activator);
        }

        public ICommand InitializeEntryPoint(
            CommandLineInput commandLineInput,
            IReadOnlyDictionary<string, string> environmentVariables) =>
            InitializeEntryPoint(commandLineInput, environmentVariables, new DefaultTypeActivator());

        public ICommand InitializeEntryPoint(CommandLineInput commandLineInput) =>
            InitializeEntryPoint(commandLineInput, new Dictionary<string, string>());

        public override string ToString() => string.Join(Environment.NewLine, Commands);
    }

    internal partial class ApplicationSchema
    {
        private static void ValidateParameters(CommandSchema command)
        {
            var duplicateOrderGroup = command.Parameters
                .GroupBy(a => a.Order)
                .FirstOrDefault(g => g.Count() > 1);

            if (duplicateOrderGroup != null)
            {
                throw CliFxException.CommandParametersDuplicateOrder(
                    command,
                    duplicateOrderGroup.Key,
                    duplicateOrderGroup.ToArray());
            }

            var duplicateNameGroup = command.Parameters
                .Where(a => !string.IsNullOrWhiteSpace(a.Name))
                .GroupBy(a => a.Name!, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault(g => g.Count() > 1);

            if (duplicateNameGroup != null)
            {
                throw CliFxException.CommandParametersDuplicateName(
                    command,
                    duplicateNameGroup.Key,
                    duplicateNameGroup.ToArray());
            }

            var nonScalarParameters = command.Parameters
                .Where(p => !p.IsScalar)
                .ToArray();

            if (nonScalarParameters.Length > 1)
            {
                throw CliFxException.CommandParametersTooManyNonScalar(
                    command,
                    nonScalarParameters);
            }

            var nonLastNonScalarParameter = command.Parameters
                .OrderByDescending(a => a.Order)
                .Skip(1)
                .LastOrDefault(p => !p.IsScalar);

            if (nonLastNonScalarParameter != null)
            {
                throw CliFxException.CommandParametersNonLastNonScalar(
                    command,
                    nonLastNonScalarParameter);
            }
        }

        private static void ValidateOptions(CommandSchema command)
        {
            var noNameGroup = command.Options
                .Where(o => o.ShortName == null && string.IsNullOrWhiteSpace(o.Name))
                .ToArray();

            if (noNameGroup.Any())
            {
                throw CliFxException.CommandOptionsNoName(
                    command,
                    noNameGroup.ToArray());
            }

            var invalidLengthNameGroup = command.Options
                .Where(o => !string.IsNullOrWhiteSpace(o.Name))
                .Where(o => o.Name!.Length <= 1)
                .ToArray();

            if (invalidLengthNameGroup.Any())
            {
                throw CliFxException.CommandOptionsInvalidLengthName(
                    command,
                    invalidLengthNameGroup);
            }

            var duplicateNameGroup = command.Options
                .Where(o => !string.IsNullOrWhiteSpace(o.Name))
                .GroupBy(o => o.Name!, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault(g => g.Count() > 1);

            if (duplicateNameGroup != null)
            {
                throw CliFxException.CommandOptionsDuplicateName(
                    command,
                    duplicateNameGroup.Key,
                    duplicateNameGroup.ToArray());
            }

            var duplicateShortNameGroup = command.Options
                .Where(o => o.ShortName != null)
                .GroupBy(o => o.ShortName!.Value)
                .FirstOrDefault(g => g.Count() > 1);

            if (duplicateShortNameGroup != null)
            {
                throw CliFxException.CommandOptionsDuplicateShortName(
                    command,
                    duplicateShortNameGroup.Key,
                    duplicateShortNameGroup.ToArray());
            }

            var duplicateEnvironmentVariableNameGroup = command.Options
                .Where(o => !string.IsNullOrWhiteSpace(o.EnvironmentVariableName))
                .GroupBy(o => o.EnvironmentVariableName!, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault(g => g.Count() > 1);

            if (duplicateEnvironmentVariableNameGroup != null)
            {
                throw CliFxException.CommandOptionsDuplicateEnvironmentVariableName(
                    command,
                    duplicateEnvironmentVariableNameGroup.Key,
                    duplicateEnvironmentVariableNameGroup.ToArray());
            }
        }

        private static void ValidateCommands(IReadOnlyList<CommandSchema> commands)
        {
            if (!commands.Any())
            {
                throw CliFxException.CommandsNotRegistered();
            }

            var duplicateNameGroup = commands
                .GroupBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault(g => g.Count() > 1);

            if (duplicateNameGroup != null)
            {
                if (!string.IsNullOrWhiteSpace(duplicateNameGroup.Key))
                    throw CliFxException.CommandsDuplicateName(
                        duplicateNameGroup.Key,
                        duplicateNameGroup.ToArray());

                throw CliFxException.CommandsTooManyDefaults(duplicateNameGroup.ToArray());
            }
        }

        public static ApplicationSchema Resolve(IReadOnlyList<Type> commandTypes)
        {
            var commands = new List<CommandSchema>();

            foreach (var commandType in commandTypes)
            {
                var command = CommandSchema.TryResolve(commandType) ??
                              throw CliFxException.InvalidCommandType(commandType);

                ValidateParameters(command);
                ValidateOptions(command);

                commands.Add(command);
            }

            ValidateCommands(commands);

            return new ApplicationSchema(commands);
        }
    }
}