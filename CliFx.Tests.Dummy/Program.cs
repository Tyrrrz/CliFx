using System.Threading.Tasks;

namespace CliFx.Tests.Dummy
{
    public static class Program
    {
        public static Task<int> Main(string[] args) =>
            new CliApplicationBuilder()
                .WithCommandsFromThisAssembly()
                .UseDescription("Dummy program used for E2E tests.")
                .Build()
                .RunAsync(args);
    }
}