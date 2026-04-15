```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.26100.2314)
11th Gen Intel Core i7-11700 2.50GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 9.0.312
  [Host]     : .NET 8.0.25 (8.0.2526.11203), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
  DefaultJob : .NET 8.0.25 (8.0.2526.11203), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI


```
| Method                    | Size  | Mean       | Error     | StdDev    | Gen0   | Allocated |
|-------------------------- |------ |-----------:|----------:|----------:|-------:|----------:|
| **CorrelationDoubleParallel** | **4096**  |   **3.259 μs** | **0.0635 μs** | **0.0951 μs** |      **-** |         **-** |
| **CorrelationDoubleParallel** | **65536** | **286.459 μs** | **5.5703 μs** | **8.9950 μs** | **1.2207** |   **10786 B** |
