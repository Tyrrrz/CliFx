using System.Threading.Tasks;
using CliFx.Directives;
using CliFx.InteractiveModeDemo.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CliFx.InteractiveModeDemo
{
    public static class Program
    {
        private static void GetServiceCollection(IServiceCollection services)
        {
            services.AddSingleton<LibraryService>();
        }

        public static async Task<int> Main()
        {
            return await new CliApplicationBuilder()
                .ConfigureServices(GetServiceCollection)
                .AddCommandsFromThisAssembly()
                .AddDirective<DebugDirective>()
                .AddDirective<PreviewDirective>()
                .UseInteractiveMode()
                .UseStartupMessage("{title} CLI {version} {{title}} {executable} {{{description}}} {test}")
                .Build()
                .RunAsync();
        }
    }
}