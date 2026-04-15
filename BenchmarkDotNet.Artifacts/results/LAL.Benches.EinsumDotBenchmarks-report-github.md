```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.26100.2314)
11th Gen Intel Core i7-11700 2.50GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 9.0.312
  [Host]     : .NET 8.0.25 (8.0.2526.11203), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
  Job-TPKMEW : .NET 8.0.25 (8.0.2526.11203), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI

Runtime=.NET 8.0  InvocationCount=1  IterationCount=1  
LaunchCount=1  UnrollFactor=1  WarmupCount=1  

```
| Method            | Length | Mean      | Error | Allocated |
|------------------ |------- |----------:|------:|----------:|
| **DotEvaluateDouble** | **256**    |  **4.000 μs** |    **NA** |     **400 B** |
| **DotEvaluateDouble** | **1024**   |  **8.800 μs** |    **NA** |     **400 B** |
| **DotEvaluateDouble** | **4096**   | **34.000 μs** |    **NA** |     **400 B** |
