using System.Collections.Generic;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;
using CliFx.Benchmarks.Commands;
using CliFx.Benchmarks.Commands.CliFxCommands;
using CommandLine;

namespace CliFx.Benchmarks
{
    [SimpleJob]
    [RankColumn]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    public class Benchmarks
    {
        private static readonly string[] Arguments = { "--str", "hello world", "-i", "13", "-b" };

        [Benchmark(Description = "CliFx - 1 command", Baseline = true)]
        public async ValueTask<int> ExecuteWithCliFxDefaultCommandOnly()
        {
            return await new CliApplicationBuilder().AddCommand(typeof(CliFxCommand))
                                                    .Build()
                                                    .RunAsync(Arguments, new Dictionary<string, string>());
        }

        [Benchmark(Description = "CliFx - 2 commands")]
        public async ValueTask<int> ExecuteWithCliFx2Commands()
        {
            return await new CliApplicationBuilder().AddCommand(typeof(CliFxCommand))
                                                    .AddCommand(typeof(CliFxNamedCommandCommand))
                                                    .Build()
                                                    .RunAsync(Arguments, new Dictionary<string, string>());
        }

        [Benchmark(Description = "CliFx - 10 commands")]
        public async ValueTask<int> ExecuteWithCliFx10Commands()
        {
            return await new CliApplicationBuilder().AddCommand(typeof(CliFxCommand))
                                                    .AddCommand(typeof(CliFxNamedCommandCommand))
                                                    .AddCommand(typeof(CliFxNamedCommandCommand00))
                                                    .AddCommand(typeof(CliFxNamedCommandCommand01))
                                                    .AddCommand(typeof(CliFxNamedCommandCommand02))
                                                    .AddCommand(typeof(CliFxNamedCommandCommand03))
                                                    .AddCommand(typeof(CliFxNamedCommandCommand04))
                                                    .AddCommand(typeof(CliFxNamedCommandCommand05))
                                                    .AddCommand(typeof(CliFxNamedCommandCommand06))
                                                    .AddCommand(typeof(CliFxNamedCommandCommand07))
                                                    .Build()
                                                    .RunAsync(Arguments, new Dictionary<string, string>());
        }

        [Benchmark(Description = "CliFx - 22 commands")]
        public async ValueTask<int> ExecuteWithCliFxAllCommands()
        {
            return await new CliApplicationBuilder().AddCommandsFromThisAssembly()
                                                    .Build()
                                                    .RunAsync(Arguments, new Dictionary<string, string>());
        }

        [Benchmark(Description = "System.CommandLine")]
        public async Task<int> ExecuteWithSystemCommandLine()
        {
            return await new SystemCommandLineCommand().ExecuteAsync(Arguments);
        }

        [Benchmark(Description = "McMaster.Extensions.CommandLineUtils")]
        public int ExecuteWithMcMaster()
        {
            return McMaster.Extensions.CommandLineUtils.CommandLineApplication.Execute<McMasterCommand>(Arguments);
        }

        [Benchmark(Description = "CommandLineParser")]
        public void ExecuteWithCommandLineParser()
        {
            new Parser()
                .ParseArguments(Arguments, typeof(CommandLineParserCommand))
                .WithParsed<CommandLineParserCommand>(c => c.Execute());
        }

        [Benchmark(Description = "PowerArgs")]
        public void ExecuteWithPowerArgs()
        {
            PowerArgs.Args.InvokeMain<PowerArgsCommand>(Arguments);
        }

        [Benchmark(Description = "Clipr")]
        public void ExecuteWithClipr()
        {
            clipr.CliParser.Parse<CliprCommand>(Arguments).Execute();
        }

        [Benchmark(Description = "Cocona")]
        public void ExecuteWithCocona()
        {
            Cocona.CoconaApp.Run<CoconaCommand>(Arguments);
        }

        public static void Main()
        {
            BenchmarkRunner.Run<Benchmarks>(DefaultConfig.Instance.With(ConfigOptions.DisableOptimizationsValidator));
        }
    }
}