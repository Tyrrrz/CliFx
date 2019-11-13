using System;
using System.Threading.Tasks;
using CliFx.Demo.Commands;
using CliFx.Demo.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CliFx.Demo
{
    public static class Program
    {
        private static IServiceProvider ConfigureServices()
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

        public static Task<int> Main(string[] args)
        {
            var serviceProvider = ConfigureServices();

            return new CliApplicationBuilder()
                .AddCommandsFromThisAssembly()
                .UseCommandFactory(schema => (ICommand) serviceProvider.GetRequiredService(schema.Type))
                .Build()
                .RunAsync(args);
        }
    }
}