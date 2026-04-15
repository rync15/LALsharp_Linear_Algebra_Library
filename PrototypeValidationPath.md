# Prototype Validation Path (W1 DoD)

- Goal: define at least one end-to-end route across architecture layers and types.
- Status: executable specification for implementation phase.

## 1. Route Definition

1. Construct tensor metadata (`shape`, `stride`) and a broadcast-compatible view.
2. Execute one safe-kernel linear algebra operation for `float`.
3. Execute the same operation for `double`.
4. Execute complex path operation (`Dotc` or Hermitian adjoint) for `Complex`.
5. Run one explicit ODE integration step using shared buffer contracts.
6. Verify outputs against known references and tolerance thresholds.

## 2. Layer Coverage

| Step | Public API | Safe Kernel | Hot Kernel | Scheduling/Memory |
|---|---|---|---|---|
| Tensor metadata and broadcast | Yes | Yes | Optional | Yes |
| Float/double linalg op | Yes | Yes | Optional | Yes |
| Complex operation | Yes | Yes | Optional | Yes |
| ODE step | Yes | Yes | Optional | Yes |

## 3. Acceptance Criteria

1. Route passes for `float`, `double`, and `Complex` where applicable.
2. No public API leaks raw pointers.
3. All route steps map to entries in `TraceabilityMatrix.md`.
4. Rule checks for safety/perf governance remain satisfiable.

## 4. Evidence Package

1. Test log links
2. Numerical error report (tolerance + actual error)
3. Optional perf snapshot for hot-path candidate steps
4. Updated matrix and checklist references
