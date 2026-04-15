# ADR-007: Unsafe Entry Criteria and Perf Gate

- Status: Proposed
- Date: 2026-04-10

## Context
Unsafe optimizations can improve speed but increase risk and maintenance cost.

## Decision
Unsafe/SIMD is allowed only when all are satisfied:
1. Benchmark proves bottleneck and measurable gain
2. Correctness parity tests pass
3. Allocation profile is acceptable
4. Safe fallback remains available

## Alternatives Considered
1. Broad unsafe policy
2. No unsafe policy
3. Team discretion without documented gate

## Consequences
1. Controlled optimization with auditable evidence.
2. Extra process overhead before merging hot-path changes.

## Validation Evidence Required
1. Before/after benchmark reports
2. Regression correctness evidence
3. Fallback route tests

## Review Trigger
Revisit if gate cost outweighs practical engineering value.
