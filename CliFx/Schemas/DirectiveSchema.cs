namespace CliFx.Schemas
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using CliFx.Attributes;
    using CliFx.Exceptions;
    using CliFx.Input;
    using CliFx.Internal.Extensions;

    /// <summary>
    /// Stores directive schema.
    /// </summary>
    public partial class DirectiveSchema
    {
        /// <summary>
        /// Directive type.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Directive name.
        /// All directives in an application must have different names.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Directive description, which is used in help text.
        /// </summary>
        public string? Description { get; }

        /// <summary>
        /// Whether directive can run only in interactive mode.
        /// </summary>
        public bool InteractiveModeOnly { get; }

        /// <summary>
        /// List of parameters.
        /// </summary>
        public IReadOnlyList<CommandParameterSchema> Parameters { get; }

        private DirectiveSchema(Type type,
                                string name,
                                string? description,
                                bool interactiveModeOnly,
                                IReadOnlyList<CommandParameterSchema> parameters)
        {
            Type = type;
            Name = name;
            Description = description;
            Parameters = parameters;
            InteractiveModeOnly = interactiveModeOnly;
        }

        /// <summary>
        /// Enumerates through parameters.
        /// </summary>
        public IEnumerable<CommandArgumentSchema> GetArguments()
        {
            foreach (CommandParameterSchema parameter in Parameters)
                yield return parameter;
        }

        /// <summary>
        /// Returns dictionary of arguments and its values.
        /// </summary>
        public IReadOnlyDictionary<CommandArgumentSchema, object?> GetArgumentValues(ICommand instance)
        {
            var result = new Dictionary<CommandArgumentSchema, object?>();

            foreach (CommandArgumentSchema argument in GetArguments())
            {
                // Skip built-in arguments
                if (argument.Property == null)
                    continue;

                object? value = argument.Property.GetValue(instance);
                result[argument] = value;
            }

            return result;
        }

        private void BindParameters(ICommand instance, IReadOnlyList<CommandParameterInput> parameterInputs)
        {
            // All inputs must be bound
            var remainingParameterInputs = parameterInputs.ToList();

            // Scalar parameters
            var scalarParameters = Parameters
                .OrderBy(p => p.Order)
                .TakeWhile(p => p.IsScalar)
                .ToArray();

            for (var i = 0; i < scalarParameters.Length; i++)
            {
                var parameter = scalarParameters[i];

                var scalarInput = i < parameterInputs.Count
                    ? parameterInputs[i]
                    : throw CliFxException.ParameterNotSet(parameter);

                parameter.BindOn(instance, scalarInput.Value);
                remainingParameterInputs.Remove(scalarInput);
            }

            // Non-scalar parameter (only one is allowed)
            var nonScalarParameter = Parameters
                .OrderBy(p => p.Order)
                .FirstOrDefault(p => !p.IsScalar);

            if (nonScalarParameter != null)
            {
                var nonScalarValues = parameterInputs
                    .Skip(scalarParameters.Length)
                    .Select(p => p.Value)
                    .ToArray();

                // Parameters are required by default and so a non-scalar parameter must
                // be bound to at least one value
                if (!nonScalarValues.Any())
                    throw CliFxException.ParameterNotSet(nonScalarParameter);

                nonScalarParameter.BindOn(instance, nonScalarValues);
                remainingParameterInputs.Clear();
            }

            // Ensure all inputs were bound
            if (remainingParameterInputs.Any())
                throw CliFxException.UnrecognizedParametersProvided(remainingParameterInputs);
        }

        internal void Bind(ICommand instance,
                           CommandInput input)
        {
            BindParameters(instance, input.Parameters);
        }

        internal string GetInternalDisplayString()
        {
            var buffer = new StringBuilder();

            // Type
            buffer.Append(Type.FullName);

            // Name
            buffer.Append(' ')
                  .Append('[')
                  .Append(Name)
                  .Append(']');

            return buffer.ToString();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return GetInternalDisplayString();
        }
    }

    public partial class DirectiveSchema
    {
        private static void ValidateParameters(DirectiveSchema command)
        {
            //var duplicateOrderGroup = command.Parameters
            //    .GroupBy(a => a.Order)
            //    .FirstOrDefault(g => g.Count() > 1);

            //if (duplicateOrderGroup != null)
            //{
            //    throw CliFxException.ParametersWithSameOrder(
            //        command,
            //        duplicateOrderGroup.Key,
            //        duplicateOrderGroup.ToArray()
            //    );
            //}

            //var duplicateNameGroup = command.Parameters
            //    .Where(a => !string.IsNullOrWhiteSpace(a.Name))
            //    .GroupBy(a => a.Name!, StringComparer.OrdinalIgnoreCase)
            //    .FirstOrDefault(g => g.Count() > 1);

            //if (duplicateNameGroup != null)
            //{
            //    throw CliFxException.ParametersWithSameName(
            //        command,
            //        duplicateNameGroup.Key,
            //        duplicateNameGroup.ToArray()
            //    );
            //}

            //var nonScalarParameters = command.Parameters
            //                                 .Where(p => !p.IsScalar)
            //                                 .ToArray();

            //if (nonScalarParameters.Length > 1)
            //{
            //    throw CliFxException.TooManyNonScalarParameters(
            //        command,
            //        nonScalarParameters
            //    );
            //}

            //var nonLastNonScalarParameter = command.Parameters
            //    .OrderByDescending(a => a.Order)
            //    .Skip(1)
            //    .LastOrDefault(p => !p.IsScalar);

            //if (nonLastNonScalarParameter != null)
            //{
            //    throw CliFxException.NonLastNonScalarParameter(
            //        command,
            //        nonLastNonScalarParameter
            //    );
            //}
        }

        internal static bool IsDirectiveType(Type type)
        {
            return type.Implements(typeof(IDirective)) &&
                   type.IsDefined(typeof(DirectiveAttribute)) &&
                   !type.IsAbstract &&
                   !type.IsInterface;
        }

        internal static DirectiveSchema? TryResolve(Type type)
        {
            if (!IsDirectiveType(type))
                return null;

            DirectiveAttribute attribute = type.GetCustomAttribute<DirectiveAttribute>()!;

            var parameters = type.GetProperties()
                .Select(CommandParameterSchema.TryResolve)
                .Where(p => p != null)
                .ToArray();

            string name = attribute.Name.TrimStart('[').TrimEnd(']');
            if (string.IsNullOrWhiteSpace(name))
                throw CliFxException.DirectiveNameIsInvalid(name, type);

            return new DirectiveSchema(
                type,
                name,
                attribute.Description,
                attribute.InteractiveModeOnly,
                parameters!
            );
        }

        //public partial class CommandOptionSchema
        //{
        //    internal static CommandOptionSchema HelpOption { get; } =
        //        new CommandOptionSchema(null, "help", 'h', null, false, "Shows help text.");

        //    internal static CommandOptionSchema VersionOption { get; } =
        //        new CommandOptionSchema(null, "version", null, null, false, "Shows version information.");
        //}
    }
}