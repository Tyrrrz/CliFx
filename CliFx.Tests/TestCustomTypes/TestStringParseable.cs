namespace CliFx.Tests.TestCustomTypes
{
    public class TestStringParseable
    {
        public string Value { get; }

        private TestStringParseable(string value)
        {
            Value = value;
        }

        public static TestStringParseable Parse(string value) => new TestStringParseable(value);
    }
}