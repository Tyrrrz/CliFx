using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace CliFx.Tests.Dummy;

// This dummy application is used in tests where an external process is required
// to properly verify the library's behavior.
public static class Program
{
    // Path to the apphost
    public static string FilePath { get; } =
        Path.ChangeExtension(
            Assembly.GetExecutingAssembly().Location,
            OperatingSystem.IsWindows() ? "exe" : null
        );

    public static async Task<int> Main()
    {
        // Make sure color codes are not produced because we rely on the output in tests
        Environment.SetEnvironmentVariable(
            "DOTNET_SYSTEM_CONSOLE_ALLOW_ANSI_COLOR_REDIRECTION",
            "false"
        );

        return await new CliApplicationBuilder()
            .AddCommandsFromThisAssembly()
            .AllowDebugMode()
            .AllowPreviewMode()
            .Build()
            .RunAsync();
    }
}
