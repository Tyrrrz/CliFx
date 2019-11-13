using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Services;

namespace CliFx.Tests.TestCommands
{
    [Command("div", Description = "Divide one number by another.")]
    public class DivideCommand : ICommand
    {
        [CommandOption("dividend", 'D', IsRequired = true, Description = "The number to divide.")]
        public double Dividend { get; set; }

        [CommandOption("divisor", 'd', IsRequired = true, Description = "The number to divide by.")]
        public double Divisor { get; set; }

        // This property should be ignored by resolver
        public bool NotAnOption { get; set; }
        
        public Task ExecuteAsync(IConsole console)
        {
            console.Output.WriteLine(Dividend / Divisor);
            return Task.CompletedTask;
        }
    }
}