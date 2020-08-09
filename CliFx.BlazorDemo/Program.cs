using System;
using System.Threading.Tasks;
using CliFx.BlazorDemo.CLI.Commands;
using CliFx.BlazorDemo.CLI.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CliFx.BlazorDemo
{
    public static class Program
    {
        private static Func<Type, object> GetServiceCollection(ICliContext cliContext, IConsole console)
        {
            // We use Microsoft.Extensions.DependencyInjection for injecting dependencies in commands
            var services = new ServiceCollection();

            // Register services
            services.AddSingleton(cliContext);
            services.AddSingleton(console);
            services.AddTransient<IWebHostRunnerService, WebHostRunnerService>();
            services.AddTransient<IBackgroundWebHostProvider, BackgroundWebHostProvider>();

            // Register commands
            services.AddTransient<DefaultCommand>();
            services.AddTransient<WebHostCommand>();
            services.AddTransient<WebHostRestartCommand>();
            services.AddTransient<WebHostStartCommand>();
            services.AddTransient<WebHostStatusCommand>();
            services.AddTransient<WebHostStopCommand>();

            return services.BuildServiceProvider().GetService;
        }

        public static async Task<int> Main()
        {
            return await new CliApplicationBuilder()
                .AddCommandsFromThisAssembly()
                .UseTypeActivator(GetServiceCollection)
                .AllowInteractiveMode(true)
                .Build()
                .RunAsync();
        }
    }
}
