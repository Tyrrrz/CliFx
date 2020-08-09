using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace CliFx.BlazorDemo.CLI.Services
{
    public enum WebHostStatuses
    {
        Stopped,
        Running
    }

    public interface IBackgroundWebHostProvider : IDisposable
    {
        IWebHost WebHost { get; }
        IServiceScope ServiceScope { get; }

        DateTime StartupTime { get; }
        TimeSpan Runtime { get; }
        WebHostStatuses Status { get; }

        Task StartAsync();
        Task RestartAsync(CancellationToken cancellationToken = default);
        Task StopAsync(CancellationToken cancellationToken = default);

        T GetService<T>();
    }
}
