﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Internal;
using CliFx.Models;

namespace CliFx.Services
{
    /// <summary>
    /// Default implementation of <see cref="ICommandSchemaResolver"/>.
    /// </summary>
    public class CommandSchemaResolver : ICommandSchemaResolver
    {
        private IReadOnlyList<CommandOptionSchema> GetCommandOptionSchemas(Type commandType)
        {
            var result = new List<CommandOptionSchema>();

            foreach (var property in commandType.GetProperties())
            {
                var attribute = property.GetCustomAttribute<CommandOptionAttribute>();

                // If an attribute is not set, then it's not an option so we just skip it
                if (attribute == null)
                    continue;

                // Build option schema
                var optionSchema = new CommandOptionSchema(property,
                    attribute.Name,
                    attribute.ShortName,
                    attribute.IsRequired,
                    attribute.Description,
                    attribute.EnvironmentVariableName);

                // Make sure there are no other options with the same name
                var existingOptionWithSameName = result
                    .Where(o => !string.IsNullOrWhiteSpace(o.Name))
                    .FirstOrDefault(o => string.Equals(o.Name, optionSchema.Name, StringComparison.OrdinalIgnoreCase));

                if (existingOptionWithSameName != null)
                {
                    throw new CliFxException(
                        $"Command type [{commandType}] has options defined with the same name: " +
                        $"[{existingOptionWithSameName.Property}] and [{optionSchema.Property}].");
                }

                // Make sure there are no other options with the same short name
                var existingOptionWithSameShortName = result
                    .Where(o => o.ShortName != null)
                    .FirstOrDefault(o => o.ShortName == optionSchema.ShortName);

                if (existingOptionWithSameShortName != null)
                {
                    throw new CliFxException(
                        $"Command type [{commandType}] has options defined with the same short name: " +
                        $"[{existingOptionWithSameShortName.Property}] and [{optionSchema.Property}].");
                }

                // Add schema to list
                result.Add(optionSchema);
            }

            return result;
        }

        private IReadOnlyList<CommandArgumentSchema> GetCommandArgumentSchemas(Type commandType)
        {
            var result = new List<CommandArgumentSchema>();
            var argumentOrders = new HashSet<int>();
            int? highestOrderArgument = null;
            int? enumerableArgumentOrder = null;

            foreach (var property in commandType.GetProperties())
            {
                var attribute = property.GetCustomAttribute<CommandArgumentAttribute>();

                // If an attribute is not set, then it's not an argument so we just skip it
                if (attribute is null)
                    continue;

                // Build argument schema
                var argumentSchema = new CommandArgumentSchema(
                    property,
                    attribute.Name,
                    attribute.IsRequired,
                    attribute.Description,
                    attribute.Order);

                // Make sure there are no arguments with the same name
                var existingArgumentWithSameName = result
                    .Where(o => !string.IsNullOrWhiteSpace(o.Name))
                    .FirstOrDefault(o => string.Equals(o.Name, argumentSchema.Name, StringComparison.OrdinalIgnoreCase));

                if (existingArgumentWithSameName != null)
                {
                    throw new CliFxException(
                        $"Command type [{commandType}] has arguments defined with the same name: " +
                        $"[{existingArgumentWithSameName.Property}] and [{argumentSchema.Property}].");
                }

                // Make sure that the order of all properties are distinct
                if (!argumentOrders.Add(argumentSchema.Order))
                {
                    throw new CliFxException(
                        $"Command type [{commandType}] has arguments defined with the same order [{argumentSchema.Order}].");
                }

                // Verify that the order is not higher than an enumerable argument found yet
                if (argumentSchema.Order > enumerableArgumentOrder)
                {
                    throw new CliFxException(
                        $"Command type [{commandType}] defines a sequence argument with lower order than another argument; sequence argument must have the highest order (appear last).");
                }

                // Set the highest found argument order if applicable
                if (!highestOrderArgument.HasValue || argumentSchema.Order > highestOrderArgument)
                {
                    highestOrderArgument = argumentSchema.Order;
                }

                // If the argument is an enumerable type
                if (argumentSchema.Property.PropertyType != typeof(string) && argumentSchema.Property.PropertyType.GetEnumerableUnderlyingType() != null)
                {
                    // Verify that no other enumerable type has been found
                    if (enumerableArgumentOrder.HasValue)
                    {
                        throw new CliFxException(
                            $"Command type [{commandType}] defines multiple sequence arguments; only one is permitted.");
                    }

                    // Remember the order of this property
                    enumerableArgumentOrder = argumentSchema.Order;

                    // Verify that no higher order arguments have yet been found
                    if (enumerableArgumentOrder > highestOrderArgument)
                    {
                        throw new CliFxException(
                            $"Command type [{commandType}] defines a sequence argument with lower order than another argument; sequence argument must have the highest order (appear last).");
                    }
                }

                // Add schema to list
                result.Add(argumentSchema);
            }

            if (result.Any(a => a.IsRequired) && result.Any(a => !a.IsRequired))
            {
                // Verify all required arguments appear before optional arguments
                var highestOrderRequiredArgument = result.Where(a => a.IsRequired).Max(a => a.Order);
                var lowestOrderOptionalArgument = result.Where(a => !a.IsRequired).Min(a => a.Order);
                if (highestOrderRequiredArgument > lowestOrderOptionalArgument)
                {
                    throw new CliFxException(
                        $"Command type [{commandType}] has required arguments that appear after optional arguments. Required arguments must appear before optional arguments.");
                }
            }

            return result;
        }

