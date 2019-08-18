using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using CliFx.Benchmarks.Commands;

namespace CliFx.Benchmarks
{
    [CoreJob]
    [RankColumn]
    public class Benchmark
    {
        private static readonly string[] Arguments = { "--str", "hello world", "-i", "13", "-b" };

        [Benchmark(Description = "CliFx", Baseline = true)]
        public Task<int> ExecuteWithCliFx() => new CliApplicationBuilder().AddCommand(typeof(CliFxCommand)).Build().RunAsync(Arguments);

        [Benchmark(Description = "System.CommandLine")]
        public Task<int> ExecuteWithSystemCommandLine() => new SystemCommandLineCommand().ExecuteAsync(Arguments);

        [Benchmark(Description = "McMaster.Extensions.CommandLineUtils")]
        public int ExecuteWithMcMaster() => McMaster.Extensions.CommandLineUtils.CommandLineApplication.Execute<McMasterCommand>(Arguments);

        // Skipped because this benchmark freezes after a couple of iterations
        // Probably wasn't designed to run multiple times in single process execution
        //[Benchmark(Description = "CommandLineParser")]
        public void ExecuteWithCommandLineParser()
        {
            var parsed = CommandLine.Parser.Default.ParseArguments(Arguments, typeof(CommandLineParserCommand));
            CommandLine.ParserResultExtensions.WithParsed<CommandLineParserCommand>(parsed, c => c.Execute());
        }

        [Benchmark(Description = "PowerArgs")]
        public void ExecuteWithPowerArgs() => PowerArgs.Args.InvokeMain<PowerArgsCommand>(Arguments);
    }
}