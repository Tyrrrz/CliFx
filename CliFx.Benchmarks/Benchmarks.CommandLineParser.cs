using BenchmarkDotNet.Attributes;
using CommandLine;

namespace CliFx.Benchmarks
{
    public partial class Benchmarks
    {
        public class CommandLineParserCommand
        {
            [Option('s', "str")]
            public string? StrOption { get; set; }

            [Option('i', "int")]
            public int IntOption { get; set; }

            [Option('b', "bool")]
            public bool BoolOption { get; set; }

            public void Execute()
            {
            }
        }

        [Benchmark(Description = "CommandLineParser")]
        public void ExecuteWithCommandLineParser() =>
            new Parser()
                .ParseArguments(Arguments, typeof(CommandLineParserCommand))
                .WithParsed<CommandLineParserCommand>(c => c.Execute());
    }
}