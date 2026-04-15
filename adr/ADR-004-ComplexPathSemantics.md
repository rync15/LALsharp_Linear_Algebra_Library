# ADR-004: Complex Number Path and Conjugation Semantics

- Status: Proposed
- Date: 2026-04-10

## Context
Linalg and ODE features must support real and complex domains with mathematically correct semantics.

## Decision
Adopt `System.Numerics.Complex` as first-class type and explicitly define:
1. Dotu vs Dotc behavior
2. Hermitian adjoint semantics
3. Real-to-complex eigenvalue output path

## Alternatives Considered
1. Real-only baseline with deferred complex support
2. Custom complex type
3. Implicit conjugation behavior

## Consequences
1. Better mathematical correctness and user expectations alignment.
2. Additional test matrix and complexity.

## Validation Evidence Required
1. Complex correctness test suite
2. Dot/Hermitian contract tests
3. Eigen real-to-complex regression cases

## Review Trigger
Revisit if runtime or type-system constraints require custom complex representation.
