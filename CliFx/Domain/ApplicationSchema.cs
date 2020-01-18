using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CliFx.Attributes;
using CliFx.Exceptions;

#if NET45 || NETSTANDARD2_0
using CliFx.Internal;
#endif

namespace CliFx.Domain
{
    internal partial class ApplicationSchema
    {
        public IReadOnlyList<CommandSchema> Commands { get; }

        public ApplicationSchema(IReadOnlyList<CommandSchema> commands)
        {
            Commands = commands;
        }

        public CommandSchema? TryFindCommandSchema(CommandLineInput input)
        {
            for (var i = input.Arguments.Count - 1; i >= 0; i--)
            {
                var potentialCommandName = string.Join(" ", input.Arguments.Take(i));

                var matchingCommandSchema = Commands.FirstOrDefault(c => c.MatchesName(potentialCommandName));

                if (matchingCommandSchema != null)
                    return matchingCommandSchema;
            }

            return null;
        }

        public ICommand? TryInitializeCommand(
            CommandLineInput input,
            IReadOnlyDictionary<string, string> environmentVariables,
            ITypeActivator activator)
        {
            for (var i = input.Arguments.Count - 1; i >= 0; i--)
            {
                var potentialCommandName = string.Join(" ", input.Arguments.Take(i));

                var matchingCommandSchema = Commands.FirstOrDefault(c => c.MatchesName(potentialCommandName));

                if (matchingCommandSchema != null)
                {
                    var target = (ICommand) activator.CreateInstance(matchingCommandSchema.Type);

                    var parameterInputs = input.Arguments.Skip(i + 1).ToArray();
                    matchingCommandSchema.Project(target, parameterInputs, input.Options, environmentVariables);
                }
            }

            return null;
        }
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
                    .AppendLine($"Command type [{command.Type.FullName}] has two or more parameters that have the same order ({duplicateOrderGroup.Key}):")
                    .AppendJoin(", ", duplicateOrderGroup.Select(o => $"[{o.Property.Name}]")).AppendLine(".")
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
                    .AppendLine($"Command type [{command.Type.FullName}] has two or more parameters that have the same name ({duplicateNameGroup.Key}):")
                    .AppendJoin(", ", duplicateNameGroup.Select(o => $"[{o.Property.Name}]")).AppendLine(".")
                    .AppendLine("Parameters in a command must all have unique names.")
                    .Append("Comparison is NOT case-sensitive.")
                    .ToString());
            }

            var nonScalarParameters = command.Parameters
                .Where(p => !p.IsScalar)
                .ToArray();

            if (nonScalarParameters.Length > 1)
            {
                throw new CliFxException(new StringBuilder()
                    .AppendLine($"Command type [{command.Type.FullName}] has two or more parameters of an enumerable type:")
                    .AppendJoin(", ", nonScalarParameters.Select(o => $"[{o.Property.Name}]")).AppendLine(".")
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
                    .AppendLine($"Command type [{command.Type.FullName}] has a parameter of an enumerable type which doesn't appear last in order:")
                    .AppendLine($"[{nonLastNonScalarParameter.Property.Name}].")
                    .Append("Parameter of an enumerable type must always come last in a command.")
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
                    .AppendLine($"Command type [{command.Type.FullName}] has two or more options that have the same name ({duplicateNameGroup.Key}):")
                    .AppendJoin(", ", duplicateNameGroup.Select(o => $"[{o.Property.Name}]")).AppendLine(".")
                    .AppendLine("Options in a command must all have unique names.")
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
                    .AppendLine($"Command type [{command.Type.FullName}] has two or more options that have the same short name ({duplicateShortNameGroup.Key}):")
                    .AppendJoin(", ", duplicateShortNameGroup.Select(o => $"[{o.Property.Name}]")).AppendLine(".")
                    .AppendLine("Options in a command must all have unique short names.")
                    .Append("Comparison is case-sensitive.")
                    .ToString());
            }
        }

        private static void ValidateCommands(IReadOnlyList<CommandSchema> commands)
        {
            var duplicateNameGroup = commands
                .GroupBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault(g => g.Count() > 1);

            if (duplicateNameGroup != null)
            {
                throw new CliFxException(new StringBuilder()
                    .AppendLine($"There are two or more commands that have the same name ({duplicateNameGroup.Key}):")
                    .AppendJoin(", ", duplicateNameGroup.Select(o => $"[{o.Type.FullName}]")).AppendLine(".")
                    .AppendLine("Commands must all have unique names. Likewise, there must not be more than one command without a name.")
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
                        .AppendLine($"Type [{commandType.Name}] is not a valid command type.")
                        .Append($"Make sure it implements {typeof(ICommand).FullName}, is marked with {typeof(CommandAttribute)}, and is not an abstract class.")
                        .ToString());
                }

                ValidateParameters(command);
                ValidateOptions(command);

                commands.Add(command);
            }

            ValidateCommands(commands);

            return new ApplicationSchema(commands);
        }

        public static ApplicationSchema Resolve(params Type[] commandTypes) =>
            Resolve((IReadOnlyList<Type>) commandTypes);
    }
}