using CliFx.Attributes;
using CliFx.Models;

namespace CliFx.Tests.TestObjects
{
    [DefaultCommand]
    [Command("command")]
    public class TestCommand : Command
    {
        [CommandOption("int", 'i', IsRequired = true)]
        public int IntOption { get; set; } = 24;

        [CommandOption("str", 's')]
        public string StringOption { get; set; } = "foo bar";

        public override ExitCode Execute() => new ExitCode(IntOption, StringOption);
    }
}