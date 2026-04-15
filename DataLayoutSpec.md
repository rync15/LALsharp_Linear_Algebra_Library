# Data Layout Specification (W1)

- Project: Span-First C# Numerical and Linear Algebra Library
- Version: 0.1-draft
- Date: 2026-04-10
- Status: Draft for architecture freeze

## 1. Scope

This file defines shared data layout contracts for:

1. TensorCore
2. LinalgCore
3. OdeCore
4. NumericalCore

## 2. Core Data Objects

### 2.1 `NDBuffer<T>`

Purpose:

1. Own contiguous storage for N-dimensional data.
2. Provide span views for safe kernels.
3. Support optional unmanaged ownership backend.

Required fields (conceptual):

1. Element type `T`
2. Logical shape
3. Strides
4. Total length
5. Ownership metadata

### 2.2 `TensorShape`

Purpose:

1. Canonical shape representation.
2. Stride derivation.
3. Offset mapping from index tuples.

Rules:

1. Shape rank must be >= 1 for tensors and vectors.
2. Stride mapping must support O(1) offset calculation.
3. Out-of-range index access must be rejected.

## 3. Stride and Offset Rules

Row-major default:

1. Last axis stride is 1.
2. Previous axis stride = next axis stride * next axis length.

Offset formula:

- For indices `i0..ik` and strides `s0..sk`:
- `offset = Σ (id * sd)`

Constraints:

1. Shape and stride vectors must have equal length.
2. Any reshape without copy must preserve total element count.
3. Non-contiguous views are valid for read/write via strided access rules.

## 4. Views and Slicing

Supported view operations:

1. Slice
2. Transpose / swap axes
3. ExpandDims / Squeeze
4. Reshape (no-copy when compatible)

View rules:

1. Views must preserve ownership reference to backing storage.
2. View creation must avoid allocation when metadata-only transform suffices.
3. Mutation semantics must be explicit for shared views.

## 5. Broadcasting Engine

Broadcast compatibility rules:

1. Compare shapes from trailing axis to leading axis.
2. Axes are compatible if equal or one side is 1.
3. Output axis size is max of compatible pair.

Implementation requirement:

1. Broadcasting should be represented by metadata/stride behavior first.
2. Physical expansion must be avoided unless kernel requires contiguous input.

## 6. Padding Semantics

Supported modes:

1. Zero
2. Edge
3. Periodic

Rules:

1. Padding policy must define read semantics for out-of-domain indices.
2. Zero-copy padded views are preferred when feasible.
3. Materialized padded buffers require explicit lifecycle ownership.

## 7. Dense Matrix and Vector Layout

Conventions:

1. Vector: rank-1 contiguous by default.
2. Matrix: rank-2 row-major by default.
3. Optimized APIs should exist for 1D and 2D fast paths.

Compatibility:

1. Dense routines must consume shared `NDBuffer<T>` shape/stride contracts.
2. High-rank tensors can be lowered to 2D kernels through stride-aware transforms.

## 8. Sparse Baseline Layout

### 8.1 CSR (Compressed Sparse Row)

Required arrays:

1. `values`
2. `colIndex`
3. `rowPtr`

Constraints:

1. `rowPtr.Length == rows + 1`
2. `values.Length == colIndex.Length`
3. `rowPtr` is monotonic non-decreasing

### 8.2 CSC (Compressed Sparse Column)

Required arrays:

1. `values`
2. `rowIndex`
3. `colPtr`

Constraints mirror CSR with row/column role swapped.

### 8.3 Sparse baseline operations

1. SpMV (CSR primary path)
2. Basic format validation
3. Optional CSR<->CSC conversion utility

## 9. Type Strategy and Compatibility

Primary data types:

1. `float`
2. `double`
3. `Complex`

Requirements:

1. Data layout contracts are type-agnostic.
2. Complex path follows interleaved memory semantics through runtime representation.
3. Type conversion behavior must be explicit at API boundary.

## 10. Validation Requirements

Required tests:

1. Shape/stride correctness tests
2. Broadcast compatibility tests
3. View aliasing tests
4. Padding mode behavior tests
5. Sparse structure invariants tests
6. Dense/sparse cross-check tests for small reference cases

## 11. Gate A/B Readiness Checklist

- [ ] O(1) offset mapping rule is documented and testable.
- [ ] View/broadcast/padding semantics are explicit.
- [ ] 1D/2D optimized paths and N-D fallback contracts are defined.
- [ ] CSR/CSC baseline format and invariants are defined.
- [ ] Type strategy for float/double/Complex is explicit.

## 12. Compatibility Layer Reference

Reference implementation for cross-core shape/stride/data compatibility:

1. `src/Core/DataStructureCompatibility.cs` (`DataStructureCompatibility`)
2. `src/Core/DataStructureCompatibility.cs` (`NDBuffer<T>`)

Covered interoperability:

1. Shared shape validation and element-count checks
2. Row-major stride derivation and offset mapping (via TensorShape contract)
3. Unified vector/matrix compatibility checks for Tensor/Linalg/Ode/Numerical pipelines
