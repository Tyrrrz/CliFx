using System.Collections.Generic;
using CliFx.Models;

namespace CliFx.Services
{
    public interface ICommandInputParser
    {
        CommandInput ParseInput(IReadOnlyList<string> commandLineArguments);
    }
}