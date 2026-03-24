using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace CliFx.Utils.Extensions;

internal static class DebuggerExtensions
{
    extension(Debugger)
    {
        public static async ValueTask WaitUntilAttachedAsync(
            CancellationToken cancellationToken = default
        )
        {
            while (!Debugger.IsAttached)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Delay(100, cancellationToken);
            }
        }
    }
}
