```

BenchmarkDotNet v0.14.0, Windows 10 (10.0.19045.5131/22H2/2022Update)
Intel Core i7-7700K CPU 4.20GHz (Kaby Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK 9.0.100
  [Host]     : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX2
  Job-MOOLFR : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX2

InvocationCount=1  UnrollFactor=1  

```
| Method                  | Mean     | Error     | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
|------------------------ |---------:|----------:|----------:|------:|--------:|----------:|------------:|
| QuantizePerTileOriginal | 5.534 ms | 0.2171 ms | 0.6230 ms |  1.01 |    0.16 | 326.37 KB |        1.00 |
