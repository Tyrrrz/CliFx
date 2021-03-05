using BenchmarkDotNet.Attributes;
using McMaster.Extensions.CommandLineUtils;

namespace CliFx.Benchmarks
{
    public partial class Benchmarks
    {
        public class McMasterCommand
        {
            [Option("--str|-s")]
            public string? StrOption { get; set; }

            [Option("--int|-i")]
            public int IntOption { get; set; }

            [Option("--bool|-b")]
            public bool BoolOption { get; set; }

            public int OnExecute() => 0;
        }

        [Benchmark(Description = "McMaster.Extensions.CommandLineUtils")]
        public int ExecuteWithMcMaster() => CommandLineApplication.Execute<McMasterCommand>(Arguments);
    }
}