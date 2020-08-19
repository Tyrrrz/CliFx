using System.Collections.Generic;
using CliFx.Attributes;

namespace CliFx.Tests.Commands
{
    [Command("cmd")]
    public class WithStringArrayOptionCommand : SelfSerializeCommandBase
    {
        [CommandOption("opt", 'o')]
        public IReadOnlyList<string>? Opt { get; set; }
    }
}