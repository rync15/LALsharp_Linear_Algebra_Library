# ADR-005: Tensor Shape/Stride/Broadcast Contract

- Status: Proposed
- Date: 2026-04-10

## Context
Tensor operations require a consistent metadata contract across modules.

## Decision
Standardize on:
1. Shape + stride metadata
2. O(1) offset mapping
3. Broadcast compatibility from trailing axes
4. Metadata-first no-copy transforms where possible

## Alternatives Considered
1. Shape-only metadata with implicit contiguous assumption
2. Eager materialization for every transform
3. Module-specific incompatible contracts

## Consequences
1. Unified semantics across Tensor/Linalg/Ode layers.
2. Requires careful non-contiguous handling in kernels.

## Validation Evidence Required
1. Shape/stride correctness tests
2. Broadcast compatibility tests
3. View aliasing tests

## Review Trigger
Revisit if future kernels require alternative canonical memory order.
