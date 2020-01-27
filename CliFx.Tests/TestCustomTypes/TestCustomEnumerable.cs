using System.Collections;
using System.Collections.Generic;

namespace CliFx.Tests.TestCustomTypes
{
    public class TestCustomEnumerable<T> : IEnumerable<T>
    {
        private readonly T[] _arr = new T[0];

        public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>) _arr).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}