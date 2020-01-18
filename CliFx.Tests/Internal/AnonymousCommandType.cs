using System.Collections.Generic;
using CliFx.Domain;

namespace CliFx.Tests.Internal
{
    internal class AnonymousCommandType : ICommandType
    {
        private readonly Dictionary<string, object> _map = new Dictionary<string, object>();

        public string Name { get; }

        public AnonymousCommandType(string name, params string[] propertyNames)
        {
            Name = name;

            foreach (var propertyName in propertyNames)
                _map[propertyName] = null;
        }

        public IEnumerable<ICommandProperty> GetProperties()
        {
            throw new System.NotImplementedException();
        }
    }
}