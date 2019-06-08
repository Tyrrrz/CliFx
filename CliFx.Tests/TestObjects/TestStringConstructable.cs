namespace CliFx.Tests.TestObjects
{
    public struct TestStringConstructable
    {
        public string Value { get; }

        public TestStringConstructable(string value)
        {
            Value = value;
        }
    }
}