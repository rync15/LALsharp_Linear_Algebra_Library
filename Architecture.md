# Architecture Specification (W1)

- Project: Span-First C# Numerical and Linear Algebra Library
- Version: 0.1-draft
- Date: 2026-04-10
- Status: Draft for Gate A/B preparation

## 1. Architecture Goals

1. Public APIs remain safe and ergonomic for C# users.
2. Internal kernels are Span-first by default.
3. Unsafe and SIMD are enabled only by perf evidence.
4. Shared data model supports Tensor, Linalg, and ODE workloads.
5. Real (`float`, `double`) and complex (`System.Numerics.Complex`) paths are first-class.

## 2. Layered Architecture

### 2.1 Public API Layer

Responsibilities:

1. Validate user inputs (shape, axis, tolerance, options).
2. Expose only safe abstractions.
3. Route requests to safe kernels or hot kernels through policy gates.

Constraints:

1. No raw pointer in public signatures.
2. Favor `ReadOnlySpan<T>` for inputs and `Span<T>` for outputs.

### 2.2 Safe Kernel Layer (Default)

Responsibilities:

1. Core algorithm implementations using safe C# and spans.
2. Deterministic behavior and full testability.
3. Shared logic for all data types and modules.

Constraints:

1. Unsafe code disallowed by default.
2. Allocation minimized with stackalloc and pooling where applicable.

### 2.3 Hot Kernel Layer (Performance-Gated)

Responsibilities:

1. SIMD, intrinsics, and unsafe optimized paths.
2. Specialized implementations for bottleneck loops.

Entry conditions:

1. Benchmark evidence exists and is linked.
2. Correctness parity with safe kernel is proven by regression tests.
3. Fallback path is implemented and documented.

### 2.4 Scheduling & Memory Layer

Responsibilities:

1. Dynamic parallel strategy by workload type.
2. Buffer pooling and unmanaged ownership management.
3. Threading caps based on `Environment.ProcessorCount`.

## 3. Workload-Aware Concurrency Policy

| Workload type | Typical ops | Strategy | Thread policy |
|---|---|---|---|
| Compute-bound | Gemm, SVD, FFT | block/tile partitioning | near CPU upper bound |
| Memory-bound | Axpy, simple reductions | controlled partitioning | below CPU upper bound |
| Small-size | small vectors/matrices | single-thread | avoid context switch overhead |

Decision rules:

1. Small workloads stay single-threaded.
2. Memory-bound kernels reduce thread count to avoid bandwidth saturation.
3. Compute-bound kernels use cache-aware chunking and dynamic partitioning.

## 4. Complex Number Path

Supported model:

1. `float`
2. `double`
3. `System.Numerics.Complex`

Requirements:

1. Dot operations distinguish unconjugated and conjugated variants.
2. Hermitian adjoint is defined and tested for matrix operations.
3. Eigen solver supports real matrix input producing complex eigenvalues.

## 5. Module Mapping

1. TensorCore: shape/stride, broadcasting, views, ufuncs, reductions, FFT baseline.
2. LinalgCore: BLAS-like operations, decompositions, solvers, sparse baseline.
3. OdeCore: explicit/implicit solver architecture and Jacobian-based pathways.
4. NumericalCore: root finding, integration, optimization, interpolation, RNG/statistics.

## 6. Error and Validation Policy

1. Input validation errors use argument exceptions with clear messages.
2. Numerical non-finite values are handled per operation contract.
3. Shape and axis checks are mandatory at API boundary.
4. Concurrency and memory policy violations are considered blocking defects.

## 7. Observability and Performance Evidence

1. Every hot path has benchmark coverage.
2. Benchmark reports include throughput, latency, and allocation stats.
3. Perf regressions are tracked by baseline snapshots.
4. Unsafe optimization requires before/after comparison artifacts.

## 8. ADR Hooks

Architecture decisions are tracked in `ADRIndex.md` and per-ADR files.

Planned ADR subjects:

1. Span-first API boundary policy
2. Safe vs hot kernel split
3. Complex path design
4. Dynamic scheduler policy
5. Sparse baseline (CSR/CSC) model
6. Padding and broadcast semantics
7. Unsafe entry criteria
8. Prototype validation route

## 9. End-to-End Prototype Route (W1 DoD)

Minimal route (cross-type):

1. Build tensor shape + broadcast view.
2. Run Gemv/Gemm for `float` and `double`.
3. Run complex Dot/Hermitian operation for `Complex`.
4. Run one explicit ODE step.
5. Verify output correctness and traceability links.

## 10. W1 Acceptance Checklist

- [ ] Layer responsibilities are explicit and non-overlapping.
- [ ] Concurrency policy documented by workload class.
- [ ] Complex path requirements captured and testable.
- [ ] Module mapping aligned with plan W2 tracks.
- [ ] ADR hooks and prototype route are defined.
