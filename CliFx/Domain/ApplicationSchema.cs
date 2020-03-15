using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Internal;

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
            var command = TryFindCommand(commandLineInput, out var argumentOffset);
            if (command == null)
            {
                throw new CliFxException(
                    $"Can't find a command that matches arguments [{string.Join(" ", commandLineInput.UnboundArguments)}].");
            }

            var parameterValues = argumentOffset == 0
                ? commandLineInput.UnboundArguments.Select(a => a.Value).ToArray()
                : commandLineInput.UnboundArguments.Skip(argumentOffset).Select(a => a.Value).ToArray();

            return command.CreateInstance(parameterValues, commandLineInput.Options, environmentVariables, activator);
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
                throw new CliFxException(new StringBuilder()
                    .AppendLine($"Command {command.Type.FullName} contains two or more parameters that have the same order ({duplicateOrderGroup.Key}):")
                    .AppendBulletList(duplicateOrderGroup.Select(o => o.Property.Name))
                    .AppendLine()
                    .Append("Parameters in a command must all have unique order.")
                    .ToString());
            }

            var duplicateNameGroup = command.Parameters
                .Where(a => !string.IsNullOrWhiteSpace(a.Name))
                .GroupBy(a => a.Name, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault(g => g.Count() > 1);

            if (duplicateNameGroup != null)
            {
                throw new CliFxException(new StringBuilder()
                    .AppendLine($"Command {command.Type.FullName} contains two or more parameters that have the same name ({duplicateNameGroup.Key}):")
                    .AppendBulletList(duplicateNameGroup.Select(o => o.Property.Name))
                    .AppendLine()
                    .Append("Parameters in a command must all have unique names.").Append(" ")
                    .Append("Comparison is NOT case-sensitive.")
                    .ToString());
            }

            var nonScalarParameters = command.Parameters
                .Where(p => !p.IsScalar)
                .ToArray();

            if (nonScalarParameters.Length > 1)
            {
                throw new CliFxException(new StringBuilder()
                    .AppendLine($"Command [{command.Type.FullName}] contains two or more parameters of an enumerable type:")
                    .AppendBulletList(nonScalarParameters.Select(o => o.Property.Name))
                    .AppendLine()
                    .AppendLine("There can only be one parameter of an enumerable type in a command.")
                    .Append("Note, the string type is not considered enumerable in this context.")
                    .ToString());
            }

            var nonLastNonScalarParameter = command.Parameters
                .OrderByDescending(a => a.Order)
                .Skip(1)
                .LastOrDefault(p => !p.IsScalar);

            if (nonLastNonScalarParameter != null)
            {
                throw new CliFxException(new StringBuilder()
                    .AppendLine($"Command {command.Type.FullName} contains a parameter of an enumerable type which doesn't appear last in order:")
                    .AppendLine($"- {nonLastNonScalarParameter.Property.Name}")
                    .AppendLine()
                    .Append("Parameter of an enumerable type must always come last to avoid ambiguity.")
                    .ToString());
            }
        }

        private static void ValidateOptions(CommandSchema command)
        {
            var duplicateNameGroup = command.Options
                .Where(o => !string.IsNullOrWhiteSpace(o.Name))
                .GroupBy(o => o.Name, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault(g => g.Count() > 1);

            if (duplicateNameGroup != null)
            {
                throw new CliFxException(new StringBuilder()
                    .AppendLine($"Command {command.Type.FullName} contains two or more options that have the same name ({duplicateNameGroup.Key}):")
                    .AppendBulletList(duplicateNameGroup.Select(o => o.Property.Name))
                    .AppendLine()
                    .Append("Options in a command must all have unique names.").Append(" ")
                    .Append("Comparison is NOT case-sensitive.")
                    .ToString());
            }

            var duplicateShortNameGroup = command.Options
                .Where(o => o.ShortName != null)
                .GroupBy(o => o.ShortName)
                .FirstOrDefault(g => g.Count() > 1);

            if (duplicateShortNameGroup != null)
            {
                throw new CliFxException(new StringBuilder()
                    .AppendLine($"Command {command.Type.FullName} contains two or more options that have the same short name ({duplicateShortNameGroup.Key}):")
                    .AppendBulletList(duplicateShortNameGroup.Select(o => o.Property.Name))
                    .AppendLine()
                    .Append("Options in a command must all have unique short names.").Append(" ")
                    .Append("Comparison is case-sensitive.")
                    .ToString());
            }

            var duplicateEnvironmentVariableNameGroup = command.Options
                .Where(o => !string.IsNullOrWhiteSpace(o.EnvironmentVariableName))
                .GroupBy(o => o.EnvironmentVariableName, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault(g => g.Count() > 1);

            if (duplicateEnvironmentVariableNameGroup != null)
            {
                throw new CliFxException(new StringBuilder()
                    .AppendLine($"Command {command.Type.FullName} contains two or more options that have the same environment variable name ({duplicateEnvironmentVariableNameGroup.Key}):")
                    .AppendBulletList(duplicateEnvironmentVariableNameGroup.Select(o => o.Property.Name))
                    .AppendLine()
                    .Append("Options in a command must all have unique environment variable names.").Append(" ")
                    .Append("Comparison is NOT case-sensitive.")
                    .ToString());
            }
        }

        private static void ValidateCommands(IReadOnlyList<CommandSchema> commands)
        {
            if (!commands.Any())
            {
                throw new CliFxException("There are no commands configured for this application.");
            }

            var duplicateNameGroup = commands
                .GroupBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault(g => g.Count() > 1);

            if (duplicateNameGroup != null)
            {
                throw new CliFxException(new StringBuilder()
                    .AppendLine($"Application contains two or more commands that have the same name ({duplicateNameGroup.Key}):")
                    .AppendBulletList(duplicateNameGroup.Select(o => o.Type.FullName))
                    .AppendLine()
                    .Append("Commands must all have unique names. Likewise, there must not be more than one command without a name.").Append(" ")
                    .Append("Comparison is NOT case-sensitive.")
                    .ToString());
            }
        }

        public static ApplicationSchema Resolve(IReadOnlyList<Type> commandTypes)
        {
            var commands = new List<CommandSchema>();

            foreach (var commandType in commandTypes)
            {
                var command = CommandSchema.TryResolve(commandType);
                if (command == null)
                {
                    throw new CliFxException(new StringBuilder()
                        .Append($"Command {commandType.FullName} is not a valid command type.").Append(" ")
                        .AppendLine("In order to be a valid command type it must:")
                        .AppendLine($" - Be annotated with {typeof(CommandAttribute).FullName}")
                        .AppendLine($" - Implement {typeof(ICommand).FullName}")
                        .AppendLine(" - Not be an abstract class")
                        .ToString());
                }

                ValidateParameters(command);
                ValidateOptions(command);

                commands.Add(command);
            }

            ValidateCommands(commands);

            return new ApplicationSchema(commands);
        }
    }
}