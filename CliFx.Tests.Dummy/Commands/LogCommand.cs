using System;
using System.Globalization;
using CliFx.Attributes;
using CliFx.Models;
using CliFx.Services;

namespace CliFx.Tests.Dummy.Commands
{
    [Command("log", Description = "Calculate the logarithm of a value.")]
    public class LogCommand : Command
    {
        [CommandOption("value", 'v', IsRequired = true, Description = "Value whose logarithm is to be found.")]
        public double Value { get; set; }

        [CommandOption("base", 'b', Description = "Logarithm base.")]
        public double Base { get; set; } = 10;

        protected override ExitCode Process()
        {
            var result = Math.Log(Value, Base);
            Output.WriteLine(result.ToString(CultureInfo.InvariantCulture));

            return ExitCode.Success;
        }
    }
}