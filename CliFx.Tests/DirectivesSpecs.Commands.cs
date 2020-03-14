using System.Threading.Tasks;
using CliFx.Attributes;

namespace CliFx.Tests
{
    public partial class DirectivesSpecs
    {
        [Command("cmd")]
        private class NamedCommand : ICommand
        {
            public ValueTask ExecuteAsync(IConsole console) => default;
        }
    }
}