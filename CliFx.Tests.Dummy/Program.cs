using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace CliFx.Tests.Dummy;

// This dummy application is used in tests for scenarios that require an external process to properly verify
public static class Program
{
    // Path to the apphost
    public static string FilePath { get; } =
        Path.ChangeExtension(
            Assembly.GetExecutingAssembly().Location,
            OperatingSystem.IsWindows() ? "exe" : null
        );

    public static async Task Main()
    {
        // Make sure color codes are not produced because we rely on the output in tests
        Environment.SetEnvironmentVariable(
            "DOTNET_SYSTEM_CONSOLE_ALLOW_ANSI_COLOR_REDIRECTION",
            "false"
        );

        await new CliApplicationBuilder().AddCommandsFromThisAssembly().Build().RunAsync();
    }
}
