# Module Test Performance Report

- Generated At: 2026-04-14 13:13:12
- Source Entry: src/Core/DataStructureCompatibility.Performance.cs -> CreateModuleProfiles()
- Scope: LinalgCore, NumericalCore, OdeCore, TensorCore
- Command: dotnet test LAL.Tests/LAL.Tests.csproj -c Release --no-build --filter <core+module-token>

| Core | Module | Token | Tests | Passed | Failed | Skipped | Elapsed (ms) | Result | Notes |
|---|---|---|---:|---:|---:|---:|---:|---|---|
| LinalgCore | Axpy | Axpy | 4 | 4 | 0 | 0 | 1550.67 | pass |  |
| LinalgCore | Cholesky | Cholesky | 2 | 2 | 0 | 0 | 1497.92 | pass |  |
| LinalgCore | DenseSolver | DenseSolver | 2 | 2 | 0 | 0 | 1393.83 | pass |  |
| LinalgCore | DistanceMetrics | DistanceMetrics | 6 | 6 | 0 | 0 | 1449.59 | pass |  |
| LinalgCore | Dot | Dot | 7 | 7 | 0 | 0 | 1512.16 | pass |  |
| LinalgCore | EigenSolver | EigenSolver | 2 | 2 | 0 | 0 | 1353.53 | pass |  |
| LinalgCore | Gemm | Gemm | 4 | 4 | 0 | 0 | 1276.93 | pass |  |
| LinalgCore | Gemv | Gemv | 4 | 4 | 0 | 0 | 1279.43 | pass |  |
| LinalgCore | Lu | Lu | 20 | 20 | 0 | 0 | 1254.91 | pass |  |
| LinalgCore | MatrixAnalysis | MatrixAnalysis | 6 | 6 | 0 | 0 | 1331.16 | pass |  |
| LinalgCore | MatrixOps | MatrixOps | 5 | 5 | 0 | 0 | 1265.53 | pass |  |
| LinalgCore | Norms | Norms | 4 | 4 | 0 | 0 | 1305.66 | pass |  |
| LinalgCore | Orthogonalization | Orthogonalization | 4 | 4 | 0 | 0 | 1250.65 | pass |  |
| LinalgCore | Qr | Qr | 2 | 2 | 0 | 0 | 1303.17 | pass |  |
| LinalgCore | Schur | Schur | 2 | 2 | 0 | 0 | 1230.6 | pass |  |
| LinalgCore | Sparse.Spmv | Spmv | 4 | 4 | 0 | 0 | 1229.87 | pass |  |
| LinalgCore | Svd | Svd | 2 | 2 | 0 | 0 | 1267.95 | pass |  |
| LinalgCore | Transpose | Transpose | 4 | 4 | 0 | 0 | 1308.02 | pass |  |
| LinalgCore | VectorOps | VectorOps | 5 | 5 | 0 | 0 | 1225.6 | pass |  |
| NumericalCore | Differentiation.FiniteDifference | FiniteDifference | 3 | 3 | 0 | 0 | 1220.98 | pass |  |
| NumericalCore | Integration.BasicQuadrature | BasicQuadrature | 4 | 4 | 0 | 0 | 1232.59 | pass |  |
| NumericalCore | Integration.GaussianQuadrature | GaussianQuadrature | 3 | 3 | 0 | 0 | 1260.03 | pass |  |
| NumericalCore | Interpolation.Rbf | Rbf | 6 | 6 | 0 | 0 | 1235.65 | pass |  |
| NumericalCore | Interpolation.Spline | Spline | 3 | 3 | 0 | 0 | 1246.15 | pass |  |
| NumericalCore | Optimization.GradientDescent | GradientDescent | 3 | 3 | 0 | 0 | 1244.96 | pass |  |
| NumericalCore | Optimization.Lbfgs | Lbfgs | 3 | 3 | 0 | 0 | 1304.87 | pass |  |
| NumericalCore | Random.Rng | Rng | 4 | 4 | 0 | 0 | 1246.85 | pass |  |
| NumericalCore | RootFinding.Brent | Brent | 3 | 3 | 0 | 0 | 1266.53 | pass |  |
| NumericalCore | RootFinding.Newton | Newton | 3 | 3 | 0 | 0 | 1230.46 | pass |  |
| NumericalCore | RootFinding.Secant | Secant | 4 | 4 | 0 | 0 | 1229.44 | pass |  |
| NumericalCore | Statistics.Covariance | Covariance | 7 | 7 | 0 | 0 | 1278.32 | pass |  |
| NumericalCore | Statistics.PearsonCorrelation | PearsonCorrelation | 5 | 5 | 0 | 0 | 1231.08 | pass |  |
| NumericalCore | Statistics.Ziggurat | Ziggurat | 5 | 5 | 0 | 0 | 1212.99 | pass |  |
| OdeCore | Bdf | Bdf | 4 | 4 | 0 | 0 | 1285.3 | pass |  |
| OdeCore | DenseOutput | DenseOutput | 3 | 3 | 0 | 0 | 1263.03 | pass |  |
| OdeCore | Euler | Euler | 7 | 7 | 0 | 0 | 1230.64 | pass |  |
| OdeCore | JacobianEstimator | JacobianEstimator | 4 | 4 | 0 | 0 | 1220.54 | pass |  |
| OdeCore | Radau | Radau | 4 | 4 | 0 | 0 | 1248.12 | pass |  |
| OdeCore | Rk4 | Rk4 | 6 | 6 | 0 | 0 | 1293.54 | pass |  |
| OdeCore | Rk45 | Rk45 | 3 | 3 | 0 | 0 | 1243.06 | pass |  |
| OdeCore | StepController | StepController | 2 | 2 | 0 | 0 | 1232.74 | pass |  |
| TensorCore | Broadcasting | Broadcasting | 2 | 2 | 0 | 0 | 1258.78 | pass |  |
| TensorCore | ComplexOps | ComplexOps | 2 | 2 | 0 | 0 | 1298.77 | pass |  |
| TensorCore | ConcatStack | ConcatStack | 1 | 1 | 0 | 0 | 1219.21 | pass |  |
| TensorCore | Convolution | Convolution | 9 | 9 | 0 | 0 | 1245.62 | pass |  |
| TensorCore | Cumulative | Cumulative | 3 | 3 | 0 | 0 | 1215.87 | pass |  |
| TensorCore | Einsum | Einsum | 4 | 4 | 0 | 0 | 1247.01 | pass |  |
| TensorCore | Fft | Fft | 8 | 8 | 0 | 0 | 1358.17 | pass |  |
| TensorCore | MaskOps | MaskOps | 1 | 1 | 0 | 0 | 1320.98 | pass |  |
| TensorCore | Padding | Padding | 2 | 2 | 0 | 0 | 1212.58 | pass |  |
| TensorCore | Reductions | Reductions | 5 | 5 | 0 | 0 | 1297.05 | pass |  |
| TensorCore | ShapeOps | ShapeOps | 3 | 3 | 0 | 0 | 1261.83 | pass |  |
| TensorCore | SortSearch | SortSearch | 2 | 2 | 0 | 0 | 1247.36 | pass |  |
| TensorCore | StridedView | StridedView | 4 | 4 | 0 | 0 | 1233.29 | pass |  |
| TensorCore | TensorShape | TensorShape | 2 | 2 | 0 | 0 | 1324.21 | pass |  |
| TensorCore | UFuncArithmetic | UFuncArithmetic | 4 | 4 | 0 | 0 | 1309.88 | pass |  |
| TensorCore | UFuncTranscendental | UFuncTranscendental | 2 | 2 | 0 | 0 | 1229.73 | pass |  |

## Core Summary

| Core | Modules | Total Tests | Passed | Failed | Skipped | Total Elapsed (ms) |
|---|---:|---:|---:|---:|---:|---:|
| LinalgCore | 19 | 89 | 89 | 0 | 0 | 25287.18 |
| NumericalCore | 14 | 56 | 56 | 0 | 0 | 17440.9 |
| OdeCore | 8 | 33 | 33 | 0 | 0 | 10016.97 |
| TensorCore | 16 | 54 | 54 | 0 | 0 | 20280.34 |

## Notes

- Timings include test host startup overhead for each module-level run.
- Module matching token uses the final segment of module name (for example Sparse.Spmv -> Spmv).
- If Notes contains no-matching-tests, no test was discovered for the selected core+token filter.
