using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace CliFx.BlazorDemo.CLI.Services
{
    public class WebHostRunnerService : IWebHostRunnerService
    {
        private readonly IConsole _console;

        public WebHostRunnerService(IConsole console)
        {
            _console = console;
        }

        public static IWebHostBuilder CreateHostBuilder()
        {
            return WebHost.CreateDefaultBuilder()
                          .UseKestrel()
                          .UseStartup<Startup>();
        }

        public IWebHost GetWebHost()
        {
            return CreateHostBuilder().Build();
        }

        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            using (IWebHost webHost = GetWebHost())
            {
                _console.Output.WriteLine("Starting WebHost...");
                await webHost.RunAsync(cancellationToken);
                _console.Output.WriteLine("Closing WebHost...");
            }
        }

        public async Task<IWebHost> StartAsync(CancellationToken cancellationToken = default)
        {
            _console.Output.WriteLine("Starting WebHost...");

            IWebHost webHost = GetWebHost();
            await webHost.StartAsync(cancellationToken);

            _console.Output.WriteLine("Running WebHost in background");

            return webHost;
        }
    }
}
