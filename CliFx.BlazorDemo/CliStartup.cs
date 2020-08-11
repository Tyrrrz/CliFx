using CliFx.BlazorDemo.CLI.Services;
using CliFx.Directives;
using Microsoft.Extensions.DependencyInjection;

namespace CliFx.BlazorDemo
{
    public class CliStartup : ICliStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // Register services
            services.AddSingleton<IWebHostRunnerService, WebHostRunnerService>()
                    .AddSingleton<IBackgroundWebHostProvider, BackgroundWebHostProvider>();
        }

        public void Configure(CliApplicationBuilder app)
        {
            app.AddCommandsFromThisAssembly()
               .AddDirective<DebugDirective>()
               .ConfigureServices(ConfigureServices)
               .UseInteractiveMode();
        }
    }
}
