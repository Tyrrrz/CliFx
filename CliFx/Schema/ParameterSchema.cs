using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using CliFx.Attributes;

namespace CliFx.Schema
{
    internal partial class ParameterSchema : MemberSchema
    {
        public int Order { get; }

        public string Name { get; }

        public ParameterSchema(
            PropertyInfo? property,
            int order,
            string name,
            string? description,
            Type? converterType,
            IReadOnlyList<Type> validatorTypes)
            : base(property, description, converterType, validatorTypes)
        {
            Order = order;
            Name = name;
        }

        public override string GetUserFacingDisplayString()
        {
            var buffer = new StringBuilder();

            buffer
                .Append('<')
                .Append(Name)
                .Append('>');

            return buffer.ToString();
        }
    }

    internal partial class ParameterSchema
    {
        public static ParameterSchema? TryResolve(PropertyInfo property)
        {
            var attribute = property.GetCustomAttribute<CommandParameterAttribute>();
            if (attribute is null)
                return null;

            var name = attribute.Name ?? property.Name.ToLowerInvariant();

            return new ParameterSchema(
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