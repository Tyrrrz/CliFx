namespace CliFx.Domain
{
    internal class CommandDirectiveInput
    {
        public string Name { get; }

        public CommandDirectiveInput(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return $"[{Name}]";
        }
    }
}