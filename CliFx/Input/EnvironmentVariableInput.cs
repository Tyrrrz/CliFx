using System.Collections.Generic;
using System.IO;

namespace CliFx.Input;

internal class EnvironmentVariableInput
{
    public string Name { get; }

    public string Value { get; }

    public EnvironmentVariableInput(string name, string value)
    {
        Name = name;
        Value = value;
    }

    public IReadOnlyList<string> SplitValues() => Value.Split(Path.PathSeparator);
}
