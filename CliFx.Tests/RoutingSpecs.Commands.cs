using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Attributes;

namespace CliFx.Tests
{
    public partial class RoutingSpecs
    {
        [Command]
        public class BenchmarkCliFxCommand : ICommand
        {
            [CommandOption("str", 's')]
            public string? StrOption { get; set; }

            [CommandOption("int", 'i')]
            public int IntOption { get; set; }

            [CommandOption("bool", 'b')]
            public bool BoolOption { get; set; }

            public ValueTask ExecuteAsync(IConsole console) => default;
        }

        [Command]
        private class DefaultCommand : ICommand
        {
            public ValueTask ExecuteAsync(IConsole console)
            {
                console.Output.WriteLine("Hello world!");
                return default;
            }
        }

        [Command("concat", Description = "Concatenate strings.")]
        private class ConcatCommand : ICommand
        {
            [CommandOption('i', IsRequired = true, Description = "Input strings.")]
            public IReadOnlyList<string> Inputs { get; set; } = Array.Empty<string>();

            [CommandOption('s', Description = "String separator.")]
            public string Separator { get; set; } = "";

            public ValueTask ExecuteAsync(IConsole console)
            {
                console.Output.WriteLine(string.Join(Separator, Inputs));
                return default;
            }
        }

        [Command("div", Description = "Divide one number by another.")]
        private class DivideCommand : ICommand
        {
            [CommandOption("dividend", 'D', IsRequired = true, Description = "The number to divide.")]
            public double Dividend { get; set; } = 0;

            [CommandOption("divisor", 'd', IsRequired = true, Description = "The number to divide by.")]
            public double Divisor { get; set; } = 0;

            public ValueTask ExecuteAsync(IConsole console)
            {
                console.Output.WriteLine(Dividend / Divisor);
                return default;
            }
        }
    }
}