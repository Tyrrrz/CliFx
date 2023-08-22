using System.Diagnostics;

namespace CliFx.Utils;

internal static class ProcessEx
{
    public static int GetCurrentProcessId()
    {
        using var process = Process.GetCurrentProcess();
        return process.Id;
    }
}
