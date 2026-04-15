```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.26100.2314)
11th Gen Intel Core i7-11700 2.50GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 9.0.312
  [Host]     : .NET 8.0.25 (8.0.2526.11203), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
  Job-MIHPTF : .NET 8.0.25 (8.0.2526.11203), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI

InvocationCount=1  UnrollFactor=1  

```
| Method       | Size | Mean     | Error    | StdDev   | Median   | Allocated |
|------------- |----- |---------:|---------:|---------:|---------:|----------:|
| **GemmMultiply** | **64**   | **122.4 μs** |  **2.04 μs** |  **2.26 μs** | **122.6 μs** |     **488 B** |
| **GemmMultiply** | **128**  | **696.3 μs** | **17.33 μs** | **50.29 μs** | **681.1 μs** |     **488 B** |
