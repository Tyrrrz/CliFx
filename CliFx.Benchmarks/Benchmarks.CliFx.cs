using System.Collections.Generic;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using CliFx.Attributes;
using CliFx.Infrastructure;

namespace CliFx.Benchmarks;

[Command]
public partial class CliFxCommand : ICommand
{
    [CommandOption("str", 's')]
    public string? StrOption { get; set; }

    [CommandOption("int", 'i')]
    public int IntOption { get; set; }

    [CommandOption("bool", 'b')]
    public bool BoolOption { get; set; }

    public ValueTask ExecuteAsync(IConsole console) => default;
}

public partial class Benchmarks
{
    [Benchmark(Description = "CliFx", Baseline = true)]
    public async ValueTask<int> ExecuteWithCliFx() =>
        await new CliApplicationBuilder()
            .AddCommand(CliFxCommand.Schema)
            .Build()
            .RunAsync(Arguments, new Dictionary<string, string>());
}
