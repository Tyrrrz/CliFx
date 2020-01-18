using System;
using CliFx.Domain;

namespace CliFx.Tests.Internal
{
    internal class PseudoCommandProperty : ICommandProperty
    {
        public string Name { get; }
        public Type ValueType { get; }
        public void SetValue(object instance, object value)
        {
            throw new NotImplementedException();
        }
    }
}