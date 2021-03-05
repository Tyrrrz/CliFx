using System;
using System.Collections.Generic;
using System.Linq;
using CliFx.Exceptions;
using CliFx.Utils.Extensions;

namespace CliFx
{
    internal partial class ApplicationSchema
    {
        public IReadOnlyList<CommandSchema> Commands { get; }

        public ApplicationSchema(IReadOnlyList<CommandSchema> commands)
        {
            Commands = commands;
        }

        public IReadOnlyList<string> GetCommandNames() => Commands
            .Select(c => c.Name)
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .ToArray()!;

        public CommandSchema? TryFindDefaultCommand() =>
            Commands.FirstOrDefault(c => c.IsDefault);

        public CommandSchema? TryFindCommand(string? commandName) =>
            Commands.FirstOrDefault(c => c.MatchesName(commandName));

        private IReadOnlyList<CommandSchema> GetDescendantCommands(
            IReadOnlyList<CommandSchema> potentialParentCommands,
            string? parentCommandName) =>
            potentialParentCommands
                // Default commands can't be children of anything
                .Where(c => !string.IsNullOrWhiteSpace(c.Name))
                // Command can't be its own child
                .Where(c => !c.MatchesName(parentCommandName))
                .Where(c =>
                    string.IsNullOrWhiteSpace(parentCommandName) ||
                    c.Name!.StartsWith(parentCommandName + ' ', StringComparison.OrdinalIgnoreCase))
                .ToArray();

        public IReadOnlyList<CommandSchema> GetDescendantCommands(string? parentCommandName) =>
            GetDescendantCommands(Commands, parentCommandName);

        public IReadOnlyList<CommandSchema> GetChildCommands(string? parentCommandName)
        {
            var descendants = GetDescendantCommands(parentCommandName);

            // Filter out descendants of descendants, leave only children
            var result = new List<CommandSchema>(descendants);

            foreach (var descendant in descendants)
            {
                var descendantDescendants = GetDescendantCommands(descendants, descendant.Name);
                result.RemoveRange(descendantDescendants);
            }

            return result;
        }
    }

