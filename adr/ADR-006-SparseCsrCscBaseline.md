# ADR-006: Sparse Baseline with CSR/CSC

- Status: Proposed
- Date: 2026-04-10

## Context
Sparse workloads need practical baseline formats with predictable operations.

## Decision
Use CSR/CSC as baseline sparse representations and prioritize CSR SpMV for initial path.

## Alternatives Considered
1. COO-only baseline
2. Custom hybrid sparse format
3. Dense-only fallback in early stages

## Consequences
1. Fast baseline for common sparse matrix-vector workloads.
2. Additional conversion and validation logic needed.

## Validation Evidence Required
1. CSR/CSC invariants tests
2. SpMV correctness/performance tests
3. Dense vs sparse cross-check cases

## Review Trigger
Revisit if workload profile strongly favors alternative sparse formats.
