using System.Collections.Generic;
using CliFx.Binding;

namespace CliFx;

/// <summary>
/// Configuration of a command-line application.
/// </summary>
public class CommandLineApplicationConfiguration(
    IReadOnlyList<CommandDescriptor> commands,
    string? debugModeEnvironmentVariable,
    string? previewModeEnvironmentVariable
)
{
    /// <summary>
    /// Commands registered in the application.
    /// </summary>
    public IReadOnlyList<CommandDescriptor> Commands { get; } = commands;

    /// <summary>
    /// Environment variable that enables the debug mode.
    /// </summary>
    public string? DebugModeEnvironmentVariable { get; } = debugModeEnvironmentVariable;

    /// <summary>
    /// Environment variable that enables the preview mode.
    /// </summary>
    public string? PreviewModeEnvironmentVariable { get; } = previewModeEnvironmentVariable;
}
