using System.Text;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Models;
using CliFx.Services;

namespace CliFx.Tests.Dummy.Commands
{
    [Command]
    public class GreeterCommand : ICommand
    {
        [CommandOption("target", 't', Description = "Greeting target.")]
        public string Target { get; set; } = "world";

        [CommandOption('e', Description = "Whether the greeting should be exclaimed.")]
        public bool IsExclaimed { get; set; }

        public Task ExecuteAsync(CommandContext context)
        {
            var buffer = new StringBuilder();

            buffer.Append("Hello").Append(' ').Append(Target);

            if (IsExclaimed)
                buffer.Append('!');

            context.Output.WriteLine(buffer.ToString());

            return Task.CompletedTask;
        }
    }
}