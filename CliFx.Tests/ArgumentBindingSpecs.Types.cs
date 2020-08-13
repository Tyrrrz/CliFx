namespace CliFx.Tests
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public partial class ArgumentBindingSpecs
    {
        private enum CustomEnum
        {
            Value1 = 1,
            Value2 = 2,
            Value3 = 3
        }

        private class StringConstructable
        {
            public string Value { get; }

            public StringConstructable(string value)
            {
                Value = value;
            }
        }

        private class StringParseable
        {
            public string Value { get; }

            private StringParseable(string value)
            {
                Value = value;
            }

            public static StringParseable Parse(string value)
            {
                return new StringParseable(value);
            }
        }

        private class StringParseableWithFormatProvider
        {
            public string Value { get; }

            private StringParseableWithFormatProvider(string value)
            {
                Value = value;
            }

            public static StringParseableWithFormatProvider Parse(string value, IFormatProvider formatProvider)
            {
                return new StringParseableWithFormatProvider(value + " " + formatProvider);
            }
        }

        private class DummyType
        {
        }

        public class CustomEnumerable<T> : IEnumerable<T>
        {
            public IEnumerator<T> GetEnumerator()
            {
                return ((IEnumerable<T>)Array.Empty<T>()).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}