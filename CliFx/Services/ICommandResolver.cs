using System.Collections.Generic;

namespace CliFx.Services
{
    public interface ICommandResolver
    {
        Command ResolveCommand(IReadOnlyList<string> commandLineArguments);
    }
}