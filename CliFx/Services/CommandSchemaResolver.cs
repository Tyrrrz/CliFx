using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CliFx.Attributes;
using CliFx.Models;

namespace CliFx.Services
{
    public class CommandSchemaResolver : ICommandSchemaResolver
    {
        private readonly IReadOnlyList<Type> _sourceTypes;

        public CommandSchemaResolver(IReadOnlyList<Type> sourceTypes)
        {
            _sourceTypes = sourceTypes;
        }

        public CommandSchemaResolver(IReadOnlyList<Assembly> sourceAssemblies)
            : this(sourceAssemblies.SelectMany(a => a.ExportedTypes).ToArray())
        {
        }

        public CommandSchemaResolver()
            : this(new[] {Assembly.GetEntryAssembly()})
        {
        }

        private IEnumerable<Type> GetCommandTypes() => _sourceTypes.Where(t => t.GetInterfaces().Contains(typeof(ICommand)));

        private IReadOnlyList<CommandOptionSchema> GetCommandOptionSchemas(Type commandType)
        {
            var result = new List<CommandOptionSchema>();

            foreach (var optionProperty in commandType.GetProperties())
            {
                var optionAttribute = optionProperty.GetCustomAttribute<CommandOptionAttribute>();

                if (optionAttribute == null)
                    continue;

                result.Add(new CommandOptionSchema(optionProperty,
                    optionAttribute.Name,
                    optionAttribute.ShortName,
                    optionAttribute.IsRequired,
                    optionAttribute.GroupName,
                    optionAttribute.Description));
            }

            return result;
        }

        public IReadOnlyList<CommandSchema> ResolveAllSchemas()
        {
            var result = new List<CommandSchema>();

            foreach (var commandType in GetCommandTypes())
            {
                var commandAttribute = commandType.GetCustomAttribute<CommandAttribute>();

                if (commandAttribute == null)
                    continue;

                result.Add(new CommandSchema(commandType,
                    commandAttribute.Name,
                    commandAttribute.IsDefault,
                    commandAttribute.Description,
                    GetCommandOptionSchemas(commandType)));
            }

            return result;
        }
    }
}