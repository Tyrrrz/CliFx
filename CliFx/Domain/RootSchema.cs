﻿using System;
using System.Collections.Generic;
using System.Linq;
using CliFx.Exceptions;
using CliFx.Internal;

namespace CliFx.Domain
{
    internal partial class RootSchema
    {
        public IReadOnlyList<CommandSchema> Commands { get; }

        public RootSchema(IReadOnlyList<CommandSchema> commands)
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

    internal partial class RootSchema
    {
        private static void ValidateParameters(CommandSchema command)
        {
            var duplicateOrderGroup = command.Parameters
                .GroupBy(a => a.Order)
                .FirstOrDefault(g => g.Count() > 1);

            if (duplicateOrderGroup != null)
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

            if (duplicateNameGroup != null)
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

            if (nonLastNonScalarParameter != null)
            {
                throw CliFxException.NonLastNonScalarParameter(
                    command,
                    nonLastNonScalarParameter
                );
            }
        }

        private static void ValidateOptions(CommandSchema command)
        {
            var noNameGroup = command.Options
                .Where(o => o.ShortName == null && string.IsNullOrWhiteSpace(o.Name))
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

            if (duplicateNameGroup != null)
            {
                throw CliFxException.OptionsWithSameName(
                    command,
                    duplicateNameGroup.Key,
                    duplicateNameGroup.ToArray()
                );
            }

            var duplicateShortNameGroup = command.Options
                .Where(o => o.ShortName != null)
                .GroupBy(o => o.ShortName!.Value)
                .FirstOrDefault(g => g.Count() > 1);

            if (duplicateShortNameGroup != null)
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

            if (duplicateEnvironmentVariableNameGroup != null)
            {
                throw CliFxException.OptionsWithSameEnvironmentVariableName(
                    command,
                    duplicateEnvironmentVariableNameGroup.Key,
                    duplicateEnvironmentVariableNameGroup.ToArray()
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

            if (duplicateNameGroup != null)
            {
                throw !string.IsNullOrWhiteSpace(duplicateNameGroup.Key)
                    ? CliFxException.CommandsWithSameName(
                        duplicateNameGroup.Key,
                        duplicateNameGroup.ToArray()
                    )
                    : CliFxException.TooManyDefaultCommands(duplicateNameGroup.ToArray());
            }
        }

        public static RootSchema Resolve(IReadOnlyList<Type> commandTypes)
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

            return new RootSchema(commands);
        }
    }
}