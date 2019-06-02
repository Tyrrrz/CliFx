using System.Collections.Generic;
using System.Threading.Tasks;

namespace CliFx
{
    public interface ICliApplication
    {
        Task<int> RunAsync(IReadOnlyList<string> commandLineArguments);
    }
}