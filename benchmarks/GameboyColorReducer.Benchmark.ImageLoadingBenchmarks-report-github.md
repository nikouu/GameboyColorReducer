```

BenchmarkDotNet v0.14.0, Windows 10 (10.0.19045.5131/22H2/2022Update)
Intel Core i7-7700K CPU 4.20GHz (Kaby Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK 9.0.100
  [Host]     : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX2


```
| Method         | Mean     | Error    | StdDev   | Median   | Ratio | RatioSD | Gen0      | Gen1      | Gen2     | Allocated | Alloc Ratio |
|--------------- |---------:|---------:|---------:|---------:|------:|--------:|----------:|----------:|---------:|----------:|------------:|
| ToWorkingImage | 17.08 ms | 0.350 ms | 1.005 ms | 16.80 ms |  1.00 |    0.08 | 2562.5000 | 1562.5000 | 500.0000 |  12.46 MB |        1.00 |
