namespace CliFx.Tests.Dummy
{
    using System.Reflection;
    using System.Threading.Tasks;

    public static partial class Program
    {
        public static Assembly Assembly { get; } = typeof(Program).Assembly;

        public static string Location { get; } = Assembly.Location;
    }

    public static partial class Program
    {
        public static async Task Main()
        {
            await new CliApplicationBuilder()
                .AddCommandsFromThisAssembly()
                .Build()
                .RunAsync();
        }
    }
}