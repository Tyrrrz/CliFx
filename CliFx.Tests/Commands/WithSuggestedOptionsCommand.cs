using System.Collections.Generic;
using CliFx.Attributes;

namespace CliFx.Tests.Commands
{
    [Command("cmd")]
    public class WithSuggestedOptionsCommand : SelfSerializeCommandBase
    {
        [CommandOption("opt-aa", 'a', IsRequired = true)]
        public string? OptA { get; set; }

        [CommandOption("opt-bb", 'b')]
        public int? OptB { get; set; }

        [CommandOption("opt-cc", 'c', IsRequired = true)]
        public IReadOnlyList<char>? OptC { get; set; }
    }
}