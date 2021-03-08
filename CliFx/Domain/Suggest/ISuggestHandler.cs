using CliFx.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CliFx.Domain.Suggest
{
    internal interface ISuggestHandler
    {
        bool StopProcessing { get; }
        void Execute(SuggestionData data);
    }
}
