# OdeCore Kernel Strategy Evaluation

- Date: 2026-04-11
- Scope: OdeCore modules
- Goal: assess and apply unsafe/SIMD/intrinsics/multithreading strategies for measurable speedups while preserving safety gates

## Module Decisions

| Module | SIMD / Intrinsics | Multithreading | Unsafe | Decision |
|---|---|---|---|---|
| Euler | Applied managed SIMD (`Vector<T>`) for float/double state update | Not enabled (single callback stage) | Not introduced | Keep safe SIMD path + scalar fallback |
| DenseOutput | Applied managed SIMD (`Vector<T>`) for float/double interpolation | Not enabled (simple linear blend) | Not introduced | Keep safe SIMD path + scalar fallback |
| Rk4 | Applied managed SIMD in stage accumulation and final weighted combine (float/double) | Not enabled for stage loops to avoid callback reentrancy assumptions | Not introduced | Keep safe SIMD path + scalar fallback |
| Rk45 | Applied managed SIMD in advance kernel and embedded error reduction (float/double) | Not enabled for full stage equations (callback-bound) | Not introduced | Keep safe SIMD path + scalar fallback |
| Bdf | Applied managed SIMD in iterative update + residual max reduction (float/double) | Not enabled (fixed-point iteration dependency) | Not introduced | Keep safe SIMD path + scalar fallback |
| Radau | Applied managed SIMD in stage update + residual max reduction + final combine (float/double) | Not enabled (fixed-point iteration dependency) | Not introduced | Keep safe SIMD path + scalar fallback |
| JacobianEstimator | Column-evaluation kernels remain scalar writes; added stackalloc fast path for small dimensions | Existing opt-in parallel path retained for large dimensions | Not introduced | Use stackalloc for small N + pooled buffers + parallel for large N |
| StepController | Scalar control logic (no vector data path) | Not applicable | Not introduced | Leave scalar implementation |

## Why Unsafe Was Not Introduced

- Current OdeCore hot loops can be accelerated with managed SIMD without pointer exposure.
- Rule-gated policy requires benchmark evidence before expanding unsafe paths.
- Existing optimized safe kernels now include SIMD + workspace reuse + optional Jacobian parallelization.

## Validation

- Command: `dotnet test LAL.sln --configuration Release --filter "FullyQualifiedName~LAL.Tests.OdeCore"`
- Result: Passed (26/26)
- Command: `dotnet test LAL.sln --configuration Release`
- Result: Passed (162/162)

## Updated Files

- src/OdeCore/Euler.cs
- src/OdeCore/DenseOutput.cs
- src/OdeCore/Rk4.cs
- src/OdeCore/Rk45.cs
- src/OdeCore/Bdf.cs
- src/OdeCore/Radau.cs
- src/OdeCore/JacobianEstimator.cs
