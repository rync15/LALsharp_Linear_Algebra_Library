# LinalgCore Strategy Reassessment

- Date: 2026-04-10
- Scope: `src/LinalgCore/*`, `src/LinalgCore/Sparse/*`
- Goal: reassess unsafe/SIMD/intrinsics/parallel strategy and tighten allocation-GC behavior with stackalloc, ArrayPool, and workspace reuse.

## Strategy Decisions

1. Unsafe: no new unsafe blocks introduced in LinalgCore.
2. SIMD/intrinsics: keep managed SIMD in BLAS1/row-dot hot loops (Axpy/Dot/Gemv row path).
3. Parallelization: keep gated row-parallel strategy in Gemv/Gemm with worker cap tied to `Environment.ProcessorCount`.
4. Allocation policy:
   - Prefer stackalloc for small temporary vectors.
   - Prefer ArrayPool for medium/large temporary workspaces.
   - Reuse workspace spans where stage vectors are needed.

## Implemented Corrections

- `Lu.FactorAndSolve`:
  - removed per-call `new int[n]` and matrix clone path from hot solve flow.
  - now uses pooled LU buffer and stackalloc/pooled pivots.
- `EigenSolver.PowerIteration`:
  - replaced per-call `new double[n]` work vector with stackalloc/pooled workspace.
- `MatrixAnalysis`:
  - `Determinant` now uses pooled LU + stackalloc/pooled pivots.
  - `Inverse` now uses pooled LU and stackalloc/pooled work vectors for basis/column solve.
  - `Rank` now uses pooled elimination workspace instead of direct array clone allocation.
  - `PseudoInverse` rectangular branches now use pooled normal-equation buffers.

## Rule Impact (Priority)

- Rule 4 (shared structures): kept consistent matrix/vector span layout and reusable workspace style.
- Rule 5 (Span API boundary): public APIs remain span-based and do not expose pointers.
- Rule 6 (Span-first default): optimized paths remain safe managed defaults with pooled buffers.
- Unsafe-related rules 7/9/10/23/30:
  - no blanket unsafe introduction,
  - no intrinsics-only hard dependency,
  - correctness gates preserved by tests and A->D gate rerun.

## Validation Snapshot

- Rules report: `artifacts/compliance/2026-04-10-w3-rules32-review.md` -> PASSED (32/32)
- Gate A: passed
- Gate B: `artifacts/gate-b/2026-04-10-gate-b-validation.md` -> PASSED, tests 93
- Gate C: `artifacts/gate-c/2026-04-10-gate-c-validation.md` -> PASSED
- Gate D: `artifacts/w5/2026-04-10-w5-validation.md` -> PASSED, tests 93
