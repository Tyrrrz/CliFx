using System;
using System.Globalization;
using CliFx.Attributes;
using CliFx.Models;

namespace CliFx.Tests.Dummy.Commands
{
    [Command("add")]
    public class AddCommand : Command
    {
        [CommandOption("a", IsRequired = true, Description = "Left operand.")]
        public double A { get; set; }

        [CommandOption("b", IsRequired = true, Description = "Right operand.")]
        public double B { get; set; }

        public override ExitCode Execute()
        {
            var result = A + B;
            Console.WriteLine(result.ToString(CultureInfo.InvariantCulture));

            return ExitCode.Success;
        }
    }
}