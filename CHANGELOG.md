# Changelog

All notable changes to this project are documented in this file.

## [0.1.0] - 2026-04-10

### Added

- Full function traceability baseline for TensorCore/LinalgCore/OdeCore/NumericalCore.
- W3 API mapping deliverables: `ApiDesign.md`, `ApiSurface.cs`, `UsageSamples.md`.
- W4 governance and verification scripts for performance and gate checks.
- Cross-module managed SIMD and gated parallel strategy updates.
- Traceability dashboard validator: `scripts/verify-traceability-dashboard.ps1` with markdown evidence output under `artifacts/traceability/`.
- W5 gate validator: `scripts/verify-w5.ps1` with machine-generated report at `artifacts/w5/2026-04-10-w5-validation.md` and summary `ValidationReport.md`.
- Gate B validator: `scripts/verify-gate-b.ps1` with report `artifacts/gate-b/2026-04-10-gate-b-validation.md`.
- Property-style invariant tests for linalg kernels: `tests/LinalgCore/InvariantPropertyTests.cs`.
- LinalgCore feature-completion APIs: `Orthogonalization`, `DistanceMetrics`, `VectorOps`, `MatrixOps`, and expanded `MatrixAnalysis` (determinant/inverse/pseudoinverse/eigenvalue entrypoint).
- Sparse CSC matrix-vector multiplication support via `Spmv.CscMultiply`.

### Changed

- Updated `PerfGate.md` and `UnsafeRationale.md` with module-level rollout policy.
- Refined `plan.md` section 5.4 with module-specific parallel strategy notes.
- Updated CI workflow `.github/workflows/ci.yml` to enforce W3 + Rules-32 and traceability dashboard gates.
- Aligned CI SDK bootstrap to .NET 9 to match repository `global.json`.
- Extended CI governance artifact upload to include W5 and validation summary reports.
- Reassessed all modules for allocation pressure and updated hot paths to prioritize stackalloc, ArrayPool, and workspace reuse.

### Fixed

- Resolved stale gate-c diagnostics and aligned script validation output.
- Added parallel-path consistency tests for Jacobian and covariance kernels.
- Reduced transient allocations in TensorCore Fft, LinalgCore Gemv/Gemm, OdeCore Rk4/Rk45/JacobianEstimator, and NumericalCore Covariance.
- Closed missing LinalgCore capability gaps with implementation and tests for orthogonalization, distance matrices, matrix/vector operators, and CSC sparse multiply.
- Expanded eigenvalue coverage with `MatrixAnalysis.Eigenvalues2x2` (real/complex) and brought full regression to 93 passing tests.
