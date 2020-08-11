using System;
using Microsoft.Extensions.DependencyInjection;

namespace CliFx.BlazorDemo
{
    public class CliStartup : ICliStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            throw new NotImplementedException();
        }

        public void Configure(CliApplicationBuilder app)
        {
            throw new NotImplementedException();
        }
    }
}
