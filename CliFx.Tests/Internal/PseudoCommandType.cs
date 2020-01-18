using System;
using System.Collections.Generic;
using System.Linq;
using CliFx.Domain;

namespace CliFx.Tests.Internal
{
    internal class PseudoCommandType : ICommandType
    {
        public string Name { get; }

        public IDictionary<string, object> Map { get; }

        public PseudoCommandType(string name, IDictionary<string, object> map)
        {
            Name = name;
            Map = map;
        }

        public PseudoCommandType(IDictionary<string, object> map)
            : this(Guid.NewGuid().ToString(), map)
        {
        }

        public IEnumerable<ICommandProperty> GetProperties() =>
        Map.Keys.Select()
    }
}