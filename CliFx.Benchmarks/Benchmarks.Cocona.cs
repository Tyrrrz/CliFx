using BenchmarkDotNet.Attributes;
using Cocona;

namespace CliFx.Benchmarks;

public partial class Benchmarks
{
    public class CoconaCommand
    {
        public void Execute(
            [Option("str", ['s'])] string? strOption,
            [Option("int", ['i'])] int intOption,
            [Option("bool", ['b'])] bool boolOption
        ) { }
    }

    [Benchmark(Description = "Cocona")]
    public void ExecuteWithCocona() => CoconaApp.Run<CoconaCommand>(Arguments);
}
