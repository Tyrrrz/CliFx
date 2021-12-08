using BenchmarkDotNet.Attributes;
using Cocona;

namespace CliFx.Benchmarks;

public partial class Benchmarks
{
    public class CoconaCommand
    {
        public void Execute(
            [Option("str", new []{'s'})]
            string? strOption,
            [Option("int", new []{'i'})]
            int intOption,
            [Option("bool", new []{'b'})]
            bool boolOption)
        {
        }
    }

    [Benchmark(Description = "Cocona")]
    public void ExecuteWithCocona() => CoconaApp.Run<CoconaCommand>(Arguments);
}