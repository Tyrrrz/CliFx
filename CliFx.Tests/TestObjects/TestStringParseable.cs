namespace CliFx.Tests.TestObjects
{
    public partial struct TestStringParseable
    {
        public string Value { get; }

        private TestStringParseable(string value)
        {
            Value = value;
        }
    }

    public partial struct TestStringParseable
    {
        public static TestStringParseable Parse(string value) => new TestStringParseable(value);
    }
}