using System;
using System.Threading.Tasks;
using CliFx.Demo.Commands;
using CliFx.Demo.Domain;
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
            services.AddSingleton<LibraryProvider>();

            // Register commands
            services.AddTransient<BookCommand>();
            services.AddTransient<BookAddCommand>();
            services.AddTransient<BookRemoveCommand>();
            services.AddTransient<BookListCommand>();

            return services.BuildServiceProvider();
        }

        public static async Task<int> Main() =>
            await new CliApplicationBuilder()
                .AddCommandsFromThisAssembly()
                .UseTypeActivator(GetServiceProvider().GetRequiredService)
                .Build()
                .RunAsync();
    }
}