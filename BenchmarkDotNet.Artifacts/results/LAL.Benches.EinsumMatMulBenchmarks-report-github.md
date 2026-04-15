```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.26100.2314)
11th Gen Intel Core i7-11700 2.50GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 9.0.312
  [Host]     : .NET 8.0.25 (8.0.2526.11203), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
  Job-TPKMEW : .NET 8.0.25 (8.0.2526.11203), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI

Runtime=.NET 8.0  InvocationCount=1  IterationCount=1  
LaunchCount=1  UnrollFactor=1  WarmupCount=1  

```
| Method                           | Size | Mean        | Error | Allocated |
|--------------------------------- |----- |------------:|------:|----------:|
| **MatMulEvaluateDouble_CenterValue** | **32**   |    **85.80 μs** |    **NA** |  **16.84 KB** |
| **MatMulEvaluateDouble_CenterValue** | **64**   | **1,533.00 μs** |    **NA** |  **68.58 KB** |
