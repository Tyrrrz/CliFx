using System.Reflection;
using System.Threading.Tasks;

namespace CliFx.Tests.Dummy
{
    // This dummy application is used in tests for scenarios
    // that require an external process to properly verify.

    public static partial class Program
    {
        public static Assembly Assembly { get; } = typeof(Program).Assembly;

        public static string Location { get; } = Assembly.Location;
    }

    public static partial class Program
    {
        public static async Task Main() =>
            await new CliApplicationBuilder()
                .AddCommandsFromThisAssembly()
                .Build()
                .RunAsync();
    }
}