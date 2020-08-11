using System.Threading.Tasks;
using CliFx.BlazorDemo.CLI.Services;
using CliFx.Directives;
using Microsoft.Extensions.DependencyInjection;

namespace CliFx.BlazorDemo
{
    public static class Program
    {
        private static void ConfigureServices(IServiceCollection services)
        {
            // Register services
            services.AddTransient<IWebHostRunnerService, WebHostRunnerService>();
            services.AddTransient<IBackgroundWebHostProvider, BackgroundWebHostProvider>();
        }

        public static async Task<int> Main()
        {
            return await new CliApplicationBuilder()
                .AddCommandsFromThisAssembly()
                .AddDirective<DebugDirective>()
                .ConfigureServices(ConfigureServices)
                .UseInteractiveMode()
                .Build()
                .RunAsync();
        }
    }
}
