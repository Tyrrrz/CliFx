using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Services;

namespace CliFx.Tests.TestCommands
{
    [Command]
    public class MultipleEnumerableArgumentsCommand
    {
        [CommandParameter(0)]
        public IReadOnlyList<string>? ArgumentA { get; set; }

        [CommandParameter(1)]
        public IReadOnlyList<string>? ArgumentB { get; set; }

        public ValueTask ExecuteAsync(IConsole console) => default;
    }
}