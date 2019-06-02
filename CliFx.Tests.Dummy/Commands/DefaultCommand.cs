using System;
using System.Text;
using CliFx.Attributes;
using CliFx.Models;

namespace CliFx.Tests.Dummy.Commands
{
    [DefaultCommand]
    public class DefaultCommand : Command
    {
        [CommandOption("target", ShortName = 't', Description = "Greeting target.")]
        public string Target { get; set; } = "world";

        [CommandOption("enthusiastic", ShortName = 'e', Description = "Whether the greeting should be enthusiastic.")]
        public bool IsEnthusiastic { get; set; }

        public override ExitCode Execute()
        {
            var buffer = new StringBuilder();

            buffer.Append("Hello ").Append(Target);

            if (IsEnthusiastic)
                buffer.Append("!!!");

            Console.WriteLine(buffer.ToString());

            return ExitCode.Success;
        }
    }
}