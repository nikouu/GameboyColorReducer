```

BenchmarkDotNet v0.14.0, Windows 10 (10.0.19045.5131/22H2/2022Update)
Intel Core i7-7700K CPU 4.20GHz (Kaby Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK 9.0.100
  [Host]     : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX2
  Job-FXILUS : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX2

InvocationCount=1  UnrollFactor=1  

```
| Method                  | Mean     | Error     | StdDev   | Median   | Ratio | RatioSD | Allocated | Alloc Ratio |
|------------------------ |---------:|----------:|---------:|---------:|------:|--------:|----------:|------------:|
| QuantizePerTileOriginal | 6.344 ms | 0.3561 ms | 1.016 ms | 6.046 ms |  1.02 |    0.22 |   1.23 MB |        1.00 |
