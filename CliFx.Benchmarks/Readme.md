## CliFx.Benchmarks

All benchmarks below were ran with the following configuration:

```
BenchmarkDotNet v0.15.8, Linux Arch Linux
AMD Ryzen 5 7530U with Radeon Graphics 1.10GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK 10.0.201
  [Host]     : .NET 10.0.5 (10.0.5, 10.0.526.15411), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.5 (10.0.5, 10.0.526.15411), X64 RyuJIT x86-64-v3
```

| Method                               |         Mean |      Error |     StdDev |  Ratio | RatioSD | Rank |
| ------------------------------------ | -----------: | ---------: | ---------: | -----: | ------: | ---: |
| CommandLineParser                    |     1.205 μs |  0.0232 μs |  0.0318 μs |   0.17 |    0.01 |    1 |
| System.CommandLine                   |     2.883 μs |  0.0252 μs |  0.0236 μs |   0.41 |    0.01 |    2 |
| CliFx                                |     7.051 μs |  0.1405 μs |  0.1827 μs |   1.00 |    0.04 |    3 |
| McMaster.Extensions.CommandLineUtils |    37.081 μs |  0.7220 μs |  0.6401 μs |   5.26 |    0.16 |    4 |
| Clipr                                |    48.179 μs |  0.3859 μs |  0.3421 μs |   6.84 |    0.18 |    5 |
| PowerArgs                            |    96.557 μs |  0.5066 μs |  0.4230 μs |  13.70 |    0.36 |    6 |
| Cocona                               | 1,076.073 μs | 15.0159 μs | 12.5389 μs | 152.71 |    4.31 |    7 |
