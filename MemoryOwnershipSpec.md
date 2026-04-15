# Memory Ownership Specification (W1)

- Project: Span-First C# Numerical and Linear Algebra Library
- Version: 0.1-draft
- Date: 2026-04-10
- Status: Draft for architecture freeze

## 1. Goals

1. Keep public APIs safe and pointer-free.
2. Centralize unmanaged memory handling in one ownership layer.
3. Minimize allocations through pooling and reusable work buffers.
4. Define strict unsafe entry boundaries and fallback policy.

## 2. Ownership Model

### 2.1 Ownership categories

1. Owned buffer: library controls lifecycle.
2. Borrowed view: references external or shared storage without owning disposal.
3. Pooled buffer: leased from pool, must be returned.
4. Unmanaged owned buffer: allocated in ownership layer only.

### 2.2 Ownership invariants

1. Every buffer must have exactly one primary lifecycle authority.
2. Borrowed views must not outlive underlying owner.
3. Disposal must be idempotent and safe.
4. Double-free and use-after-free are blocking defects.

## 3. Managed Memory Policy

1. Use spans at API and safe kernel boundaries.
2. Use `ArrayPool<T>` for temporary large buffers.
3. Use `stackalloc` only for short-lived small buffers.
4. Avoid hidden allocations in hot loops.

## 4. Unmanaged Memory Policy

1. Unmanaged allocation is restricted to ownership layer.
2. Allocation/deallocation methods are centralized.
3. Unsafe blocks outside ownership/hot-kernel policy are not allowed.
4. Any unmanaged path must have a managed fallback path.

## 5. Buffer Lifecycle State Machine

States:

1. Allocated
2. Leased
3. InUse
4. Returned
5. Disposed

Rules:

1. `Allocated -> Leased -> InUse -> Returned` for pooled buffers.
2. `Allocated -> InUse -> Disposed` for non-pooled owned buffers.
3. Access in `Returned` or `Disposed` is invalid.

## 6. API Boundary Rules

1. Public APIs must not expose raw pointers.
2. Ownership transfer, if any, must be explicit in API contract.
3. Returned spans are valid only within documented lifetime.
4. Buffer aliasing behavior must be documented for views.

## 7. Unsafe Entry Gate

Unsafe code may be introduced only if all conditions hold:

1. A benchmark demonstrates measurable bottleneck.
2. Regression tests prove numerical correctness parity.
3. Fallback safe path exists and is maintained.
4. Risk note and rationale are recorded in ADR.

Required evidence package:

1. Benchmark before/after report
2. Correctness test results
3. Allocation profile
4. Rollback/fallback note

## 8. Pooling Strategy

1. Define reusable work buffers per module (Tensor/Linalg/Ode/Numerical).
2. Prefer thread-local or scoped pooling for short computations.
3. Track pool misses and churn in perf reports.
4. Ensure clear return ownership in exceptions/failures.

## 9. Error Handling and Safety

1. Disposal errors must not corrupt global state.
2. Exception paths must return pooled resources.
3. Debug checks should detect invalid state transitions.
4. High-severity memory errors block release.

## 10. Validation Requirements

Required tests:

1. Ownership lifecycle tests (normal and exceptional paths)
2. Pool lease/return correctness tests
3. No-leak stress tests
4. Unsafe/fallback parity tests
5. Multi-thread ownership race tests for scheduler-sensitive paths

## 11. W1 Acceptance Checklist

- [ ] Ownership categories and invariants are defined.
- [ ] Unmanaged memory boundaries are centralized and explicit.
- [ ] Unsafe gate requirements and evidence package are documented.
- [ ] Pooling and lifecycle states are testable.
- [ ] API boundary safety rules align with Span-first policy.
