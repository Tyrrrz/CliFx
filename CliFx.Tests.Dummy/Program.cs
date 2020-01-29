using System.Threading.Tasks;

namespace CliFx.Tests.Dummy
{
    public class Program
    {
        public static async Task Main() =>
            await new CliApplicationBuilder()
                .AddCommandsFromThisAssembly()
                .Build()
                .RunAsync();
    }
}