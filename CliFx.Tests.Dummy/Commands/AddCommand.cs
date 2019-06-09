using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CliFx.Attributes;
using CliFx.Models;

namespace CliFx.Tests.Dummy.Commands
{
    [Command("add")]
    public class AddCommand : Command
    {
        [CommandOption("values", 'v', IsRequired = true, Description = "Values.")]
        public IReadOnlyList<double> Values { get; set; }

        public override ExitCode Execute()
        {
            var result = Values.Sum();
            Console.WriteLine(result.ToString(CultureInfo.InvariantCulture));

            return ExitCode.Success;
        }
    }
}