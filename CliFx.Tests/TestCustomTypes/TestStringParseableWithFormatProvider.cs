using System;

namespace CliFx.Tests.TestCustomTypes
{
    public class TestStringParseableWithFormatProvider
    {
        public string Value { get; }

        private TestStringParseableWithFormatProvider(string value)
        {
            Value = value;
        }

        public static TestStringParseableWithFormatProvider Parse(string value, IFormatProvider formatProvider) =>
            new TestStringParseableWithFormatProvider(value + " " + formatProvider);
    }
}