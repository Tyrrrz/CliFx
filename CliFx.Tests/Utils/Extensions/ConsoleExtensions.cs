using CliFx.Infrastructure;
using Xunit.Abstractions;

namespace CliFx.Tests.Utils.Extensions;

internal static class ConsoleExtensions
{
    extension(FakeInMemoryConsole console)
    {
        public void DumpToTestOutput(ITestOutputHelper testOutput)
        {
            testOutput.WriteLine("[*] Captured standard output:");
            testOutput.WriteLine(console.ReadOutputString());

            testOutput.WriteLine("[*] Captured standard error:");
            testOutput.WriteLine(console.ReadErrorString());
        }
    }
}
