using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Services;

namespace CliFx.Tests.TestCommands
{
    [Command("concat", Description = "Concatenate strings.")]
    public class ConcatCommand : ICommand
    {
        [CommandOption('i', IsRequired = true, Description = "Input strings.")]
        public IReadOnlyList<string> Inputs { get; set; } = new string[0];

        [CommandOption('s', Description = "String separator.")]
        public string Separator { get; set; } = ""; 
        
        public ValueTask ExecuteAsync(IConsole console)
        {
            console.Output.WriteLine(string.Join(Separator, Inputs));
            return default;
        }
    }
}