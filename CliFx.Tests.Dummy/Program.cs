using System.Globalization;
using System.Threading.Tasks;

namespace CliFx.Tests.Dummy
{
    public static class Program
    {
        public static Task<int> Main(string[] args)
        {
            // Set culture to invariant to maintain consistent format because we rely on it in tests
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

            return new CliApplicationBuilder()
                .WithCommandsFromThisAssembly()
                .UseDescription("Dummy program used for E2E tests.")
                .Build()
                .RunAsync(args);
        }
    }
}