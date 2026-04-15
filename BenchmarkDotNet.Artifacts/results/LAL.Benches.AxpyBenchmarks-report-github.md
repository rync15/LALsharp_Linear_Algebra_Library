```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.26100.2314)
11th Gen Intel Core i7-11700 2.50GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 9.0.312
  [Host]     : .NET 8.0.25 (8.0.2526.11203), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
  Job-QMPETL : .NET 8.0.25 (8.0.2526.11203), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI

InvocationCount=1  UnrollFactor=1  

```
| Method     | N     | Mean     | Error     | StdDev    | Median   | Allocated |
|----------- |------ |---------:|----------:|----------:|---------:|----------:|
| **AxpyDouble** | **4096**  | **13.46 μs** |  **2.101 μs** |  **5.927 μs** | **10.85 μs** |     **400 B** |
| **AxpyDouble** | **65536** | **81.83 μs** | **18.762 μs** | **52.917 μs** | **52.60 μs** |     **400 B** |
