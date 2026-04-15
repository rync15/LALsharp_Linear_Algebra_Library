# Performance Node Audit

- Matrix: TraceabilityMatrix.md
- Generated: 2026-04-13 13:52:50
- Total performance nodes: 39
- Missing performance nodes: 0

| Function ID | Function Name | Performance Node | Exists |
|---|---|---|---|
| F002 | Broadcasting Engine | benches/TensorCore/BroadcastBenchmarks.cs | Yes |
| F004 | Padding (Zero/Edge/Periodic) | benches/TensorCore/PaddingBenchmarks.cs | Yes |
| F005 | UFunc Arithmetic | benches/TensorCore/UFuncArithmeticBenchmarks.cs | Yes |
| F006 | UFunc Transcendental | benches/TensorCore/UFuncTranscendentalBenchmarks.cs | Yes |
| F009 | Concatenate and Stack | benches/TensorCore/ConcatStackBenchmarks.cs | Yes |
| F010 | Argsort/Lexsort/Search | benches/TensorCore/SortSearchBenchmarks.cs | Yes |
| F011 | Reductions and Aggregations | benches/TensorCore/ReductionsBenchmarks.cs | Yes |
| F012 | Einsum and Contraction | benches/TensorCore/EinsumBenchmarks.cs | Yes |
| F013 | FFT/IFFT/RFFT | benches/TensorCore/FftBenchmarks.cs | Yes |
| F014 | Convolution | benches/TensorCore/ConvolutionBenchmarks.cs | Yes |
| F100 | Axpy | benches/LinalgCore/AxpyBenchmarks.cs | Yes |
| F101 | Dot/Dotu/Dotc | benches/LinalgCore/DotBenchmarks.cs | Yes |
| F102 | Vector Norms | benches/LinalgCore/NormsBenchmarks.cs | Yes |
| F103 | Gemv | benches/LinalgCore/GemvBenchmarks.cs | Yes |
| F104 | Gemm | benches/LinalgCore/GemmBenchmarks.cs | Yes |
| F106 | LU Decomposition | benches/LinalgCore/LuBenchmarks.cs | Yes |
| F107 | QR Decomposition | benches/LinalgCore/QrBenchmarks.cs | Yes |
| F108 | Cholesky | benches/LinalgCore/CholeskyBenchmarks.cs | Yes |
| F109 | SVD | benches/LinalgCore/SvdBenchmarks.cs | Yes |
| F110 | Schur Decomposition | benches/LinalgCore/SchurBenchmarks.cs | Yes |
| F111 | Dense Linear Solver | benches/LinalgCore/DenseSolverBenchmarks.cs | Yes |
| F112 | Eigen Solver | benches/LinalgCore/EigenSolverBenchmarks.cs | Yes |
| F113 | Pseudoinverse/Rank/Trace | benches/LinalgCore/MatrixAnalysisBenchmarks.cs | Yes |
| F114 | Sparse CSR/CSC SpMV | benches/LinalgCore/Sparse/SpmvBenchmarks.cs | Yes |
| F200 | Euler/Improved Euler | benches/OdeCore/EulerBenchmarks.cs | Yes |
| F201 | RK4 | benches/OdeCore/Rk4Benchmarks.cs | Yes |
| F202 | Dormand-Prince RK45 | benches/OdeCore/Rk45Benchmarks.cs | Yes |
| F204 | BDF | benches/OdeCore/BdfBenchmarks.cs | Yes |
| F205 | Radau | benches/OdeCore/RadauBenchmarks.cs | Yes |
| F300 | Newton-Raphson | benches/NumericalCore/RootFindingBenchmarks.cs | Yes |
| F301 | Secant | benches/NumericalCore/RootFindingBenchmarks.cs | Yes |
| F302 | Brent | benches/NumericalCore/RootFindingBenchmarks.cs | Yes |
| F303 | Trapezoidal/Simpson | benches/NumericalCore/IntegrationBenchmarks.cs | Yes |
| F304 | Gaussian Quadrature | benches/NumericalCore/IntegrationBenchmarks.cs | Yes |
| F306 | Gradient Descent | benches/NumericalCore/OptimizationBenchmarks.cs | Yes |
| F307 | L-BFGS | benches/NumericalCore/OptimizationBenchmarks.cs | Yes |
| F309 | RBF Interpolation | benches/NumericalCore/InterpolationBenchmarks.cs | Yes |
| F310 | RNG and Distributions | benches/NumericalCore/RandomBenchmarks.cs | Yes |
| F311 | Covariance/Correlation | benches/NumericalCore/StatisticsBenchmarks.cs | Yes |
