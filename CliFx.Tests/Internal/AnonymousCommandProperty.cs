using System;
using CliFx.Domain;

namespace CliFx.Tests.Internal
{
    internal class AnonymousCommandProperty : ICommandProperty
    {
        private readonly Action<>

        public AnonymousCommandProperty(string name, Type valueType)
        {
            Name = name;
            ValueType = valueType;
        }

        public string Name { get; }
        public Type ValueType { get; }
        public void SetValue(object instance, object value)
        {
            throw new NotImplementedException();
        }
    }
}