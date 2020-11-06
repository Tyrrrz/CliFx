using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using CliFx.Attributes;

namespace CliFx.Domain
{
    internal partial class CommandParameterSchema : CommandArgumentSchema
    {
        public int Order { get; }

        public string Name { get; }

        public CommandParameterSchema(
            PropertyInfo? property,
            int order,
            string name,
            string? description,
            Type? converter = null,
            Type[]? validators = null)
            : base(property, description, converter, validators)
        {
            Order = order;
            Name = name;
        }

        public string GetUserFacingDisplayString()
        {
            var buffer = new StringBuilder();

            buffer
                .Append('<')
                .Append(Name)
                .Append('>');

            return buffer.ToString();
        }

        public string GetInternalDisplayString() => $"{Property?.Name ?? "<implicit>"} ([{Order}] {GetUserFacingDisplayString()})";

        [ExcludeFromCodeCoverage]
        public override string ToString() => GetInternalDisplayString();
    }

    internal partial class CommandParameterSchema
    {
        public static CommandParameterSchema? TryResolve(PropertyInfo property)
        {
            var attribute = property.GetCustomAttribute<CommandParameterAttribute>();
            if (attribute == null)
                return null;

            var name = attribute.Name ?? property.Name.ToLowerInvariant();

            return new CommandParameterSchema(
                property,
                attribute.Order,
                name,
                attribute.Description,
                attribute.Converter,
                attribute.Validators
            );
        }
    }
}