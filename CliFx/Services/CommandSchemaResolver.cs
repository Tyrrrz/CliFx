using System;
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
        private readonly ICommandArgumentSchemasValidator _commandArgumentSchemasValidator;

        /// <summary>
        /// Creates an instance of <see cref="CommandSchemaResolver"/>.
        /// </summary>
        public CommandSchemaResolver(ICommandArgumentSchemasValidator commandArgumentSchemasValidator)
        {
            _commandArgumentSchemasValidator = commandArgumentSchemasValidator;
        }

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
                        $"Command type [{commandType}] has two options that have the same name ({optionSchema.Name}): " +
                        $"[{existingOptionWithSameName.Property}] and [{optionSchema.Property}]. " +
                        "All options in a command need to have unique names (case-insensitive).");
                }

                // Make sure there are no other options with the same short name
                var existingOptionWithSameShortName = result
                    .Where(o => o.ShortName != null)
                    .FirstOrDefault(o => o.ShortName == optionSchema.ShortName);

                if (existingOptionWithSameShortName != null)
                {
                    throw new CliFxException(
                        $"Command type [{commandType}] has two options that have the same short name ({optionSchema.ShortName}): " +
                        $"[{existingOptionWithSameShortName.Property}] and [{optionSchema.Property}]. " +
                        "All options in a command need to have unique short names (case-sensitive).");
                }

                // Add schema to list
                result.Add(optionSchema);
            }

            return result;
        }

        private IReadOnlyList<CommandArgumentSchema> GetCommandArgumentSchemas(Type commandType)
        {
            var argumentSchemas = commandType.GetProperties()
                .Select(p => new { Property = p, Attribute = p.GetCustomAttribute<CommandArgumentAttribute>() })
                .Where(a => a.Attribute != null)
                .Select(a => new CommandArgumentSchema(a.Property, a.Attribute.Name, a.Attribute.IsRequired, a.Attribute.Description, a.Attribute.Order))
                .ToList();

            var validationErrors = _commandArgumentSchemasValidator.ValidateArgumentSchemas(argumentSchemas).ToList();
            if (validationErrors.Any())
            {
                throw new CliFxException($"Command type [{commandType}] has invalid argument configuration:\n" +
                    $"{string.Join("\n", validationErrors.Select(v => v.Message))}");
            }

            return argumentSchemas;
        }

        /// <inheritdoc />
        public IReadOnlyList<CommandSchema> GetCommandSchemas(IReadOnlyList<Type> commandTypes)
        {
            // Make sure there's at least one command defined
            if (!commandTypes.Any())
            {
                throw new CliFxException(
                    "There are no commands defined. " +
                    "An application needs to have at least one command to work.");
            }

            var result = new List<CommandSchema>();

            foreach (var commandType in commandTypes)
            {
                // Make sure command type implements ICommand.
                if (!commandType.Implements(typeof(ICommand)))
                {
                    throw new CliFxException(
                        $"Command type [{commandType}] needs to implement [{typeof(ICommand)}]."
                        + Environment.NewLine + Environment.NewLine +
                        $"public class {commandType.Name} : ICommand" + Environment.NewLine +
                        "//                             ^-- implement interface");
                }

                // Get attribute
                var attribute = commandType.GetCustomAttribute<CommandAttribute>();

                // Make sure attribute is set
                if (attribute == null)
                {
                    throw new CliFxException(
                        $"Command type [{commandType}] needs to be annotated with [{typeof(CommandAttribute)}]."
                        + Environment.NewLine + Environment.NewLine +
                        "[Command] // <-- add attribute" + Environment.NewLine +
                        $"public class {commandType.Name} : ICommand");
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
                        $"Command type [{existingCommandWithSameName.Type}] has the same name as another command type [{commandType}]. " +
                        "All commands need to have unique names (case-insensitive).");
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