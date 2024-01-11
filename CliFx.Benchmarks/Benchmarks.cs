using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;

namespace CliFx.Benchmarks;

[RankColumn]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public partial class Benchmarks
{
    private static readonly string[] Arguments = ["--str", "hello world", "-i", "13", "-b"];

    public static void Main() =>
        BenchmarkRunner.Run<Benchmarks>(
            DefaultConfig.Instance.WithOptions(ConfigOptions.DisableOptimizationsValidator)
        );
}
