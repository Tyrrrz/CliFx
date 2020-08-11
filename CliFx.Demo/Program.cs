using System;
using System.Threading.Tasks;
using CliFx.Demo.Commands;
using CliFx.Demo.Services;
using CliFx.Directives;
using Microsoft.Extensions.DependencyInjection;

namespace CliFx.Demo
{
    public static class Program
    {
        private static IServiceProvider GetServiceProvider()
        {
            // We use Microsoft.Extensions.DependencyInjection for injecting dependencies in commands
            var services = new ServiceCollection();

            // Register services
            services.AddSingleton<LibraryService>();

            // Register commands
            services.AddTransient<BookCommand>();
            services.AddTransient<BookAddCommand>();
            services.AddTransient<BookRemoveCommand>();
            services.AddTransient<BookListCommand>();

            return services.BuildServiceProvider();
        }

        public static async Task<int> Main() =>
            await new CliApplicationBuilder()
                .UseTypeActivator(GetServiceProvider().GetService)
                .AddCommandsFromThisAssembly()
                .AddDirective<DebugDirective>()
                .AddDirective<PreviewDirective>()
                .Build()
                .RunAsync();
    }
}