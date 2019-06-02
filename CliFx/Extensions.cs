using System;
using System.Linq;
using System.Threading.Tasks;

namespace CliFx
{
    public static class Extensions
    {
        public static Task<int> RunAsync(this ICliApplication application) =>
            application.RunAsync(Environment.GetCommandLineArgs().Skip(1).ToArray());
    }
}