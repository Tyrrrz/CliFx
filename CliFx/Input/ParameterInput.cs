using System.Diagnostics.CodeAnalysis;

namespace CliFx.Input
{
    internal class ParameterInput
    {
        public string Value { get; }

        public ParameterInput(string value) => Value = value;

        [ExcludeFromCodeCoverage]
        public override string ToString() => Value;
    }
}