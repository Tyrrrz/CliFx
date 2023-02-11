using System;
using System.Reflection;
using System.Threading.Tasks;

namespace CliFx.Tests.Dummy;

// This dummy application is used in tests for scenarios
// that require an external process to properly verify.

public static partial class Program
{
    public static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();

    public static string Location { get; } = Assembly.Location;
}

public static partial class Program
{
    public static async Task Main()
    {
        // Make sure color codes are not produced because we rely on the output in tests
        Environment.SetEnvironmentVariable(
            "DOTNET_SYSTEM_CONSOLE_ALLOW_ANSI_COLOR_REDIRECTION",
            "false"
        );

        await new CliApplicationBuilder()
            .AddCommandsFromThisAssembly()
            .Build()
            .RunAsync();
    }
}