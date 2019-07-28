using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace CliFx.Benchmarks
{
    public static class Program
    {
        public static void Main() =>
            BenchmarkRunner.Run(typeof(Program).Assembly, DefaultConfig.Instance
                .With(ConfigOptions.DisableOptimizationsValidator));
    }
}