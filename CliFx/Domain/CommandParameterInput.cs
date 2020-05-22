namespace CliFx.Domain
{
    internal class CommandParameterInput
    {
        public string Value { get; }

        public CommandParameterInput(string value) => Value = value;

        public override string ToString() => Value;
    }
}