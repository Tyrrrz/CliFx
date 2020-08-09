using System;
using System.Threading;
using System.Threading.Tasks;
using CliFx.Exceptions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace CliFx.BlazorDemo.CLI.Services
{
    public class BackgroundWebHostProvider : IBackgroundWebHostProvider
    {
        private bool disposedValue;

        private readonly IWebHostRunnerService _webHostRunnerService;
        private readonly IConsole _console;

        private IWebHost? _webHost;
        public IWebHost WebHost => _webHost ?? throw new CommandException("WebHost has not been started.");

        private IServiceScope? _serviceScope;
        public IServiceScope ServiceScope => _serviceScope ?? throw new CommandException("WebHost has not been started.");

        public DateTime StartupTime { get; private set; }
        public TimeSpan Runtime => _webHost is null ? TimeSpan.Zero : DateTime.UtcNow - StartupTime;
        public WebHostStatuses Status => _webHost is null ? WebHostStatuses.Stopped : WebHostStatuses.Running;

        public BackgroundWebHostProvider(IWebHostRunnerService webHostRunnerService, IConsole console)
        {
            _webHostRunnerService = webHostRunnerService;
            _console = console;
        }

        public async Task StartAsync()
        {
            await _console.Output.WriteLineAsync("Starting WebHost background worker...");
            StartupTime = DateTime.UtcNow;

            _webHost = await _webHostRunnerService.StartAsync();
            IServiceScopeFactory serviceScopeFactory = WebHost.Services.GetService<IServiceScopeFactory>();
            _serviceScope = serviceScopeFactory.CreateScope();
        }

        public async Task RestartAsync(CancellationToken cancellationToken = default)
        {
            await _console.Output.WriteLineAsync("Restarting WebHost background worker...");

            await StopAsync(cancellationToken);
            await _console.Output.WriteLineAsync("WebHost background stopped.");

            await StartAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            await _console.Output.WriteLineAsync("Stopping WebHost background worker...");

            if (_webHost != null)
            {
                await _webHost.StopAsync(cancellationToken);
                await _webHost.WaitForShutdownAsync(cancellationToken);
                _webHost.Dispose();
                _webHost = null;
            }

            _serviceScope?.Dispose();
            _serviceScope = null;

            StartupTime = DateTime.MinValue;
        }

        public T GetService<T>()
        {
            return ServiceScope.ServiceProvider.GetRequiredService<T>();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (_webHost != null)
                    {
                        _console.Output.WriteLine("Stopping WebHost background worker...");

                        _webHost.StopAsync().GetAwaiter().GetResult();
                        _webHost.Dispose();
                        _webHost = null;
                    }

                    _serviceScope?.Dispose();
                    _serviceScope = null;

                    StartupTime = DateTime.MinValue;
                }

                // free unmanaged resources (unmanaged objects) and override finalizer
                // set large fields to null
                disposedValue = true;
            }
        }

        // override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~WebHostProviderService()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