    internal partial class ApplicationSchema
    {
        private static void ValidateParameters(CommandSchema command)
        {
            var duplicateOrderGroup = command.Parameters
                .GroupBy(a => a.Order)
                .FirstOrDefault(g => g.Count() > 1);

            if (duplicateOrderGroup is not null)
            {
                throw CliFxException.ParametersWithSameOrder(
                    command,
                    duplicateOrderGroup.Key,
                    duplicateOrderGroup.ToArray()
                );
            }

            var duplicateNameGroup = command.Parameters
                .Where(a => !string.IsNullOrWhiteSpace(a.Name))
                .GroupBy(a => a.Name!, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault(g => g.Count() > 1);

            if (duplicateNameGroup is not null)
            {
                throw CliFxException.ParametersWithSameName(
                    command,
                    duplicateNameGroup.Key,
                    duplicateNameGroup.ToArray()
                );
            }

            var nonScalarParameters = command.Parameters
                .Where(p => !p.IsScalar)
                .ToArray();

            if (nonScalarParameters.Length > 1)
            {
                throw CliFxException.TooManyNonScalarParameters(
                    command,
                    nonScalarParameters
                );
            }

            var nonLastNonScalarParameter = command.Parameters
                .OrderByDescending(a => a.Order)
                .Skip(1)
                .LastOrDefault(p => !p.IsScalar);

            if (nonLastNonScalarParameter is not null)
            {
                throw CliFxException.NonLastNonScalarParameter(
                    command,
                    nonLastNonScalarParameter
                );
            }

            var invalidConverterParameters = command.Parameters
                .Where(p => p.ConverterType is not null && !p.ConverterType.Implements(typeof(IArgumentValueConverter)))
                .ToArray();

            if (invalidConverterParameters.Any())
            {
                throw CliFxException.ParametersWithInvalidConverters(
                    command,
                    invalidConverterParameters
                );
            }

            var invalidValidatorParameters = command.Parameters
                .Where(p => !p.ValidatorTypes.All(x => x.Implements(typeof(IArgumentValueValidator))))
                .ToArray();

            if (invalidValidatorParameters.Any())
            {
                throw CliFxException.ParametersWithInvalidValidators(
                    command,
                    invalidValidatorParameters
                );
            }
        }

        private static void ValidateOptions(CommandSchema command)
        {
            var noNameGroup = command.Options
                .Where(o => o.ShortName is null && string.IsNullOrWhiteSpace(o.Name))
                .ToArray();

            if (noNameGroup.Any())
            {
                throw CliFxException.OptionsWithNoName(
                    command,
                    noNameGroup.ToArray()
                );
            }

            var invalidLengthNameGroup = command.Options
                .Where(o => !string.IsNullOrWhiteSpace(o.Name))
                .Where(o => o.Name!.Length <= 1)
                .ToArray();

            if (invalidLengthNameGroup.Any())
            {
                throw CliFxException.OptionsWithInvalidLengthName(
                    command,
                    invalidLengthNameGroup
                );
            }

            var duplicateNameGroup = command.Options
                .Where(o => !string.IsNullOrWhiteSpace(o.Name))
                .GroupBy(o => o.Name!, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault(g => g.Count() > 1);

            if (duplicateNameGroup is not null)
            {
                throw CliFxException.OptionsWithSameName(
                    command,
                    duplicateNameGroup.Key,
                    duplicateNameGroup.ToArray()
                );
            }

            var duplicateShortNameGroup = command.Options
                .Where(o => o.ShortName is not null)
                .GroupBy(o => o.ShortName!.Value)
                .FirstOrDefault(g => g.Count() > 1);

            if (duplicateShortNameGroup is not null)
            {
                throw CliFxException.OptionsWithSameShortName(
                    command,
                    duplicateShortNameGroup.Key,
                    duplicateShortNameGroup.ToArray()
                );
            }

            var duplicateEnvironmentVariableNameGroup = command.Options
                .Where(o => !string.IsNullOrWhiteSpace(o.EnvironmentVariableName))
                .GroupBy(o => o.EnvironmentVariableName!, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault(g => g.Count() > 1);

            if (duplicateEnvironmentVariableNameGroup is not null)
            {
                throw CliFxException.OptionsWithSameEnvironmentVariableName(
                    command,
                    duplicateEnvironmentVariableNameGroup.Key,
                    duplicateEnvironmentVariableNameGroup.ToArray()
                );
            }

            var invalidConverterOptions = command.Options
                .Where(o => o.ConverterType is not null && !o.ConverterType.Implements(typeof(IArgumentValueConverter)))
                .ToArray();

            if (invalidConverterOptions.Any())
            {
                throw CliFxException.OptionsWithInvalidConverters(
                    command,
                    invalidConverterOptions
                );
            }

            var invalidValidatorOptions = command.Options
                .Where(o => !o.ValidatorTypes.All(x => x.Implements(typeof(IArgumentValueValidator))))
                .ToArray();

            if (invalidValidatorOptions.Any())
            {
                throw CliFxException.OptionsWithInvalidValidators(
                    command,
                    invalidValidatorOptions
                );
            }

            var nonLetterFirstCharacterInNameOptions = command.Options
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) && !char.IsLetter(o.Name[0]))
                .ToArray();

            if (nonLetterFirstCharacterInNameOptions.Any())
            {
                throw CliFxException.OptionsWithNonLetterCharacterName(
                    command,
                    nonLetterFirstCharacterInNameOptions
                );
            }

            var nonLetterShortNameOptions = command.Options
                .Where(o => o.ShortName is not null && !char.IsLetter(o.ShortName.Value))
                .ToArray();

            if (nonLetterShortNameOptions.Any())
            {
                throw CliFxException.OptionsWithNonLetterCharacterShortName(
                    command,
                    nonLetterShortNameOptions
                );
            }
        }

        private static void ValidateCommands(IReadOnlyList<CommandSchema> commands)
        {
            if (!commands.Any())
            {
                throw CliFxException.NoCommandsDefined();
            }

            var duplicateNameGroup = commands
                .GroupBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault(g => g.Count() > 1);

            if (duplicateNameGroup is not null)
            {
                throw !string.IsNullOrWhiteSpace(duplicateNameGroup.Key)
                    ? CliFxException.CommandsWithSameName(
                        duplicateNameGroup.Key,
                        duplicateNameGroup.ToArray()
                    )
                    : CliFxException.TooManyDefaultCommands(duplicateNameGroup.ToArray());
            }
        }

        public static ApplicationSchema Resolve(IReadOnlyList<Type> commandTypes)
        {
            var commands = new List<CommandSchema>();

            foreach (var commandType in commandTypes)
            {
                var command =
                    CommandSchema.TryResolve(commandType) ??
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