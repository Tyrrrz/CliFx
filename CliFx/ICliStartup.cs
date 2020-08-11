using Microsoft.Extensions.DependencyInjection;

namespace CliFx
{
    /// <summary>
    /// Abstraction for CliFx framework configuration using startup class.
    /// </summary>
    public interface ICliStartup
    {
        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        void ConfigureServices(IServiceCollection services);

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure CliFx framework.
        /// </summary>
        void Configure(CliApplicationBuilder app);
    }
}
