using CliFx.Infrastructure;
using Xunit.Abstractions;

namespace CliFx.Tests.Utils
{
    internal static class ConsoleExtensions
    {
        public static void DumpToTestOutput(this FakeInMemoryConsole console, ITestOutputHelper testOutputHelper)
        {
            testOutputHelper.WriteLine("-------------------------------");

            testOutputHelper.WriteLine("Captured standard output:");
            testOutputHelper.WriteLine(console.ReadOutputString());

            testOutputHelper.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");

            testOutputHelper.WriteLine("Captured standard error:");
            testOutputHelper.WriteLine(console.ReadErrorString());

            testOutputHelper.WriteLine("-------------------------------");
        }
    }
}