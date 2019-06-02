using System.Collections.Generic;
using CliFx.Models;

namespace CliFx.Services
{
    public interface ICommandOptionParser
    {
        CommandOptionSet ParseOptions(IReadOnlyList<string> commandLineArguments);
    }
}