using CliFx.Infrastructure;
using Xunit.Abstractions;

namespace CliFx.Tests.Utils.Extensions;

internal static class ConsoleExtensions
{
    public static void DumpToTestOutput(
        this FakeInMemoryConsole console,
        ITestOutputHelper testOutput
    )
    {
        testOutput.WriteLine("[*] Captured standard output:");
        testOutput.WriteLine(console.ReadOutputString());

        testOutput.WriteLine("[*] Captured standard error:");
        testOutput.WriteLine(console.ReadErrorString());
    }
}
