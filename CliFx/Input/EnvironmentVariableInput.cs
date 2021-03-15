using System.Collections.Generic;
using System.IO;

namespace CliFx.Input
{
    internal class EnvironmentVariableInput
    {
        public string Name { get; }

        public string RawValue { get; }

        public EnvironmentVariableInput(string name, string rawValue)
        {
            Name = name;
            RawValue = rawValue;
        }

        public string GetValue() => RawValue;

        public IReadOnlyList<string> GetValues() => RawValue.Split(Path.PathSeparator);
    }
}