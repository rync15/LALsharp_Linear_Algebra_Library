```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.26100.2314)
11th Gen Intel Core i7-11700 2.50GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 9.0.312
  [Host]     : .NET 8.0.25 (8.0.2526.11203), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
  Job-XOHDZW : .NET 8.0.25 (8.0.2526.11203), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI

Runtime=.NET 8.0  IterationCount=6  LaunchCount=1  
WarmupCount=2  

```
| Method                      | SignalEdge | KernelEdge | Mean       | Error    | StdDev   | Allocated |
|---------------------------- |----------- |----------- |-----------:|---------:|---------:|----------:|
| **ConvolveND_AutoAdaptiveGate** | **10**         | **3**          |   **283.8 μs** |  **4.22 μs** |  **1.50 μs** |     **200 B** |
| **ConvolveND_AutoAdaptiveGate** | **10**         | **5**          | **1,233.4 μs** | **23.82 μs** |  **6.19 μs** |     **201 B** |
| **ConvolveND_AutoAdaptiveGate** | **12**         | **3**          |   **486.5 μs** | **20.57 μs** |  **5.34 μs** |     **200 B** |
| **ConvolveND_AutoAdaptiveGate** | **12**         | **5**          | **2,222.1 μs** | **64.51 μs** | **16.75 μs** |     **203 B** |
| **ConvolveND_AutoAdaptiveGate** | **16**         | **3**          | **1,191.7 μs** | **39.72 μs** | **14.16 μs** |     **201 B** |
| **ConvolveND_AutoAdaptiveGate** | **16**         | **5**          | **5,240.1 μs** | **83.34 μs** | **21.64 μs** |     **206 B** |
