using System;
using System.Collections.Generic;
using System.Linq;
using CliFx.Exceptions;
using CliFx.Internal;
using CliFx.Models;

namespace CliFx.Services
{
    /// <inheritdoc />
    public class CommandArgumentSchemasValidator : ICommandArgumentSchemasValidator
    {
        private bool IsEnumerableArgument(CommandArgumentSchema schema)
        {
            return schema.Property.PropertyType != typeof(string) && schema.Property.PropertyType.GetEnumerableUnderlyingType() != null;
        }
        
        /// <inheritdoc />
        public IEnumerable<ValidationError> ValidateArgumentSchemas(IReadOnlyCollection<CommandArgumentSchema> commandArgumentSchemas)
        {
            if (commandArgumentSchemas.Count == 0)
            {
                // No validation needed
                yield break;
            }
            
            // Make sure there are no arguments with the same name
            var duplicateNameGroups = commandArgumentSchemas
                .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                .GroupBy(x => x.Name)
                .Where(x => x.Count() > 1);
            foreach (var schema in duplicateNameGroups)
            {
                yield return new ValidationError($"Multiple arguments with same name: \"{schema.Key}\".");
            }

            // Make sure that the order of all properties are distinct
            var duplicateOrderGroups = commandArgumentSchemas
                .GroupBy(x => x.Order)
                .Where(x => x.Count() > 1);
            foreach (var schema in duplicateOrderGroups)
            {
                yield return new ValidationError($"Multiple arguments with the same order: \"{schema.Key}\".");
            }

            var enumerableArguments = commandArgumentSchemas
                .Where(IsEnumerableArgument)
                .ToList();

            // Verify that no more than one enumerable argument exists
            if (enumerableArguments.Count > 1)
            {
                yield return new ValidationError($"Multiple sequence arguments found; only one is supported.");
            }
            
            // If an enumerable argument exists, ensure that it has the highest order
            if (enumerableArguments.Count == 1)
            {
                if (enumerableArguments.Single().Order != commandArgumentSchemas.Max(x => x.Order))
                {
                    yield return new ValidationError($"A sequence argument was defined with a lower order than another argument; the sequence argument must have the highest order (appear last).");
                }
            }
            
            // Verify that all required arguments appear before optional arguments
            if (commandArgumentSchemas.Any(x => x.IsRequired) && commandArgumentSchemas.Any(x => !x.IsRequired) &&
                commandArgumentSchemas.Where(x => x.IsRequired).Max(x => x.Order) > commandArgumentSchemas.Where(x => !x.IsRequired).Min(x => x.Order))
            {
                yield return new ValidationError("One or more required arguments appear after optional arguments. Required arguments must appear before (i.e. have lower order than) optional arguments.");
            }
        }
    }

    /// <summary>
    /// Represents a failed validation.
    /// </summary>
    public class ValidationError
    {
        /// <summary>
        /// Creates an instance of <see cref="ValidationError"/> with a message.
        /// </summary>
        public ValidationError(string message)
        {
            Message = message;
        }

        /// <summary>
        /// The error message for the failed validation.
        /// </summary>
        public string Message { get; }
    }
}