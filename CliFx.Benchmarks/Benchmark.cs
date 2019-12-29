using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using CliFx.Benchmarks.Commands;

namespace CliFx.Benchmarks
{
    [SimpleJob]
    [RankColumn]
    public class Benchmark
    {
        private static readonly string[] Arguments = {"--str", "hello world", "-i", "13", "-b"};

        [Benchmark(Description = "CliFx", Baseline = true)]
        public async ValueTask<int> ExecuteWithCliFx() => await new CliApplicationBuilder().AddCommand(typeof(CliFxCommand)).Build().RunAsync(Arguments);

        [Benchmark(Description = "System.CommandLine")]
        public async ValueTask<int> ExecuteWithSystemCommandLine() => await new SystemCommandLineCommand().ExecuteAsync(Arguments);

        [Benchmark(Description = "McMaster.Extensions.CommandLineUtils")]
        public int ExecuteWithMcMaster() => McMaster.Extensions.CommandLineUtils.CommandLineApplication.Execute<McMasterCommand>(Arguments);

        [Benchmark(Description = "CommandLineParser")]
        public void ExecuteWithCommandLineParser()
        {
            var parsed = new CommandLine.Parser().ParseArguments(Arguments, typeof(CommandLineParserCommand));
            CommandLine.ParserResultExtensions.WithParsed<CommandLineParserCommand>(parsed, c => c.Execute());
        }

        [Benchmark(Description = "PowerArgs")]
        public void ExecuteWithPowerArgs() => PowerArgs.Args.InvokeMain<PowerArgsCommand>(Arguments);

        [Benchmark(Description = "Clipr")]
        public void ExecuteWithClipr() => clipr.CliParser.Parse<CliprCommand>(Arguments).Execute();
    }
}