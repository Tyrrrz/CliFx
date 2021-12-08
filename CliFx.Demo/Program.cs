using CliFx;
using CliFx.Demo.Commands;
using CliFx.Demo.Domain;
using Microsoft.Extensions.DependencyInjection;

// We use Microsoft.Extensions.DependencyInjection for injecting dependencies in commands
var services = new ServiceCollection();

// Register services
services.AddSingleton<LibraryProvider>();

// Register commands
services.AddTransient<BookCommand>();
services.AddTransient<BookAddCommand>();
services.AddTransient<BookRemoveCommand>();
services.AddTransient<BookListCommand>();

var serviceProvider = services.BuildServiceProvider();

return await new CliApplicationBuilder()
    .SetDescription("Demo application showcasing CliFx features.")
    .AddCommandsFromThisAssembly()
    .UseTypeActivator(serviceProvider.GetRequiredService)
    .Build()
    .RunAsync();