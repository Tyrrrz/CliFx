using System.Collections.Generic;

namespace CliFx.Domain.Suggest
{
    abstract class AbstractSuggestHandler : ISuggestHandler
    {
        public bool StopProcessing { get; protected set; }

        public IEnumerable<string> Suggestions { get; protected set; } = new List<string>();

        public abstract void Execute(SuggestionData data);

        public IEnumerable<string> NoSuggestions()
        {
            return new List<string>();
        }
    }
}
