using System;
using System.Linq;
using CliFx.Models;
using CliFx.Services;

namespace CliFx.Tests.Internal
{
    internal static class Extensions
    {
        public static CommandSchema GetCommandSchema(this ICommandSchemaResolver resolver, Type commandType) =>
            resolver.GetCommandSchemas(new[] {commandType}).Single();
    }
}