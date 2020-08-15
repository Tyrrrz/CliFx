using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CliFx.Attributes;
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
            console.Output.WriteLine(new string('=', 105));

            console.Output.Write(" Service Type".PadRight(41));
            console.Output.Write('|');
            console.Output.Write(" ImplementationType".PadRight(41));
            console.Output.Write('|');
            console.Output.WriteLine(" Lifetime".PadRight(21));

            console.Output.WriteLine(new string('=', 105));

            ServiceLifetime? lastLifetime = null;
            foreach (ServiceDescriptor item in serviceDescriptors.OrderBy(x => x.Lifetime)
                                                                 .ThenBy(x => x.ServiceType.Name)
                                                                 .ThenBy(x => x.ImplementationType?.Name))
            {
                if (lastLifetime is null)
                    lastLifetime = item.Lifetime;
                else if (lastLifetime != item.Lifetime)
                {
                    console.Output.WriteLine(new string('-', 105));
                    lastLifetime = item.Lifetime;
                }

                console.Output.Write(' ');
                console.Output.Write(item.ServiceType.Name.PadRight(40));
                console.Output.Write('|');
                console.Output.Write(' ');
                console.Output.Write(item.ImplementationType?.Name.PadRight(40) ?? string.Empty.PadRight(40));
                console.Output.Write('|');
                console.Output.Write(' ');
                console.Output.WriteLine(item.Lifetime.ToString().PadRight(20));
            }

            console.Output.WriteLine(new string('=', 105));
        }
    }
}
