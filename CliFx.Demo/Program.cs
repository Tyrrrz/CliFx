using CliFx;
using CliFx.Demo.Domain;
using Microsoft.Extensions.DependencyInjection;

return await new CommandLineApplicationBuilder()
    .SetVersion("v1.2.3")
    .SetDescription("Demo application showcasing CliFx features.")
    .AddCommandsFromThisAssembly()
    .AllowDebugMode()
    .AllowPreviewMode()
    .UseTypeInstantiator(commands =>
    {
        // We use Microsoft.Extensions.DependencyInjection for injecting dependencies in commands
        var services = new ServiceCollection();
        services.AddSingleton<LibraryProvider>();

        // Register all commands as transient services
        foreach (var command in commands)
            services.AddTransient(command.Type);

        return services.BuildServiceProvider();
    })
    .Build()
    .RunAsync();