        /// <inheritdoc />
        public IReadOnlyList<CommandSchema> GetCommandSchemas(IReadOnlyList<Type> commandTypes)
        {
            // Make sure there's at least one command defined
            if (!commandTypes.Any())
            {
                throw new CliFxException("There are no commands defined.");
            }

            var result = new List<CommandSchema>();

            foreach (var commandType in commandTypes)
            {
                // Make sure command type implements ICommand.
                if (!commandType.Implements(typeof(ICommand)))
                {
                    throw new CliFxException($"Command type [{commandType}] must implement {typeof(ICommand)}.");
                }

                // Get attribute
                var attribute = commandType.GetCustomAttribute<CommandAttribute>();

                // Make sure attribute is set
                if (attribute == null)
                {
                    throw new CliFxException($"Command type [{commandType}] must be annotated with [{typeof(CommandAttribute)}].");
                }

                // Get option schemas
                var optionSchemas = GetCommandOptionSchemas(commandType);

                // Get argument schemas
                var argumentSchemas = GetCommandArgumentSchemas(commandType);

                // Build command schema
                var commandSchema = new CommandSchema(commandType,
                    attribute.Name,
                    attribute.Description,
                    argumentSchemas, optionSchemas);

                // Make sure there are no other commands with the same name
                var existingCommandWithSameName = result
                    .FirstOrDefault(c => string.Equals(c.Name, commandSchema.Name, StringComparison.OrdinalIgnoreCase));

                if (existingCommandWithSameName != null)
                {
                    throw new CliFxException(
                        $"Command type [{existingCommandWithSameName.Type}] has the same name as another command type [{commandType}].");
                }

                // Add schema to list
                result.Add(commandSchema);
            }

            return result;
        }

        /// <inheritdoc />
        public CommandCandidate? GetTargetCommandSchema(IReadOnlyList<CommandSchema> availableCommandSchemas, CommandInput commandInput)
        {
            // If no arguments are given, use the default command
            CommandSchema targetSchema;
            if (!commandInput.Arguments.Any())
            {
                targetSchema = availableCommandSchemas.FirstOrDefault(c => c.IsDefault());
                return targetSchema is null ? null : new CommandCandidate(targetSchema, new string[0], commandInput);
            }

            // Arguments can be part of the a command name as long as they are single words, i.e. no whitespace characters
            var longestPossibleCommandName = string.Join(" ", commandInput.Arguments.TakeWhile(arg => !Regex.IsMatch(arg, @"\s")));

            // Find the longest matching schema
            var orderedSchemas = availableCommandSchemas.OrderByDescending(x => x.Name?.Length);
            targetSchema = orderedSchemas.FirstOrDefault(c => longestPossibleCommandName.StartsWith(c.Name ?? string.Empty, StringComparison.Ordinal))
                           ?? availableCommandSchemas.FirstOrDefault(c => c.IsDefault());

            // Get remaining positional arguments
            var commandArgumentsCount = targetSchema?.Name?.Split(' ').Length ?? 0;
            var positionalArguments = commandInput.Arguments.Skip(commandArgumentsCount).ToList();

            return targetSchema is null ? null : new CommandCandidate(targetSchema, positionalArguments, commandInput);
        }
    }
}