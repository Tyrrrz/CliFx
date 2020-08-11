using System.Threading.Tasks;
using CliFx.Demo.Services;
using CliFx.Directives;
using Microsoft.Extensions.DependencyInjection;

namespace CliFx.Demo
{
    public static class Program
    {
        private static void ConfigureServices(IServiceCollection services)
        {
            // Register services
            services.AddSingleton<LibraryService>();
        }

        public static async Task<int> Main() =>
            await new CliApplicationBuilder()
                .ConfigureServices(ConfigureServices)
                .AddCommandsFromThisAssembly()
                .AddDirective<DebugDirective>()
                .AddDirective<PreviewDirective>()
                .Build()
                .RunAsync();
    }
}