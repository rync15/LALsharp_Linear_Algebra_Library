# Api Design (W3)

- Project: LAL
- Date: 2026-04-10
- Goal: Map completed numerical modules to consistent C# namespaces and Span-first API boundaries.

## 1. API Principles

1. Public API uses span-based inputs/outputs where possible (`ReadOnlySpan<T>`, `Span<T>`).
2. Core algorithm modules stay in domain namespaces (`LAL.TensorCore`, `LAL.LinalgCore`, `LAL.OdeCore`, `LAL.NumericalCore.*`).
3. Error handling uses explicit argument validation (`ArgumentException`, `ArgumentOutOfRangeException`).
4. High-level convenience overloads are additive wrappers and do not remove lower-level kernel signatures.
5. Unsafe/intrinsics are gated by W4 evidence and safe fallback paths.

## 1.1 Module-Specific Design Principles

1. TensorCore: shape-first and axis-explicit contracts, metadata transforms preferred over materialization.
2. LinalgCore: BLAS-like parameter ordering (`a`, `rows`, `cols`, `x`, `y`) with deterministic argument validation.
3. OdeCore: solver APIs keep explicit `(t, dt, y, yOut, system)` signature to preserve integration semantics.
4. NumericalCore: functional-style delegates (`Func<double,double>`) for root-finding/integration/optimization, with explicit tolerance/iteration bounds.

## 2. Namespace Mapping (Completed Modules)

| Domain | Namespace | Covered Modules |
|---|---|---|
| Tensor | `LAL.TensorCore` | TensorShape, Broadcasting, StridedView, Padding, UFuncArithmetic, UFuncTranscendental, MaskOps, ShapeOps, ConcatStack, SortSearch, Reductions, Einsum, Fft, Convolution |
| Linear Algebra | `LAL.LinalgCore` | Axpy, Dot, Norms, Gemv, Gemm, Transpose, Lu, Qr, Cholesky, Svd, Schur, DenseSolver, EigenSolver, MatrixAnalysis |
| Linear Algebra Sparse | `LAL.LinalgCore.Sparse` | Spmv |
| ODE | `LAL.OdeCore` | Euler, Rk4, Rk45, JacobianEstimator, Bdf, Radau, StepController, DenseOutput |
| Numerical Root Finding | `LAL.NumericalCore.RootFinding` | Newton, Secant, Brent |
| Numerical Integration | `LAL.NumericalCore.Integration` | BasicQuadrature, GaussianQuadrature |
| Numerical Differentiation | `LAL.NumericalCore.Differentiation` | FiniteDifference |
| Numerical Optimization | `LAL.NumericalCore.Optimization` | GradientDescent, Lbfgs |
| Numerical Interpolation | `LAL.NumericalCore.Interpolation` | Spline, Rbf |
| Numerical Random | `LAL.NumericalCore.Random` | Rng |
| Numerical Statistics | `LAL.NumericalCore.Statistics` | Covariance, PearsonCorrelation, Ziggurat |

## 2.1 ApiSurface Quick Summary (Representative APIs)

- Purpose: 先用每個 Core 的代表性 API 快速判斷審閱重點，再進入完整方法群對照。

