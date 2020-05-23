﻿using System.Reflection;
using System.Text;
using CliFx.Attributes;

namespace CliFx.Domain
{
    internal partial class CommandParameterSchema : CommandArgumentSchema
    {
        public int Order { get; }

        public string Name { get; }

        public CommandParameterSchema(PropertyInfo? property, int order, string name, string? description)
            : base(property, description)
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
                attribute.Description
            );
        }
    }
}