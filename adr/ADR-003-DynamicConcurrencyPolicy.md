# ADR-003: Dynamic Concurrency Scheduling Policy

- Status: Proposed
- Date: 2026-04-10

## Context
A fixed thread strategy performs poorly across compute-bound, memory-bound, and small workloads.

## Decision
Use workload-aware scheduling:
1. Compute-bound: high parallelism
2. Memory-bound: limited parallelism
3. Small-size: single-thread

## Alternatives Considered
1. Always max-thread parallelism
2. Always single-thread
3. User-configured only, no defaults

## Consequences
1. Better overall performance stability.
2. Requires benchmark calibration and tuning maintenance.

## Validation Evidence Required
1. Scheduler benchmark matrix
2. Throughput and allocation comparisons by workload class

## Review Trigger
Revisit if workload classification proves unreliable in practice.
