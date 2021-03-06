## CliFx.Benchmarks

All benchmarks below were ran with the following configuration:

```ini
BenchmarkDotNet=v0.12.0, OS=Windows 10.0.14393.3443 (1607/AnniversaryUpdate/Redstone1)
Intel Core i5-4460 CPU 3.20GHz (Haswell), 1 CPU, 4 logical and 4 physical cores
Frequency=3124994 Hz, Resolution=320.0006 ns, Timer=TSC
.NET Core SDK=3.1.100
  [Host]     : .NET Core 3.1.0 (CoreCLR 4.700.19.56402, CoreFX 4.700.19.56404), X64 RyuJIT
  DefaultJob : .NET Core 3.1.0 (CoreCLR 4.700.19.56402, CoreFX 4.700.19.56404), X64 RyuJIT
```

| Method                               |        Mean |     Error |     StdDev | Ratio | RatioSD | Rank |
| ------------------------------------ | ----------: | --------: | ---------: | ----: | ------: | ---: |
| CommandLineParser                    |    24.79 us |  0.166 us |   0.155 us |  0.49 |    0.00 |    1 |
| CliFx                                |    50.27 us |  0.248 us |   0.232 us |  1.00 |    0.00 |    2 |
| Clipr                                |   160.22 us |  0.817 us |   0.764 us |  3.19 |    0.02 |    3 |
| McMaster.Extensions.CommandLineUtils |   166.45 us |  1.111 us |   1.039 us |  3.31 |    0.03 |    4 |
| System.CommandLine                   |   170.27 us |  0.599 us |   0.560 us |  3.39 |    0.02 |    5 |
| PowerArgs                            |   306.12 us |  1.495 us |   1.398 us |  6.09 |    0.03 |    6 |
| Cocona                               | 1,856.07 us | 48.727 us | 141.367 us | 37.88 |    2.60 |    7 |
