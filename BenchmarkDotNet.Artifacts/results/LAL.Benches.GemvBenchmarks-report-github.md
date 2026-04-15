```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.26100.2314)
11th Gen Intel Core i7-11700 2.50GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 9.0.312
  [Host]     : .NET 8.0.25 (8.0.2526.11203), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
  Job-YMRDDA : .NET 8.0.25 (8.0.2526.11203), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI

InvocationCount=1  UnrollFactor=1  

```
| Method       | Rows | Cols | Mean     | Error    | StdDev   | Median   | Allocated |
|------------- |----- |----- |---------:|---------:|---------:|---------:|----------:|
| **GemvMultiply** | **256**  | **256**  | **23.69 μs** | **0.572 μs** | **1.576 μs** | **23.30 μs** |     **488 B** |
| **GemvMultiply** | **1024** | **256**  | **90.67 μs** | **3.113 μs** | **8.415 μs** | **86.30 μs** |     **488 B** |
