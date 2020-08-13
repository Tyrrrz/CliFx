namespace CliFx.Tests.Internal
{
    using System;
    using System.Threading.Tasks;

    internal static class TaskExtensions
    {
        public static async Task IgnoreCancellation(this Task task)
        {
            try
            {
                await task;
            }
            catch (OperationCanceledException)
            {

            }
        }
    }
}