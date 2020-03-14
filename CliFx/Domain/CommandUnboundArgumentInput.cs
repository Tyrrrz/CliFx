namespace CliFx.Domain
{
    internal class CommandUnboundArgumentInput
    {
        public string Value { get; }

        public CommandUnboundArgumentInput(string value)
        {
            Value = value;
        }

        public override string ToString() => Value;
    }
}