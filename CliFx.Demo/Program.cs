using CliFx;
using CliFx.Demo.Domain;
using Microsoft.Extensions.DependencyInjection;

return await new CliApplicationBuilder()
    .SetDescription("Demo application showcasing CliFx features.")
    .AddCommandsFromThisAssembly()
    .UseTypeActivator(commandSchemas =>
    {
        // We use Microsoft.Extensions.DependencyInjection for injecting dependencies in commands
        var services = new ServiceCollection();
        services.AddSingleton<LibraryProvider>();

        // Register all commands as transient services
        foreach (var commandSchema in commandSchemas)
            services.AddTransient(commandSchema.Type);

        return services.BuildServiceProvider();
    })
    .Build()
    .RunAsync();
