using System;

namespace CliFx.Tests
{
    public partial class CommandOptionInputConverterTests
    {
        private enum TestEnum
        {
            Value1,
            Value2,
            Value3
        }

        private class TestStringConstructable
        {
            public string Value { get; }

            public TestStringConstructable(string value)
            {
                Value = value;
            }
        }

        private class TestStringParseable
        {
            public string Value { get; }

            private TestStringParseable(string value)
            {
                Value = value;
            }

            public static TestStringParseable Parse(string value) => new TestStringParseable(value);
        }

        private class TestStringParseableWithFormatProvider
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
}