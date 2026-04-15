```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.26100.2314)
11th Gen Intel Core i7-11700 2.50GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 9.0.312
  [Host]     : .NET 8.0.25 (8.0.2526.11203), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
  Job-XOHDZW : .NET 8.0.25 (8.0.2526.11203), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI

Runtime=.NET 8.0  IterationCount=6  LaunchCount=1  
WarmupCount=2  

```
| Method                      | SignalSize | KernelSize | Mean      | Error     | StdDev   | Allocated |
|---------------------------- |----------- |----------- |----------:|----------:|---------:|----------:|
| **Convolve2D_AutoAdaptiveGate** | **48**         | **5**          |  **42.65 μs** |  **0.760 μs** | **0.271 μs** |         **-** |
| **Convolve2D_AutoAdaptiveGate** | **48**         | **9**          | **129.09 μs** |  **1.487 μs** | **0.386 μs** |         **-** |
| **Convolve2D_AutoAdaptiveGate** | **64**         | **5**          |  **75.69 μs** |  **3.123 μs** | **1.114 μs** |         **-** |
| **Convolve2D_AutoAdaptiveGate** | **64**         | **9**          | **230.72 μs** |  **8.283 μs** | **2.954 μs** |         **-** |
| **Convolve2D_AutoAdaptiveGate** | **128**        | **5**          | **302.17 μs** |  **7.283 μs** | **1.891 μs** |         **-** |
| **Convolve2D_AutoAdaptiveGate** | **128**        | **9**          | **924.92 μs** | **22.561 μs** | **8.046 μs** |         **-** |
