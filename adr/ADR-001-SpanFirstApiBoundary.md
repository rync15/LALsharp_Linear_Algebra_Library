# ADR-001: Span-first Public API Boundary

- Status: Proposed
- Date: 2026-04-10

## Context
Public APIs must stay safe, efficient, and allocation-aware while supporting numerical workloads.

## Decision
Expose `ReadOnlySpan<T>` for inputs and `Span<T>` for outputs by default; do not expose raw pointers in public signatures.

## Alternatives Considered
1. Array-only API
2. Pointer-based API
3. Generic enumerable API

## Consequences
1. Better safety and lower overhead than enumerable abstractions.
2. Requires careful lifetime documentation.

## Validation Evidence Required
1. API signature audit report
2. Analyzer checks for pointer leakage

## Review Trigger
Revisit if major runtime changes make alternative API boundaries clearly superior.
