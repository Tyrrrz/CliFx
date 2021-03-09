﻿using System.Collections.Generic;

namespace CliFx.Domain.Suggest
{
    abstract class AbstractSuggestHandler : ISuggestHandler
    {
        public bool StopProcessing { get; protected set; }

        public abstract void Execute(SuggestState state);

        public IEnumerable<string> NoSuggestions()
        {
            return new List<string>();
        }
    }
}