using System.Collections.Generic;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using CliFx.Binding;
using CliFx.Infrastructure;

namespace CliFx.Benchmarks;

public partial class Benchmarks
{
    [Command]
    private partial class CliFxCommand : ICommand
    {
        [CommandOption("str", 's')]
        public string? StrOption { get; set; }

        [CommandOption("int", 'i')]
        public int IntOption { get; set; }

        [CommandOption("bool", 'b')]
        public bool BoolOption { get; set; }

        public ValueTask ExecuteAsync(IConsole console) => default;
    }

    [Benchmark(Description = "CliFx", Baseline = true)]
    public async ValueTask<int> ExecuteWithCliFx() =>
        await new CommandLineApplicationBuilder()
            .AddCommand(CliFxCommand.Descriptor)
            .Build()
            .RunAsync(Arguments, new Dictionary<string, string>());
}
