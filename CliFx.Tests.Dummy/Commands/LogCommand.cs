using System;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Models;
using CliFx.Services;

namespace CliFx.Tests.Dummy.Commands
{
    [Command("log", Description = "Calculates the logarithm of a value.")]
    public class LogCommand : ICommand
    {
        [CommandOption("value", 'v', IsRequired = true, Description = "Value whose logarithm is to be found.")]
        public double Value { get; set; }

        [CommandOption("base", 'b', Description = "Logarithm base.")]
        public double Base { get; set; } = 10;

        public Task ExecuteAsync(CommandContext context)
        {
            var result = Math.Log(Value, Base);
            context.Output.WriteLine(result);

            return Task.CompletedTask;
        }
    }
}