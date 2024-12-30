```

BenchmarkDotNet v0.14.0, Windows 10 (10.0.19045.5131/22H2/2022Update)
Intel Core i7-7700K CPU 4.20GHz (Kaby Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK 9.0.100
  [Host]     : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX2
  Job-VLCNGR : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX2

InvocationCount=1  UnrollFactor=1  

```
| Method                  | Mean      | Error     | StdDev    | Median    | Ratio | RatioSD | Gen0      | Allocated | Alloc Ratio |
|------------------------ |----------:|----------:|----------:|----------:|------:|--------:|----------:|----------:|------------:|
| QuantizePerTileOriginal |  5.805 ms | 0.1155 ms | 0.3024 ms |  5.700 ms |  1.00 |    0.07 |         - |   1.23 MB |        1.00 |
| QuantizePerTile         | 15.876 ms | 0.3155 ms | 0.6925 ms | 15.666 ms |  2.74 |    0.18 | 2000.0000 |  13.07 MB |       10.59 |
