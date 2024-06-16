using System.Collections.Generic;
using System.IO;

namespace CliFx.Input;

/// <summary>
/// Input provided by the means of an environment variable.
/// </summary>
public class EnvironmentVariableInput(string name, string value)
{
    /// <summary>
    /// Environment variable name.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// Environment variable value.
    /// </summary>
    public string Value { get; } = value;

    internal IReadOnlyList<string> SplitValues() => Value.Split(Path.PathSeparator);
}
