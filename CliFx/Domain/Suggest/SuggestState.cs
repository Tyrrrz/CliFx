using System;
using System.Collections.Generic;

namespace CliFx.Domain.Suggest
{
    internal class SuggestState

    {
        public string CommandCandidate { get; set; } = "";

        public int Index { get; set; }
        public IReadOnlyList<string> Arguments { get; set; } = new List<string>();
        public IEnumerable<string> Suggestions { get; internal set; } = new List<string>();

        internal bool IsLastArgument()
        {
            return Index == Arguments.Count - 1;
        }

    }
}
