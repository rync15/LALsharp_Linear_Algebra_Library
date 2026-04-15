```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.26100.2314)
11th Gen Intel Core i7-11700 2.50GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 9.0.312
  [Host]     : .NET 8.0.25 (8.0.2526.11203), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
  Job-TPKMEW : .NET 8.0.25 (8.0.2526.11203), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI

Runtime=.NET 8.0  InvocationCount=1  IterationCount=1  
LaunchCount=1  UnrollFactor=1  WarmupCount=1  

```
| Method                   | Length | Mean         | Error | Allocated |
|------------------------- |------- |-------------:|------:|----------:|
| **ArgsortDouble_FirstIndex** | **256**    |    **23.100 μs** |    **NA** |    **4368 B** |
| LexsortDouble_FirstIndex | 256    |    28.700 μs |    NA |    4376 B |
| NonZeroDouble_IntoSpan   | 256    |     3.300 μs |    NA |     400 B |
| Where_IntoSpan           | 256    |     2.500 μs |    NA |     400 B |
| **ArgsortDouble_FirstIndex** | **1024**   |   **112.100 μs** |    **NA** |   **13584 B** |
| LexsortDouble_FirstIndex | 1024   |   139.700 μs |    NA |   13592 B |
| NonZeroDouble_IntoSpan   | 1024   |    12.450 μs |    NA |     400 B |
| Where_IntoSpan           | 1024   |     9.800 μs |    NA |     400 B |
| **ArgsortDouble_FirstIndex** | **4096**   | **1,265.900 μs** |    **NA** |   **50448 B** |
| LexsortDouble_FirstIndex | 4096   | 1,544.100 μs |    NA |   50456 B |
| NonZeroDouble_IntoSpan   | 4096   |   300.300 μs |    NA |     400 B |
| Where_IntoSpan           | 4096   |   379.000 μs |    NA |     400 B |
