```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.26100.2314)
11th Gen Intel Core i7-11700 2.50GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 9.0.312
  [Host]     : .NET 8.0.25 (8.0.2526.11203), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
  DefaultJob : .NET 8.0.25 (8.0.2526.11203), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI


```
| Method                    | Size | Mean      | Error    | StdDev   | Median    | Gen0   | Allocated |
|-------------------------- |----- |----------:|---------:|---------:|----------:|-------:|----------:|
| **RbfWeightsSequential**      | **64**   |  **57.99 μs** | **0.425 μs** | **0.377 μs** |  **57.93 μs** |      **-** |      **50 B** |
| RbfWeightsParallel        | 64   |  57.93 μs | 0.844 μs | 0.705 μs |  58.01 μs |      - |      50 B |
| RbfWeightsFloatSequential | 64   |  38.30 μs | 0.364 μs | 0.323 μs |  38.26 μs |      - |      50 B |
| RbfWeightsFloatParallel   | 64   |  39.10 μs | 0.501 μs | 0.469 μs |  38.98 μs |      - |      50 B |
| RbfWeightsComplexParallel | 64   |  89.56 μs | 0.913 μs | 0.762 μs |  89.77 μs |      - |      56 B |
| **RbfWeightsSequential**      | **128**  | **230.98 μs** | **2.702 μs** | **2.395 μs** | **230.69 μs** |      **-** |      **80 B** |
| RbfWeightsParallel        | 128  | 208.01 μs | 3.623 μs | 3.026 μs | 207.39 μs | 0.2441 |    3193 B |
| RbfWeightsFloatSequential | 128  | 154.54 μs | 3.075 μs | 6.486 μs | 157.45 μs |      - |      80 B |
| RbfWeightsFloatParallel   | 128  | 119.46 μs | 2.354 μs | 3.375 μs | 118.36 μs | 0.3662 |    3285 B |
| RbfWeightsComplexParallel | 128  | 341.57 μs | 6.651 μs | 7.393 μs | 339.65 μs |      - |    2811 B |
