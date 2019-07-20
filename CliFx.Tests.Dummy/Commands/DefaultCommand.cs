using System.Text;
using CliFx.Attributes;
using CliFx.Models;
using CliFx.Services;

namespace CliFx.Tests.Dummy.Commands
{
    [Command]
    public class DefaultCommand : Command
    {
        [CommandOption("target", 't', Description = "Greeting target.")]
        public string Target { get; set; } = "world";

        [CommandOption('e', Description = "Whether the greeting should be enthusiastic.")]
        public bool IsEnthusiastic { get; set; }

        protected override ExitCode Process()
        {
            var buffer = new StringBuilder();

            buffer.Append("Hello ").Append(Target);

            if (IsEnthusiastic)
                buffer.Append("!!!");

            Output.WriteLine(buffer.ToString());

            return ExitCode.Success;
        }
    }
}