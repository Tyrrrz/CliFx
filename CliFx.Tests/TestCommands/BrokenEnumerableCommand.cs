using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Tests.TestCustomTypes;

namespace CliFx.Tests.TestCommands
{
    [Command]
    public class BrokenEnumerableCommand : ICommand
    {
        [CommandParameter(0)]
        public TestCustomEnumerable<string>? Test { get; set; }

        public ValueTask ExecuteAsync(IConsole console) => default;
    }
}