# ADR-002: Safe Kernel vs Hot Kernel Split

- Status: Proposed
- Date: 2026-04-10

## Context
Performance-critical kernels may require unsafe/SIMD, but maintainability and correctness require a safe baseline.

## Decision
Adopt two internal paths:
1. Safe Kernel: default, fully testable, span-based
2. Hot Kernel: optional, performance-gated, unsafe/SIMD allowed

## Alternatives Considered
1. Safe-only implementation
2. Hot-only implementation
3. Mixed implementation without explicit boundaries

## Consequences
1. Improves maintainability with explicit optimization boundaries.
2. Adds complexity in dual-path testing and regression maintenance.

## Validation Evidence Required
1. Correctness parity tests
2. Benchmarks proving hot-path gains
3. Fallback path coverage

## Review Trigger
Revisit if hot-path gains are consistently negligible.
