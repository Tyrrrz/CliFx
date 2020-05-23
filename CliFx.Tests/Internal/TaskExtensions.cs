using System;
using System.Threading.Tasks;

namespace CliFx.Tests.Internal
{
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