using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Services;

namespace CliFx.Tests.Dummy.Commands
{
    [Command("sum", Description = "Calculate the sum of all input values.")]
    public class SumCommand : ICommand
    {
        [CommandOption("values", 'v', IsRequired = true, Description = "Input values.")]
        public IReadOnlyList<double> Values { get; set; }

        public Task ExecuteAsync(IConsole console)
        {
            var result = Values.Sum();
            console.Output.WriteLine(result);

            return Task.CompletedTask;
        }
    }
}