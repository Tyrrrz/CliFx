using System.Diagnostics;

namespace CliFx.Internal
{
    internal static class ProcessEx
    {
        public static int GetCurrentProcessId()
        {
            using Process process = Process.GetCurrentProcess();
            return process.Id;
        }
    }
}