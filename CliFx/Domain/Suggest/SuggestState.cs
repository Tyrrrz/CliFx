using System.Collections.Generic;

namespace CliFx.Domain.Suggest
{
    internal class SuggestState

    {
        public string Command { get; set; } = "";
        public int Index { get; set; }
        public IReadOnlyList<string> Arguments { get; set; } = new List<string>();
        public IEnumerable<string> Suggestions { get; internal set; } = new List<string>();
    }
}
