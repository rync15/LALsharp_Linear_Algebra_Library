```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.26100.2314)
11th Gen Intel Core i7-11700 2.50GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 9.0.312
  [Host]     : .NET 8.0.25 (8.0.2526.11203), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
  Job-TPKMEW : .NET 8.0.25 (8.0.2526.11203), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI

Runtime=.NET 8.0  InvocationCount=1  IterationCount=1  
LaunchCount=1  UnrollFactor=1  WarmupCount=1  

```
| Method         | Length | Mean         | Error | Allocated |
|--------------- |------- |-------------:|------:|----------:|
| **QuantileDouble** | **64**     |   **7,200.0 ns** |    **NA** |     **400 B** |
| QuantileFloat  | 64     |   6,600.0 ns |    NA |     400 B |
| MeanDouble     | 64     |     850.0 ns |    NA |     400 B |
| **QuantileDouble** | **256**    |  **30,850.0 ns** |    **NA** |     **400 B** |
| QuantileFloat  | 256    |  29,600.0 ns |    NA |     400 B |
| MeanDouble     | 256    |   2,300.0 ns |    NA |     400 B |
| **QuantileDouble** | **1024**   | **135,200.0 ns** |    **NA** |    **8952 B** |
| QuantileFloat  | 1024   | 174,700.0 ns |    NA |    4856 B |
| MeanDouble     | 1024   |   9,800.0 ns |    NA |     400 B |
