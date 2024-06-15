using System.Collections.Generic;
using System.IO;

namespace CliFx.Input;

public class EnvironmentVariableInput(string name, string value)
{
    public string Name { get; } = name;

    public string Value { get; } = value;

    public IReadOnlyList<string> SplitValues() => Value.Split(Path.PathSeparator);
}
