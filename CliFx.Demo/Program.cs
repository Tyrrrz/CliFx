using System.Threading.Tasks;
using CliFx.Demo.Commands;
using CliFx.Demo.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CliFx.Demo
{
    public static class Program
    {
        public static Task<int> Main(string[] args)
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

            var serviceProvider = services.BuildServiceProvider();

            return new CliApplicationBuilder()
                .AddCommandsFromThisAssembly()
                .UseCommandFactory(schema => (ICommand) serviceProvider.GetRequiredService(schema.Type))
                .Build()
                .RunAsync(args);
        }
    }
}