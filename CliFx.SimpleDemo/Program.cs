using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.SimpleDemo.Commands;

namespace CliFx.SimpleDemo
{
    public static class Program
    {
        private static readonly string[] Arguments = { "--str", "hello world", "-i", "13", "-b" };

        public static async Task<int> Main()
        {
            return await new CliApplicationBuilder().AddCommand(typeof(CliFxBenchmarkCommand))
                                                    .Build()
                                                    .RunAsync(Arguments, new Dictionary<string, string>());
        }
    }
}