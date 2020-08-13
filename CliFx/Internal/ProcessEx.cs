namespace CliFx.Internal
{
    using System.Diagnostics;

    internal static class ProcessEx
    {
        public static int GetCurrentProcessId()
        {
            using Process process = Process.GetCurrentProcess();

            return process.Id;
        }
    }
}