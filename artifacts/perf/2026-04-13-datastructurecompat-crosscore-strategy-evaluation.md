# Cross-Core Strategy Evaluation via DataStructureCompatibility

Date: 2026-04-13

## Scope
- Entry: DataStructureCompatibility + NDBuffer<T>
- Cores: LinalgCore, NumericalCore, OdeCore, TensorCore
- Target strategies: unsafe, SIMD, CPU intrinsics, multithreading

Legend:
- S = Scalar
- V = SIMD
- I = Intrinsics
- U = Unsafe
- P = Parallel

## Runtime strategy controls added
- DataStructurePerformanceSettings (global)
- Get/Set/Reset APIs on DataStructureCompatibility
- Runtime hardware capability probe

## Module assessment (all modules)

### LinalgCore (19)
| Module | Current | Recommended |
|---|---|---|
| Axpy | S+V+I+U+P | S+V+I+U+P |
| Cholesky | S | S+V |
| DenseSolver | S | S+V+P |
| DistanceMetrics | S | S+V+P |
| Dot | S+V+I+U+P | S+V+I+U+P |
| EigenSolver | S+V | S+V+P |
| Gemm | S+V+P | S+V+I+U+P |
| Gemv | S+V+P | S+V+I+U+P |
| Lu | S | S+V+P |
| MatrixAnalysis | S+V | S+V+P |
| MatrixOps | S+V | S+V+I+U+P |
| Norms | S+V | S+V+I+U+P |
| Orthogonalization | S | S+V+P |
| Qr | S | S+V+P |
| Schur | S | S+V |
| Sparse.Spmv | S | S+P |
| Svd | S | S+V+P |
| Transpose | S+V | S+V+P |
| VectorOps | S+V | S+V+I+U+P |

### NumericalCore (14)
| Module | Current | Recommended |
|---|---|---|
| Differentiation.FiniteDifference | S | S+V |
| Integration.BasicQuadrature | S | S+V+P |
| Integration.GaussianQuadrature | S | S+V+P |
| Interpolation.Rbf | S+P | S+V+P |
| Interpolation.Spline | S | S+V |
| Optimization.GradientDescent | S | S+V+P |
| Optimization.Lbfgs | S | S+V+P |
| Random.Rng | S | S+V |
| RootFinding.Brent | S | S |
| RootFinding.Newton | S | S+V |
| RootFinding.Secant | S | S |
| Statistics.Covariance | S+V+P | S+V+I+U+P |
| Statistics.PearsonCorrelation | S+V+P | S+V+I+U+P |
| Statistics.Ziggurat | S | S+V |

### OdeCore (8)
| Module | Current | Recommended |
|---|---|---|
| Bdf | S+V | S+V+P |
| DenseOutput | S+V | S+V+I+U+P |
| Euler | S+V+I+U+P | S+V+I+U+P |
| JacobianEstimator | S+P | S+V+P |
| Radau | S+V | S+V+P |
| Rk4 | S+V | S+V+P |
| Rk45 | S+V | S+V+P |
| StepController | S | S |

### TensorCore (16)
| Module | Current | Recommended |
|---|---|---|
| Broadcasting | S | S+V+P |
| ComplexOps | S | S+V |
| ConcatStack | S | S+V+P |
| Convolution | S+V+P | S+V+I+U+P |
| Cumulative | S | S+V+P |
| Einsum | S+V+P | S+V+I+U+P |
| Fft | S+V | S+V+P |
| MaskOps | S | S+V+P |
| Padding | S | S+V+P |
| Reductions | S+V | S+V+I+U+P |
| ShapeOps | S | S+V+P |
| SortSearch | S+V | S+V+P |
| StridedView | S | S |
| TensorShape | S | S |
| UFuncArithmetic | S+V+I+U+P | S+V+I+U+P |
| UFuncTranscendental | S | S+V |

## Implemented in this change set
- Added shared low-level kernels:
  - unsafe + AVX intrinsics + SIMD fallback + scalar fallback
  - AXPY, DOT, elementwise Add/Subtract/Multiply/Divide, scaled add (Euler integration)
- Added DataStructureCompatibility performance entry APIs:
  - LinalgAxpy / LinalgDot
  - TensorAdd / TensorSubtract / TensorMultiply / TensorDivide
  - OdeEulerStep
  - NumericalCorrelation
- Added global strategy controls and hardware capability detection.
- Updated existing hot modules to reuse shared kernels:
  - LinalgCore: Axpy, Dot
  - TensorCore: UFuncArithmetic (real float/double paths)
  - OdeCore: Euler (real float/double integrate paths)

## Notes
- Module profile table is surfaced by runtime API: GetModulePerformanceProfiles().
- The table intentionally covers all 57 modules and can be used as the roadmap for follow-up optimization passes.
