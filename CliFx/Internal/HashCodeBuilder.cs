using System.Collections.Generic;
using System.Linq;

namespace CliFx.Internal
{
    internal class HashCodeBuilder
    {
        private int _code = 17;

        public HashCodeBuilder Add(int hashCode)
        {
            unchecked
            {
                _code = _code * 23 + hashCode;
            }

            return this;
        }

        public HashCodeBuilder Add(IEnumerable<int> hashCodes)
        {
            foreach (var hashCode in hashCodes)
                Add(hashCode);

            return this;
        }

        public HashCodeBuilder Add<T>(T obj, IEqualityComparer<T> comparer) => Add(comparer.GetHashCode(obj));

        public HashCodeBuilder Add<T>(T obj) => Add(obj, EqualityComparer<T>.Default);

        public HashCodeBuilder AddMany<T>(IEnumerable<T> objs, IEqualityComparer<T> comparer) => Add(objs.Select(comparer.GetHashCode));

        public HashCodeBuilder AddMany<T>(IEnumerable<T> objs) => AddMany(objs, EqualityComparer<T>.Default);

        public int Build() => _code;
    }
}