using System.Diagnostics.CodeAnalysis;

namespace CliFx
{
    internal class CommandParameterInput
    {
        public string Value { get; }

        public CommandParameterInput(string value) => Value = value;

        [ExcludeFromCodeCoverage]
        public override string ToString() => Value;
    }
}