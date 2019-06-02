using System;
using System.Globalization;
using CliFx.Attributes;
using CliFx.Models;

namespace CliFx.Tests.Dummy.Commands
{
    [Command("log")]
    public class LogCommand : Command
    {
        [CommandOption("value", IsRequired = true, Description = "Value whose logarithm is to be found.")]
        public double Value { get; set; }

        [CommandOption("base", Description = "Logarithm base.")]
        public double Base { get; set; } = 10;

        public override ExitCode Execute()
        {
            var result = Math.Log(Value, Base);
            Console.WriteLine(result.ToString(CultureInfo.InvariantCulture));

            return ExitCode.Success;
        }
    }
}