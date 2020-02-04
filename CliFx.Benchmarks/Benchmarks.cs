using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;
using CliFx.Benchmarks.Commands;
using CommandLine;

namespace CliFx.Benchmarks
{
    [SimpleJob]
    [RankColumn]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    public class Benchmarks
    {
        private static readonly string[] Arguments = {"--str", "hello world", "-i", "13", "-b"};

        [Benchmark(Description = "CliFx", Baseline = true)]
        public async ValueTask<int> ExecuteWithCliFx() =>
            await new CliApplicationBuilder().AddCommand(typeof(CliFxCommand)).Build().RunAsync(Arguments);

        [Benchmark(Description = "System.CommandLine")]
        public async Task<int> ExecuteWithSystemCommandLine() =>
            await new SystemCommandLineCommand().ExecuteAsync(Arguments);

        [Benchmark(Description = "McMaster.Extensions.CommandLineUtils")]
        public int ExecuteWithMcMaster() =>
            McMaster.Extensions.CommandLineUtils.CommandLineApplication.Execute<McMasterCommand>(Arguments);

        [Benchmark(Description = "CommandLineParser")]
        public void ExecuteWithCommandLineParser() =>
            new Parser()
                .ParseArguments(Arguments, typeof(CommandLineParserCommand))
                .WithParsed<CommandLineParserCommand>(c => c.Execute());

        [Benchmark(Description = "PowerArgs")]
        public void ExecuteWithPowerArgs() =>
            PowerArgs.Args.InvokeMain<PowerArgsCommand>(Arguments);

        [Benchmark(Description = "Clipr")]
        public void ExecuteWithClipr() =>
            clipr.CliParser.Parse<CliprCommand>(Arguments).Execute();

        [Benchmark(Description = "Cocona")]
        public void ExecuteWithCocona() =>
            Cocona.CoconaApp.Run<CoconaCommand>(Arguments);

        public static void Main() =>
            BenchmarkRunner.Run<Benchmarks>(DefaultConfig.Instance.With(ConfigOptions.DisableOptimizationsValidator));
    }
}