using System.Diagnostics;

namespace CliFx.Internal
{
    internal static class ProcessEx
    {
        public static int GetCurrentProcessId()
        {
            using var process = Process.GetCurrentProcess();
            return process.Id;
        }
    }
}