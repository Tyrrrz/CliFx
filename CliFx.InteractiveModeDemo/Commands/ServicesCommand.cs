using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace CliFx.InteractiveModeDemo.Commands
{
    [Command("services", Description = "Prints a list of registered services in application.")]
    public class ServicesCommand : ICommand
    {
        private readonly ICliContext _cliContext;

        public ServicesCommand(ICliContext cliContext)
        {
            _cliContext = cliContext;
        }

        public ValueTask ExecuteAsync(IConsole console)
        {
            DebugPrintServices(console, _cliContext.Services);

            return default;
        }

        private void DebugPrintServices(IConsole console, IEnumerable<ServiceDescriptor> serviceDescriptors)
        {
            TableUtils.Write(console,
                             serviceDescriptors.OrderBy(x => x.Lifetime)
                                               .ThenBy(x => x.ServiceType.Name)
                                               .ThenBy(x => x.ImplementationType?.Name),
                             new string[] { "Service type", "Implementation type", "F", "I", "Lifetime" },
                             footnotes:
                             "  F - whether implementation factory is used\n" +
                             "  I - whether implementation instanace is used",
                             x => x.ServiceType.Name,
                             x => x.ImplementationType == null ? string.Empty : x.ImplementationType.Name,
                             x => x.ImplementationFactory == null ? string.Empty : "+",
                             x => x.ImplementationInstance == null ? string.Empty : "+",
                             x => x.Lifetime.ToString());
        }
    }
}