| Core | 代表性 API（ApiSurface） | 完整方法群位置 |
|---|---|---|
| LinalgCore | Axpy, Gemm, Spmv, Determinant, Inverse, QrDecomposeThin, SingularValues | [LinalgCore 小節](ApiDesign.md#linalgcore) |
| NumericalCore | Newton, Brent, GaussianIntegrate, GradientDescent, LbfgsSolveScalar, RbfEvaluateGaussian, ZigguratNextNormal | [NumericalCore 小節](ApiDesign.md#numericalcore) |
| OdeCore | EulerStep, Rk45Step, StepBackwardEuler, StepOneStage, EstimateForwardDifference, ProposeStep | [OdeCore 小節](ApiDesign.md#odecore) |
| TensorCore | RowMajorStrides, BroadcastShape, Sum, EvaluateMatMul, ForwardND, SearchSorted, GetConvolutionParallelSettings | [TensorCore 小節](ApiDesign.md#tensorcore) |

## 2.2 ApiSurface Module Mapping (Module -> Method Groups)

- Source of truth snapshot: [artifacts/api/2026-04-13-apisurface-module-mapping.md](artifacts/api/2026-04-13-apisurface-module-mapping.md)

### LinalgCore

| Module | ApiSurface 方法群 |
|---|---|
| Axpy | Axpy |
| Cholesky | CholeskyDecomposeLower |
| DenseSolver | DenseSolve |
| DistanceMetrics | PairwiseCosine, PairwiseEuclidean, PairwiseMahalanobis |
| Dot | Dot |
| EigenSolver | PowerIteration |
| Gemm | Gemm |
| Gemv | Gemv |
| Lu | LuDecomposeInPlace, LuFactorAndSolve, LuSolve |
| MatrixAnalysis | Determinant, DominantEigenvalue, Eigenvalues2x2, Inverse, PseudoInverse, PseudoInverseSquare, Rank, Trace |
| MatrixOps | MatrixAdd, MatrixDot, MatrixMultiply, MatrixSubtract |
| Norms | NormInfinity, NormL1, NormL2, NormL2Squared |
| Orthogonalization | GramSchmidtOrthonormalize, OrthogonalProjection, ProjectionCoefficient |
| Qr | QrDecomposeThin |
| Schur | RealSchur2x2 |
| Spmv | Spmv |
| Svd | SingularValues |
| Transpose | ConjugateTransposeMatrix, TransposeMatrix |
| VectorOps | VectorAdd, VectorDot, VectorInnerProduct, VectorOuterProduct, VectorSubtract |

### NumericalCore

| Module | ApiSurface 方法群 |
|---|---|
| BasicQuadrature | Trapezoidal |
| Brent | Brent |
| Covariance | Correlation, Covariance |
| FiniteDifference | CentralDiff |
| GaussianQuadrature | GaussianIntegrate |
| GradientDescent | GradientDescent |
| Lbfgs | LbfgsSolveScalar |
| Newton | Newton |
| PearsonCorrelation | PearsonCompute |
| Rbf | RbfComputeGaussianWeights, RbfEvaluateGaussian |
| Rng | RngNextComplexNormal, RngNextComplexUniform, RngNextNormal, RngNextNormalFloat, RngNextUInt, RngNextUniform, RngNextUniformFloat, RngNormalizeSeed |
| Secant | Secant |
| Spline | ComputeNaturalSecondDerivatives, EvaluateNaturalCubic |
| Ziggurat | ZigguratNextComplexNormal, ZigguratNextComplexU01, ZigguratNextComplexUniform, ZigguratNextNormal, ZigguratNextNormalFloat, ZigguratNextU01, ZigguratNextU01Float, ZigguratNextUniform |

### OdeCore

| Module | ApiSurface 方法群 |
|---|---|
| Bdf | StepBackwardEuler |
| DenseOutput | InterpolateLinear |
| Euler | EulerStep |
| JacobianEstimator | EstimateForwardDifference |
| Radau | StepOneStage |
| Rk4 | Rk4Step |
| Rk45 | Rk45Step |
| StepController | ProposeStep |

### TensorCore

| Module | ApiSurface 方法群 |
|---|---|
| Broadcasting | BroadcastShape |
| ComplexOps | Conjugate, Im, Imaginary, Re, Real |
| ConcatStack | Concatenate, Stack |
| Convolution | GetConvolutionParallelSettings, ResetConvolutionParallelSettings, SetConvolutionParallelSettings |
| Cumulative | CumProd, CumSum |
| Einsum | Compute, Evaluate, EvaluateMatMul, Kronecker, TensorContractionGemm |
| Fft | Forward, Forward2D, ForwardND, Inverse, Inverse2D, InverseND, Rfft, RfftND |
| MaskOps | Equal, GreaterThan, LessThan, Where |
| Padding | EdgePad1D, PeriodicPad1D, ZeroPad1D |
| Reductions | Sum |
| ShapeOps | ExpandDims, Flatten, Reshape, ReshapeCopy, Squeeze, SwapAxes, SwapAxes2D, Transpose2D, TransposeShape |
| SortSearch | SearchSorted |
| StridedView | ComputeSlicedShape, ExpandDimsView, PermuteShape, SqueezeView |
| TensorShape | Offset, RowMajorStrides |
| UFuncArithmetic | Add, Divide, Multiply, Power, Subtract |
| UFuncTranscendental | Exp |

## 3. API Layer Separation

- Kernel layer: algorithm implementations in `src/**` with validated primitives.
- API surface layer: façade-style wrappers in [ApiSurface.cs](ApiSurface.cs).
- Usage layer: end-user examples in [UsageSamples.md](UsageSamples.md).

## 4. W3 Confirmation Summary

- Completed source modules: 49
- Completed test modules: 49
- Required W3 deliverables present: `ApiDesign.md`, `ApiSurface.cs`, `UsageSamples.md`
- Namespace mapping coverage: all completed source namespaces mapped.
