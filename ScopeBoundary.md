# Scope Boundary

- Project: Span-First C# Numerical and Linear Algebra Library
- Version: 0.1-draft
- Date: 2026-04-10
- Source: plan.md, 開發計畫與規格書.md

## 1. In Scope (W0/W1 Baseline)

### 1.1 Core modules

1. TensorCore (shape/stride, broadcasting, slicing, ufuncs, reductions, einsum, FFT baseline)
2. LinalgCore (BLAS-like ops, matrix factorizations, dense solvers, sparse baseline CSR/CSC)
3. OdeCore (explicit and implicit solver architecture baseline)
4. NumericalCore (root finding, integration, optimization, interpolation, RNG/statistics baseline)

### 1.2 Architecture and governance

1. Public API boundary policy (`Span<T>` / `ReadOnlySpan<T>` first)
2. Safe/Hot kernel layering and unsafe gate policy
3. Traceability baseline (function -> source -> implementation -> test -> performance)
4. Rule compliance baseline (32-rule auditable checklist)

### 1.3 Platform

1. .NET 8 (LTS)
2. x64 Windows and Linux

## 2. Out of Scope (Current Phase)

1. Standalone topology-theory module unrelated to numerical computation
2. Blanket unsafe rewrite without benchmark evidence
3. Visualization/UI tooling not required for numerical kernel development
4. Non-.NET runtime targets in current baseline phase
5. Feature-complete SciPy-equivalent ecosystem in W0/W1 stage

## 3. Deferred Scope (Post W1 / W2+)

1. Advanced sparse direct solvers beyond baseline SpMV and storage formats
2. Broad statistical hypothesis-testing suite
3. Autodiff and symbolic differentiation integration
4. Full control-theory specialized solvers (e.g., Riccati family)
5. Extended transform families (DCT/DWT) beyond FFT baseline

## 4. Boundary Rules

1. Any new feature request must first be added to `TraceabilityMatrix.md` with a new Function ID.
2. Any rule-impacting change must include an update in `RuleComplianceChecklist.md`.
3. Any unsafe code proposal must include a benchmark artifact and fallback strategy.
4. Any module-level architectural change must be recorded in ADR documents.

## 5. Change Control Workflow

1. Proposal intake: open scope-change item with rationale and expected value.
2. Classification: mark as In-Scope / Deferred / Out-of-Scope.
3. Traceability update: add or update function IDs and source mapping.
4. Rule impact check: identify affected rules (1-32) and required evidence.
5. Approval gate: architecture owner + quality owner sign-off.

## 6. Gate A Exit Criteria for Scope

- [ ] In-scope list aligns with plan.md W0/W1 definitions.
- [ ] Out-of-scope and deferred lists are explicit and reviewable.
- [ ] Change control workflow is documented and usable by team.
