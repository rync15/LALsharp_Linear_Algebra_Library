# ADR-008: W1 End-to-End Prototype Validation Route

- Status: Proposed
- Date: 2026-04-10

## Context
W1 DoD requires at least one end-to-end prototype path across key data types.

## Decision
Adopt a minimal cross-type validation route:
1. Tensor shape/stride/broadcast setup
2. Linalg operation path (`float` and `double`)
3. Complex operation path (`Complex`)
4. ODE step execution
5. Traceability and rule linkage verification

## Alternatives Considered
1. Module-only isolated smoke tests
2. Full production pipeline prototype
3. Deferred prototype to W2

## Consequences
1. Early integration confidence before heavy W2 implementation.
2. Requires baseline stubs and test harness alignment.

## Validation Evidence Required
1. Prototype execution log
2. Type coverage report
3. Traceability links to W0 function IDs

## Review Trigger
Revisit when W2 introduces major architectural changes.
