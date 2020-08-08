using System;
using System.Threading.Tasks;
using CliFx.Demo.Commands;
using CliFx.Demo.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CliFx.Demo
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
            services.AddSingleton<LibraryService>();

            // Register commands
            services.AddTransient<BookCommand>();
            services.AddTransient<BookAddCommand>();
            services.AddTransient<BookRemoveCommand>();
            services.AddTransient<BookListCommand>();

            return services.BuildServiceProvider().GetService;
        }

        public static async Task<int> Main()
        {
            return await new CliApplicationBuilder()
                .AddCommandsFromThisAssembly()
                .UseTypeActivator(GetServiceCollection)
                .Build()
                .RunAsync();
        }
    }
}