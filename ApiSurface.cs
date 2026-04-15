using LAL.LinalgCore;
using LAL.LinalgCore.Sparse;
using LAL.NumericalCore.Differentiation;
using LAL.NumericalCore.Integration;
using LAL.NumericalCore.Interpolation;
using LAL.NumericalCore.Optimization;
using LAL.NumericalCore.Random;
using LAL.NumericalCore.RootFinding;
using LAL.NumericalCore.Statistics;
using LAL.OdeCore;
using LAL.TensorCore;
using System.Numerics;

namespace LAL.Api;

// W3 API surface reference: wrapper-oriented entry points over completed modules.
public static class TensorApi
{
    public static int[] RowMajorStrides(ReadOnlySpan<int> shape) => LAL.TensorCore.TensorShape.ComputeRowMajorStrides(shape);
    public static int Offset(ReadOnlySpan<int> indices, ReadOnlySpan<int> shape, ReadOnlySpan<int> strides) => LAL.TensorCore.TensorShape.GetOffset(indices, shape, strides);
    public static int[] BroadcastShape(ReadOnlySpan<int> left, ReadOnlySpan<int> right) => LAL.TensorCore.Broadcasting.BroadcastShapes(left, right);
    public static double Sum(ReadOnlySpan<double> values) => LAL.TensorCore.Reductions.Sum(values);
    public static float Sum(ReadOnlySpan<float> values) => LAL.TensorCore.Reductions.Sum(values);
    public static Complex Sum(ReadOnlySpan<Complex> values) => LAL.TensorCore.Reductions.Sum(values);
    public static void Exp(ReadOnlySpan<double> values, Span<double> destination) => LAL.TensorCore.UFuncTranscendental.Exp(values, destination);
    public static void Exp(ReadOnlySpan<float> values, Span<float> destination) => LAL.TensorCore.UFuncTranscendental.Exp(values, destination);
    public static void Exp(ReadOnlySpan<Complex> values, Span<Complex> destination) => LAL.TensorCore.UFuncTranscendental.Exp(values, destination);
    public static int SearchSorted(ReadOnlySpan<double> sorted, double value) => LAL.TensorCore.SortSearch.SearchSorted(sorted, value);
    public static int SearchSorted(ReadOnlySpan<float> sorted, float value) => LAL.TensorCore.SortSearch.SearchSorted(sorted, value);
    public static int SearchSorted(ReadOnlySpan<Complex> sorted, Complex value) => LAL.TensorCore.SortSearch.SearchSorted(sorted, value);
    public static ConvolutionParallelSettings GetConvolutionParallelSettings() => LAL.TensorCore.Convolution.GetParallelSettings();
    public static void SetConvolutionParallelSettings(ConvolutionParallelSettings settings) => LAL.TensorCore.Convolution.SetParallelSettings(settings);
    public static void ResetConvolutionParallelSettings() => LAL.TensorCore.Convolution.ResetParallelSettings();

    public static void Re(ReadOnlySpan<Complex> values, Span<double> destination) => LAL.TensorCore.ComplexOps.Re(values, destination);
    public static void Im(ReadOnlySpan<Complex> values, Span<double> destination) => LAL.TensorCore.ComplexOps.Im(values, destination);
    public static void Real(ReadOnlySpan<Complex> values, Span<double> destination) => LAL.TensorCore.ComplexOps.Real(values, destination);
    public static void Imaginary(ReadOnlySpan<Complex> values, Span<double> destination) => LAL.TensorCore.ComplexOps.Imaginary(values, destination);
    public static void Conjugate(ReadOnlySpan<Complex> values, Span<Complex> destination) => LAL.TensorCore.ComplexOps.Conjugate(values, destination);

    public static void Concatenate(ReadOnlySpan<double> left, ReadOnlySpan<double> right, Span<double> destination) => LAL.TensorCore.ConcatStack.Concatenate(left, right, destination);
    public static void Concatenate(ReadOnlySpan<float> left, ReadOnlySpan<float> right, Span<float> destination) => LAL.TensorCore.ConcatStack.Concatenate(left, right, destination);
    public static void Concatenate(ReadOnlySpan<Complex> left, ReadOnlySpan<Complex> right, Span<Complex> destination) => LAL.TensorCore.ConcatStack.Concatenate(left, right, destination);
    public static void Stack(ReadOnlySpan<double> topRow, ReadOnlySpan<double> bottomRow, int cols, Span<double> destination) => LAL.TensorCore.ConcatStack.Stack(topRow, bottomRow, cols, destination);
    public static void Stack(ReadOnlySpan<float> topRow, ReadOnlySpan<float> bottomRow, int cols, Span<float> destination) => LAL.TensorCore.ConcatStack.Stack(topRow, bottomRow, cols, destination);
    public static void Stack(ReadOnlySpan<Complex> topRow, ReadOnlySpan<Complex> bottomRow, int cols, Span<Complex> destination) => LAL.TensorCore.ConcatStack.Stack(topRow, bottomRow, cols, destination);

    public static void CumSum(ReadOnlySpan<double> values, Span<double> destination) => LAL.TensorCore.Cumulative.CumSum(values, destination);
    public static void CumSum(ReadOnlySpan<float> values, Span<float> destination) => LAL.TensorCore.Cumulative.CumSum(values, destination);
    public static void CumSum(ReadOnlySpan<Complex> values, Span<Complex> destination) => LAL.TensorCore.Cumulative.CumSum(values, destination);
    public static void CumProd(ReadOnlySpan<double> values, Span<double> destination) => LAL.TensorCore.Cumulative.CumProd(values, destination);
    public static void CumProd(ReadOnlySpan<float> values, Span<float> destination) => LAL.TensorCore.Cumulative.CumProd(values, destination);
    public static void CumProd(ReadOnlySpan<Complex> values, Span<Complex> destination) => LAL.TensorCore.Cumulative.CumProd(values, destination);

    public static double Evaluate(string signature, ReadOnlySpan<double> left, ReadOnlySpan<double> right) => LAL.TensorCore.Einsum.Evaluate(signature, left, right);
    public static float Evaluate(string signature, ReadOnlySpan<float> left, ReadOnlySpan<float> right) => LAL.TensorCore.Einsum.Evaluate(signature, left, right);
    public static Complex Evaluate(string signature, ReadOnlySpan<Complex> left, ReadOnlySpan<Complex> right) => LAL.TensorCore.Einsum.Evaluate(signature, left, right);
    public static double[,] EvaluateMatMul(string signature, double[,] left, double[,] right) => LAL.TensorCore.Einsum.EvaluateMatMul(signature, left, right);
    public static float[,] EvaluateMatMul(string signature, float[,] left, float[,] right) => LAL.TensorCore.Einsum.EvaluateMatMul(signature, left, right);
    public static Complex[,] EvaluateMatMul(string signature, Complex[,] left, Complex[,] right) => LAL.TensorCore.Einsum.EvaluateMatMul(signature, left, right);
    public static void Compute(string signature, ReadOnlySpan<double> left, ReadOnlySpan<double> right, Span<double> destination, int m = 0, int n = 0, int k = 0) => LAL.TensorCore.Einsum.Compute(signature, left, right, destination, m, n, k);
    public static void Compute(string signature, ReadOnlySpan<float> left, ReadOnlySpan<float> right, Span<float> destination, int m = 0, int n = 0, int k = 0) => LAL.TensorCore.Einsum.Compute(signature, left, right, destination, m, n, k);
    public static void Compute(string signature, ReadOnlySpan<Complex> left, ReadOnlySpan<Complex> right, Span<Complex> destination, int m = 0, int n = 0, int k = 0) => LAL.TensorCore.Einsum.Compute(signature, left, right, destination, m, n, k);
    public static void TensorContractionGemm(ReadOnlySpan<double> left, ReadOnlySpan<double> right, Span<double> destination, int m, int n, int k) => LAL.TensorCore.Einsum.TensorContractionGemm(left, right, destination, m, n, k);
    public static void TensorContractionGemm(ReadOnlySpan<float> left, ReadOnlySpan<float> right, Span<float> destination, int m, int n, int k) => LAL.TensorCore.Einsum.TensorContractionGemm(left, right, destination, m, n, k);
    public static void TensorContractionGemm(ReadOnlySpan<Complex> left, ReadOnlySpan<Complex> right, Span<Complex> destination, int m, int n, int k) => LAL.TensorCore.Einsum.TensorContractionGemm(left, right, destination, m, n, k);
    public static void Kronecker(ReadOnlySpan<double> left, int leftRows, int leftCols, ReadOnlySpan<double> right, int rightRows, int rightCols, Span<double> destination) => LAL.TensorCore.Einsum.Kronecker(left, leftRows, leftCols, right, rightRows, rightCols, destination);
    public static void Kronecker(ReadOnlySpan<float> left, int leftRows, int leftCols, ReadOnlySpan<float> right, int rightRows, int rightCols, Span<float> destination) => LAL.TensorCore.Einsum.Kronecker(left, leftRows, leftCols, right, rightRows, rightCols, destination);
    public static void Kronecker(ReadOnlySpan<Complex> left, int leftRows, int leftCols, ReadOnlySpan<Complex> right, int rightRows, int rightCols, Span<Complex> destination) => LAL.TensorCore.Einsum.Kronecker(left, leftRows, leftCols, right, rightRows, rightCols, destination);

    public static void Forward(ReadOnlySpan<Complex> input, Span<Complex> output) => LAL.TensorCore.Fft.Forward(input, output);
    public static void Inverse(ReadOnlySpan<Complex> input, Span<Complex> output) => LAL.TensorCore.Fft.Inverse(input, output);
    public static void Forward2D(ReadOnlySpan<Complex> input, int rows, int cols, Span<Complex> output) => LAL.TensorCore.Fft.Forward2D(input, rows, cols, output);
    public static void Inverse2D(ReadOnlySpan<Complex> input, int rows, int cols, Span<Complex> output) => LAL.TensorCore.Fft.Inverse2D(input, rows, cols, output);
    public static void ForwardND(ReadOnlySpan<Complex> input, ReadOnlySpan<int> shape, Span<Complex> output) => LAL.TensorCore.Fft.ForwardND(input, shape, output);
    public static void InverseND(ReadOnlySpan<Complex> input, ReadOnlySpan<int> shape, Span<Complex> output) => LAL.TensorCore.Fft.InverseND(input, shape, output);
    public static void Rfft(ReadOnlySpan<double> input, Span<Complex> output) => LAL.TensorCore.Fft.Rfft(input, output);
    public static void Rfft(ReadOnlySpan<float> input, Span<Complex> output) => LAL.TensorCore.Fft.Rfft(input, output);
    public static void RfftND(ReadOnlySpan<double> input, ReadOnlySpan<int> shape, Span<Complex> output) => LAL.TensorCore.Fft.RfftND(input, shape, output);
    public static void RfftND(ReadOnlySpan<float> input, ReadOnlySpan<int> shape, Span<Complex> output) => LAL.TensorCore.Fft.RfftND(input, shape, output);

    public static void GreaterThan(ReadOnlySpan<double> values, double threshold, Span<bool> mask) => LAL.TensorCore.MaskOps.GreaterThan(values, threshold, mask);
    public static void GreaterThan(ReadOnlySpan<float> values, float threshold, Span<bool> mask) => LAL.TensorCore.MaskOps.GreaterThan(values, threshold, mask);
    public static void GreaterThan(ReadOnlySpan<Complex> values, double threshold, Span<bool> mask) => LAL.TensorCore.MaskOps.GreaterThan(values, threshold, mask);
    public static void LessThan(ReadOnlySpan<double> values, double threshold, Span<bool> mask) => LAL.TensorCore.MaskOps.LessThan(values, threshold, mask);
    public static void LessThan(ReadOnlySpan<float> values, float threshold, Span<bool> mask) => LAL.TensorCore.MaskOps.LessThan(values, threshold, mask);
    public static void LessThan(ReadOnlySpan<Complex> values, double threshold, Span<bool> mask) => LAL.TensorCore.MaskOps.LessThan(values, threshold, mask);
    public static void Equal(ReadOnlySpan<double> values, double target, Span<bool> mask, double epsilon = 1e-12) => LAL.TensorCore.MaskOps.Equal(values, target, mask, epsilon);
    public static void Equal(ReadOnlySpan<float> values, float target, Span<bool> mask, float epsilon = 1e-6f) => LAL.TensorCore.MaskOps.Equal(values, target, mask, epsilon);
    public static void Equal(ReadOnlySpan<Complex> values, Complex target, Span<bool> mask, double epsilon = 1e-12) => LAL.TensorCore.MaskOps.Equal(values, target, mask, epsilon);
    public static void Where(ReadOnlySpan<double> values, ReadOnlySpan<bool> mask, Span<double> destination, double fallback = 0d) => LAL.TensorCore.MaskOps.Where(values, mask, destination, fallback);
    public static void Where(ReadOnlySpan<float> values, ReadOnlySpan<bool> mask, Span<float> destination, float fallback = 0f) => LAL.TensorCore.MaskOps.Where(values, mask, destination, fallback);
    public static void Where(ReadOnlySpan<Complex> values, ReadOnlySpan<bool> mask, Span<Complex> destination, Complex fallback) => LAL.TensorCore.MaskOps.Where(values, mask, destination, fallback);
    public static void Where(ReadOnlySpan<Complex> values, ReadOnlySpan<bool> mask, Span<Complex> destination) => LAL.TensorCore.MaskOps.Where(values, mask, destination);

    public static double[] ZeroPad1D(ReadOnlySpan<double> input, int left, int right) => LAL.TensorCore.Padding.ZeroPad1D(input, left, right);
    public static float[] ZeroPad1D(ReadOnlySpan<float> input, int left, int right) => LAL.TensorCore.Padding.ZeroPad1D(input, left, right);
    public static Complex[] ZeroPad1D(ReadOnlySpan<Complex> input, int left, int right) => LAL.TensorCore.Padding.ZeroPad1D(input, left, right);
    public static double[] EdgePad1D(ReadOnlySpan<double> input, int left, int right) => LAL.TensorCore.Padding.EdgePad1D(input, left, right);
    public static float[] EdgePad1D(ReadOnlySpan<float> input, int left, int right) => LAL.TensorCore.Padding.EdgePad1D(input, left, right);
    public static Complex[] EdgePad1D(ReadOnlySpan<Complex> input, int left, int right) => LAL.TensorCore.Padding.EdgePad1D(input, left, right);
    public static double[] PeriodicPad1D(ReadOnlySpan<double> input, int left, int right) => LAL.TensorCore.Padding.PeriodicPad1D(input, left, right);
    public static float[] PeriodicPad1D(ReadOnlySpan<float> input, int left, int right) => LAL.TensorCore.Padding.PeriodicPad1D(input, left, right);
    public static Complex[] PeriodicPad1D(ReadOnlySpan<Complex> input, int left, int right) => LAL.TensorCore.Padding.PeriodicPad1D(input, left, right);

    public static void Reshape(ReadOnlySpan<double> source, Span<double> destination) => LAL.TensorCore.ShapeOps.Reshape(source, destination);
    public static void Reshape(ReadOnlySpan<float> source, Span<float> destination) => LAL.TensorCore.ShapeOps.Reshape(source, destination);
    public static void Reshape(ReadOnlySpan<Complex> source, Span<Complex> destination) => LAL.TensorCore.ShapeOps.Reshape(source, destination);
    public static void ReshapeCopy(ReadOnlySpan<double> source, Span<double> destination) => LAL.TensorCore.ShapeOps.ReshapeCopy(source, destination);
    public static void ReshapeCopy(ReadOnlySpan<float> source, Span<float> destination) => LAL.TensorCore.ShapeOps.ReshapeCopy(source, destination);
    public static void ReshapeCopy(ReadOnlySpan<Complex> source, Span<Complex> destination) => LAL.TensorCore.ShapeOps.ReshapeCopy(source, destination);
    public static void Flatten(ReadOnlySpan<double> source, Span<double> destination) => LAL.TensorCore.ShapeOps.Flatten(source, destination);
    public static void Flatten(ReadOnlySpan<float> source, Span<float> destination) => LAL.TensorCore.ShapeOps.Flatten(source, destination);
    public static void Flatten(ReadOnlySpan<Complex> source, Span<Complex> destination) => LAL.TensorCore.ShapeOps.Flatten(source, destination);
    public static void Transpose2D(ReadOnlySpan<double> source, int rows, int cols, Span<double> destination) => LAL.TensorCore.ShapeOps.Transpose2D(source, rows, cols, destination);
    public static void Transpose2D(ReadOnlySpan<float> source, int rows, int cols, Span<float> destination) => LAL.TensorCore.ShapeOps.Transpose2D(source, rows, cols, destination);
    public static void Transpose2D(ReadOnlySpan<Complex> source, int rows, int cols, Span<Complex> destination) => LAL.TensorCore.ShapeOps.Transpose2D(source, rows, cols, destination);
    public static void SwapAxes2D(ReadOnlySpan<double> source, int rows, int cols, Span<double> destination) => LAL.TensorCore.ShapeOps.SwapAxes2D(source, rows, cols, destination);
    public static void SwapAxes2D(ReadOnlySpan<float> source, int rows, int cols, Span<float> destination) => LAL.TensorCore.ShapeOps.SwapAxes2D(source, rows, cols, destination);
    public static void SwapAxes2D(ReadOnlySpan<Complex> source, int rows, int cols, Span<Complex> destination) => LAL.TensorCore.ShapeOps.SwapAxes2D(source, rows, cols, destination);
    public static int[] ExpandDims(ReadOnlySpan<int> shape, int axis) => LAL.TensorCore.ShapeOps.ExpandDims(shape, axis);
    public static int[] Squeeze(ReadOnlySpan<int> shape, int axis = -1) => LAL.TensorCore.ShapeOps.Squeeze(shape, axis);
    public static int[] SwapAxes(ReadOnlySpan<int> shape, int axisA, int axisB) => LAL.TensorCore.ShapeOps.SwapAxes(shape, axisA, axisB);
    public static int[] TransposeShape(ReadOnlySpan<int> shape, ReadOnlySpan<int> permutation) => LAL.TensorCore.ShapeOps.TransposeShape(shape, permutation);

    public static ReadOnlySpan<T> Slice1D<T>(ReadOnlySpan<T> data, int start, int length) => LAL.TensorCore.StridedView.Slice1D(data, start, length);
    public static Span<T> Slice1D<T>(Span<T> data, int start, int length) => LAL.TensorCore.StridedView.Slice1D(data, start, length);
    public static int Slice1D<T>(ReadOnlySpan<T> data, int start, int stop, int step, Span<T> destination) => LAL.TensorCore.StridedView.Slice1D(data, start, stop, step, destination);
    public static int[] ComputeSlicedShape(ReadOnlySpan<int> shape, int axis, int start, int length) => LAL.TensorCore.StridedView.ComputeSlicedShape(shape, axis, start, length);
    public static int[] PermuteShape(ReadOnlySpan<int> shape, ReadOnlySpan<int> permutation) => LAL.TensorCore.StridedView.PermuteShape(shape, permutation);
    public static int[] ExpandDimsView(ReadOnlySpan<int> shape, int axis) => LAL.TensorCore.StridedView.ExpandDims(shape, axis);
    public static int[] SqueezeView(ReadOnlySpan<int> shape, int axis = -1) => LAL.TensorCore.StridedView.Squeeze(shape, axis);

    public static void Add(ReadOnlySpan<double> left, ReadOnlySpan<double> right, Span<double> destination) => LAL.TensorCore.UFuncArithmetic.Add(left, right, destination);
    public static void Add(ReadOnlySpan<float> left, ReadOnlySpan<float> right, Span<float> destination) => LAL.TensorCore.UFuncArithmetic.Add(left, right, destination);
    public static void Add(ReadOnlySpan<Complex> left, ReadOnlySpan<Complex> right, Span<Complex> destination) => LAL.TensorCore.UFuncArithmetic.Add(left, right, destination);
    public static void Subtract(ReadOnlySpan<double> left, ReadOnlySpan<double> right, Span<double> destination) => LAL.TensorCore.UFuncArithmetic.Subtract(left, right, destination);
    public static void Subtract(ReadOnlySpan<float> left, ReadOnlySpan<float> right, Span<float> destination) => LAL.TensorCore.UFuncArithmetic.Subtract(left, right, destination);
    public static void Subtract(ReadOnlySpan<Complex> left, ReadOnlySpan<Complex> right, Span<Complex> destination) => LAL.TensorCore.UFuncArithmetic.Subtract(left, right, destination);
    public static void Multiply(ReadOnlySpan<double> left, ReadOnlySpan<double> right, Span<double> destination) => LAL.TensorCore.UFuncArithmetic.Multiply(left, right, destination);
    public static void Multiply(ReadOnlySpan<float> left, ReadOnlySpan<float> right, Span<float> destination) => LAL.TensorCore.UFuncArithmetic.Multiply(left, right, destination);
    public static void Multiply(ReadOnlySpan<Complex> left, ReadOnlySpan<Complex> right, Span<Complex> destination) => LAL.TensorCore.UFuncArithmetic.Multiply(left, right, destination);
    public static void Divide(ReadOnlySpan<double> numerator, ReadOnlySpan<double> denominator, Span<double> destination) => LAL.TensorCore.UFuncArithmetic.Divide(numerator, denominator, destination);
    public static void Divide(ReadOnlySpan<float> numerator, ReadOnlySpan<float> denominator, Span<float> destination) => LAL.TensorCore.UFuncArithmetic.Divide(numerator, denominator, destination);
    public static void Divide(ReadOnlySpan<Complex> numerator, ReadOnlySpan<Complex> denominator, Span<Complex> destination) => LAL.TensorCore.UFuncArithmetic.Divide(numerator, denominator, destination);
    public static void Power(ReadOnlySpan<double> basis, double exponent, Span<double> destination) => LAL.TensorCore.UFuncArithmetic.Power(basis, exponent, destination);
    public static void Power(ReadOnlySpan<double> basis, ReadOnlySpan<double> exponent, Span<double> destination) => LAL.TensorCore.UFuncArithmetic.Power(basis, exponent, destination);
    public static void Power(ReadOnlySpan<float> basis, float exponent, Span<float> destination) => LAL.TensorCore.UFuncArithmetic.Power(basis, exponent, destination);
    public static void Power(ReadOnlySpan<float> basis, ReadOnlySpan<float> exponent, Span<float> destination) => LAL.TensorCore.UFuncArithmetic.Power(basis, exponent, destination);
    public static void Power(ReadOnlySpan<Complex> basis, double exponent, Span<Complex> destination) => LAL.TensorCore.UFuncArithmetic.Power(basis, exponent, destination);
    public static void Power(ReadOnlySpan<Complex> basis, ReadOnlySpan<Complex> exponent, Span<Complex> destination) => LAL.TensorCore.UFuncArithmetic.Power(basis, exponent, destination);
}

public static class LinalgApi
{
    public static void Axpy(double alpha, ReadOnlySpan<double> x, Span<double> y) => LAL.LinalgCore.Axpy.Compute(alpha, x, y);
    public static double Dot(ReadOnlySpan<double> x, ReadOnlySpan<double> y) => LAL.LinalgCore.Dot.Dotu(x, y);
    public static void Gemv(ReadOnlySpan<double> a, int rows, int cols, ReadOnlySpan<double> x, Span<double> y) => LAL.LinalgCore.Gemv.Multiply(a, rows, cols, x, y);
    public static void Gemm(ReadOnlySpan<double> a, ReadOnlySpan<double> b, Span<double> c, int m, int n, int k) => LAL.LinalgCore.Gemm.Multiply(a, b, c, m, n, k);
    public static void Spmv(ReadOnlySpan<double> values, ReadOnlySpan<int> colIndices, ReadOnlySpan<int> rowPointers, ReadOnlySpan<double> x, Span<double> y) => LAL.LinalgCore.Sparse.Spmv.CsrMultiply(values, colIndices, rowPointers, x, y);

    public static bool CholeskyDecomposeLower(ReadOnlySpan<double> matrix, int n, Span<double> lower, double positiveTolerance = 1e-12) => LAL.LinalgCore.Cholesky.DecomposeLower(matrix, n, lower, positiveTolerance);
    public static bool DenseSolve(ReadOnlySpan<double> matrix, int n, ReadOnlySpan<double> b, Span<double> x, double singularTolerance = 1e-12) => LAL.LinalgCore.DenseSolver.Solve(matrix, n, b, x, singularTolerance);

    public static void PairwiseEuclidean(ReadOnlySpan<double> points, int pointCount, int dimension, Span<double> distances) => LAL.LinalgCore.DistanceMetrics.PairwiseEuclidean(points, pointCount, dimension, distances);
    public static void PairwiseCosine(ReadOnlySpan<double> points, int pointCount, int dimension, Span<double> distances) => LAL.LinalgCore.DistanceMetrics.PairwiseCosine(points, pointCount, dimension, distances);
    public static void PairwiseMahalanobis(ReadOnlySpan<double> points, int pointCount, int dimension, ReadOnlySpan<double> inverseCovariance, Span<double> distances) => LAL.LinalgCore.DistanceMetrics.PairwiseMahalanobis(points, pointCount, dimension, inverseCovariance, distances);

    public static EigenResult PowerIteration(ReadOnlySpan<double> matrix, int n, Span<double> eigenvector, double tolerance = 1e-10, int maxIterations = 1_000) => LAL.LinalgCore.EigenSolver.PowerIteration(matrix, n, eigenvector, tolerance, maxIterations);

    public static LuDecompositionResult LuDecomposeInPlace(Span<double> matrix, int n, Span<int> pivots, double singularTolerance = 1e-12) => LAL.LinalgCore.Lu.DecomposeInPlace(matrix, n, pivots, singularTolerance);
    public static bool LuSolve(ReadOnlySpan<double> lu, int n, ReadOnlySpan<int> pivots, ReadOnlySpan<double> b, Span<double> x, double singularTolerance = 1e-12) => LAL.LinalgCore.Lu.Solve(lu, n, pivots, b, x, singularTolerance);
    public static bool LuFactorAndSolve(ReadOnlySpan<double> matrix, int n, ReadOnlySpan<double> b, Span<double> x, double singularTolerance = 1e-12) => LAL.LinalgCore.Lu.FactorAndSolve(matrix, n, b, x, singularTolerance);

    public static double Determinant(ReadOnlySpan<double> matrix, int n, double singularTolerance = 1e-12) => LAL.LinalgCore.MatrixAnalysis.Determinant(matrix, n, singularTolerance);
    public static bool Inverse(ReadOnlySpan<double> matrix, int n, Span<double> inverse, double singularTolerance = 1e-12) => LAL.LinalgCore.MatrixAnalysis.Inverse(matrix, n, inverse, singularTolerance);
    public static double Trace(ReadOnlySpan<double> matrix, int n) => LAL.LinalgCore.MatrixAnalysis.Trace(matrix, n);
    public static int Rank(ReadOnlySpan<double> matrix, int rows, int cols, double tolerance = 1e-10) => LAL.LinalgCore.MatrixAnalysis.Rank(matrix, rows, cols, tolerance);
    public static bool PseudoInverseSquare(ReadOnlySpan<double> matrix, int n, Span<double> pseudoInverse, double singularTolerance = 1e-12) => LAL.LinalgCore.MatrixAnalysis.PseudoInverseSquare(matrix, n, pseudoInverse, singularTolerance);
    public static bool PseudoInverse(ReadOnlySpan<double> matrix, int rows, int cols, Span<double> pseudoInverse, double singularTolerance = 1e-12) => LAL.LinalgCore.MatrixAnalysis.PseudoInverse(matrix, rows, cols, pseudoInverse, singularTolerance);
    public static EigenResult DominantEigenvalue(ReadOnlySpan<double> matrix, int n, Span<double> eigenvector, double tolerance = 1e-10, int maxIterations = 1_000) => LAL.LinalgCore.MatrixAnalysis.DominantEigenvalue(matrix, n, eigenvector, tolerance, maxIterations);
    public static void Eigenvalues2x2(ReadOnlySpan<double> matrix, Span<Complex> eigenvalues) => LAL.LinalgCore.MatrixAnalysis.Eigenvalues2x2(matrix, eigenvalues);

    public static void MatrixAdd(ReadOnlySpan<double> left, ReadOnlySpan<double> right, Span<double> destination, int rows, int cols) => LAL.LinalgCore.MatrixOps.Add(left, right, destination, rows, cols);
    public static void MatrixAdd(ReadOnlySpan<float> left, ReadOnlySpan<float> right, Span<float> destination, int rows, int cols) => LAL.LinalgCore.MatrixOps.Add(left, right, destination, rows, cols);
    public static void MatrixAdd(ReadOnlySpan<Complex> left, ReadOnlySpan<Complex> right, Span<Complex> destination, int rows, int cols) => LAL.LinalgCore.MatrixOps.Add(left, right, destination, rows, cols);
    public static void MatrixSubtract(ReadOnlySpan<double> left, ReadOnlySpan<double> right, Span<double> destination, int rows, int cols) => LAL.LinalgCore.MatrixOps.Subtract(left, right, destination, rows, cols);
    public static void MatrixSubtract(ReadOnlySpan<float> left, ReadOnlySpan<float> right, Span<float> destination, int rows, int cols) => LAL.LinalgCore.MatrixOps.Subtract(left, right, destination, rows, cols);
    public static void MatrixSubtract(ReadOnlySpan<Complex> left, ReadOnlySpan<Complex> right, Span<Complex> destination, int rows, int cols) => LAL.LinalgCore.MatrixOps.Subtract(left, right, destination, rows, cols);
    public static void MatrixMultiply(ReadOnlySpan<double> matrix, int rows, int cols, ReadOnlySpan<double> vector, Span<double> destination) => LAL.LinalgCore.MatrixOps.Multiply(matrix, rows, cols, vector, destination);
    public static void MatrixMultiply(ReadOnlySpan<float> matrix, int rows, int cols, ReadOnlySpan<float> vector, Span<float> destination) => LAL.LinalgCore.MatrixOps.Multiply(matrix, rows, cols, vector, destination);
    public static void MatrixMultiply(ReadOnlySpan<Complex> matrix, int rows, int cols, ReadOnlySpan<Complex> vector, Span<Complex> destination) => LAL.LinalgCore.MatrixOps.Multiply(matrix, rows, cols, vector, destination);
    public static void MatrixMultiply(ReadOnlySpan<double> left, int leftRows, int sharedDim, ReadOnlySpan<double> right, int rightCols, Span<double> destination) => LAL.LinalgCore.MatrixOps.Multiply(left, leftRows, sharedDim, right, rightCols, destination);
    public static void MatrixMultiply(ReadOnlySpan<float> left, int leftRows, int sharedDim, ReadOnlySpan<float> right, int rightCols, Span<float> destination) => LAL.LinalgCore.MatrixOps.Multiply(left, leftRows, sharedDim, right, rightCols, destination);
    public static void MatrixMultiply(ReadOnlySpan<Complex> left, int leftRows, int sharedDim, ReadOnlySpan<Complex> right, int rightCols, Span<Complex> destination) => LAL.LinalgCore.MatrixOps.Multiply(left, leftRows, sharedDim, right, rightCols, destination);
    public static double MatrixDot(ReadOnlySpan<double> left, ReadOnlySpan<double> right, int rows, int cols) => LAL.LinalgCore.MatrixOps.Dot(left, right, rows, cols);
    public static float MatrixDot(ReadOnlySpan<float> left, ReadOnlySpan<float> right, int rows, int cols) => LAL.LinalgCore.MatrixOps.Dot(left, right, rows, cols);
    public static Complex MatrixDot(ReadOnlySpan<Complex> left, ReadOnlySpan<Complex> right, int rows, int cols) => LAL.LinalgCore.MatrixOps.Dot(left, right, rows, cols);

    public static double NormL1(ReadOnlySpan<double> values) => LAL.LinalgCore.Norms.L1(values);
    public static float NormL1(ReadOnlySpan<float> values) => LAL.LinalgCore.Norms.L1(values);
    public static double NormL1(ReadOnlySpan<Complex> values) => LAL.LinalgCore.Norms.L1(values);
    public static double NormL2Squared(ReadOnlySpan<double> values) => LAL.LinalgCore.Norms.L2Squared(values);
    public static float NormL2Squared(ReadOnlySpan<float> values) => LAL.LinalgCore.Norms.L2Squared(values);
    public static double NormL2Squared(ReadOnlySpan<Complex> values) => LAL.LinalgCore.Norms.L2Squared(values);
    public static double NormL2(ReadOnlySpan<double> values) => LAL.LinalgCore.Norms.L2(values);
    public static float NormL2(ReadOnlySpan<float> values) => LAL.LinalgCore.Norms.L2(values);
    public static double NormL2(ReadOnlySpan<Complex> values) => LAL.LinalgCore.Norms.L2(values);
    public static double NormInfinity(ReadOnlySpan<double> values) => LAL.LinalgCore.Norms.Infinity(values);
    public static float NormInfinity(ReadOnlySpan<float> values) => LAL.LinalgCore.Norms.Infinity(values);
    public static double NormInfinity(ReadOnlySpan<Complex> values) => LAL.LinalgCore.Norms.Infinity(values);

    public static double ProjectionCoefficient(ReadOnlySpan<double> vector, ReadOnlySpan<double> basis) => LAL.LinalgCore.Orthogonalization.ProjectionCoefficient(vector, basis);
    public static void OrthogonalProjection(ReadOnlySpan<double> vector, ReadOnlySpan<double> basis, Span<double> destination) => LAL.LinalgCore.Orthogonalization.OrthogonalProjection(vector, basis, destination);
    public static int GramSchmidtOrthonormalize(ReadOnlySpan<double> vectors, int vectorCount, int dimension, Span<double> orthonormalVectors, double tolerance = 1e-10) => LAL.LinalgCore.Orthogonalization.GramSchmidtOrthonormalize(vectors, vectorCount, dimension, orthonormalVectors, tolerance);
    public static bool QrDecomposeThin(ReadOnlySpan<double> matrix, int rows, int cols, Span<double> q, Span<double> r, double tolerance = 1e-12) => LAL.LinalgCore.Qr.DecomposeThin(matrix, rows, cols, q, r, tolerance);
    public static void RealSchur2x2(ReadOnlySpan<double> matrix, Span<double> schur, int iterations = 32) => LAL.LinalgCore.Schur.RealSchur2x2(matrix, schur, iterations);
    public static void SingularValues(ReadOnlySpan<double> matrix, int rows, int cols, Span<double> singularValues, int maxSweeps = 32) => LAL.LinalgCore.Svd.SingularValues(matrix, rows, cols, singularValues, maxSweeps);

    public static void TransposeMatrix(ReadOnlySpan<double> input, int rows, int cols, Span<double> output) => LAL.LinalgCore.Transpose.Matrix(input, rows, cols, output);
    public static void TransposeMatrix(ReadOnlySpan<float> input, int rows, int cols, Span<float> output) => LAL.LinalgCore.Transpose.Matrix(input, rows, cols, output);
    public static void TransposeMatrix(ReadOnlySpan<Complex> input, int rows, int cols, Span<Complex> output) => LAL.LinalgCore.Transpose.Matrix(input, rows, cols, output);
    public static void ConjugateTransposeMatrix(ReadOnlySpan<Complex> input, int rows, int cols, Span<Complex> output) => LAL.LinalgCore.Transpose.ConjugateTranspose(input, rows, cols, output);

    public static void VectorAdd(ReadOnlySpan<double> left, ReadOnlySpan<double> right, Span<double> destination) => LAL.LinalgCore.VectorOps.Add(left, right, destination);
    public static void VectorAdd(ReadOnlySpan<float> left, ReadOnlySpan<float> right, Span<float> destination) => LAL.LinalgCore.VectorOps.Add(left, right, destination);
    public static void VectorAdd(ReadOnlySpan<Complex> left, ReadOnlySpan<Complex> right, Span<Complex> destination) => LAL.LinalgCore.VectorOps.Add(left, right, destination);
    public static void VectorSubtract(ReadOnlySpan<double> left, ReadOnlySpan<double> right, Span<double> destination) => LAL.LinalgCore.VectorOps.Subtract(left, right, destination);
    public static void VectorSubtract(ReadOnlySpan<float> left, ReadOnlySpan<float> right, Span<float> destination) => LAL.LinalgCore.VectorOps.Subtract(left, right, destination);
    public static void VectorSubtract(ReadOnlySpan<Complex> left, ReadOnlySpan<Complex> right, Span<Complex> destination) => LAL.LinalgCore.VectorOps.Subtract(left, right, destination);
    public static double VectorDot(ReadOnlySpan<double> left, ReadOnlySpan<double> right) => LAL.LinalgCore.VectorOps.Dot(left, right);
    public static float VectorDot(ReadOnlySpan<float> left, ReadOnlySpan<float> right) => LAL.LinalgCore.VectorOps.Dot(left, right);
    public static Complex VectorDot(ReadOnlySpan<Complex> left, ReadOnlySpan<Complex> right) => LAL.LinalgCore.VectorOps.Dot(left, right);
    public static double VectorInnerProduct(ReadOnlySpan<double> left, ReadOnlySpan<double> right) => LAL.LinalgCore.VectorOps.InnerProduct(left, right);
    public static float VectorInnerProduct(ReadOnlySpan<float> left, ReadOnlySpan<float> right) => LAL.LinalgCore.VectorOps.InnerProduct(left, right);
    public static Complex VectorInnerProduct(ReadOnlySpan<Complex> left, ReadOnlySpan<Complex> right) => LAL.LinalgCore.VectorOps.InnerProduct(left, right);
    public static void VectorOuterProduct(ReadOnlySpan<double> left, ReadOnlySpan<double> right, Span<double> destination) => LAL.LinalgCore.VectorOps.OuterProduct(left, right, destination);
    public static void VectorOuterProduct(ReadOnlySpan<float> left, ReadOnlySpan<float> right, Span<float> destination) => LAL.LinalgCore.VectorOps.OuterProduct(left, right, destination);
    public static void VectorOuterProduct(ReadOnlySpan<Complex> left, ReadOnlySpan<Complex> right, Span<Complex> destination) => LAL.LinalgCore.VectorOps.OuterProduct(left, right, destination);
}

public static class OdeApi
{
    public static void EulerStep(double t, double dt, ReadOnlySpan<double> y, Span<double> yOut, OdeSystem system) => LAL.OdeCore.Euler.Step(t, dt, y, yOut, system);
    public static void EulerStep(float t, float dt, ReadOnlySpan<float> y, Span<float> yOut, OdeSystemFloat system) => LAL.OdeCore.Euler.Step(t, dt, y, yOut, system);
    public static void EulerStep(double t, double dt, ReadOnlySpan<Complex> y, Span<Complex> yOut, OdeSystemComplex system) => LAL.OdeCore.Euler.Step(t, dt, y, yOut, system);
    public static void Rk4Step(double t, double dt, ReadOnlySpan<double> y, Span<double> yOut, OdeSystem system) => LAL.OdeCore.Rk4.Step(t, dt, y, yOut, system);
    public static void Rk4Step(float t, float dt, ReadOnlySpan<float> y, Span<float> yOut, OdeSystemFloat system) => LAL.OdeCore.Rk4.Step(t, dt, y, yOut, system);
    public static void Rk4Step(double t, double dt, ReadOnlySpan<Complex> y, Span<Complex> yOut, OdeSystemComplex system) => LAL.OdeCore.Rk4.Step(t, dt, y, yOut, system);
    public static Rk45StepResult Rk45Step(double t, double dt, ReadOnlySpan<double> y, Span<double> yOut, OdeSystem system) => LAL.OdeCore.Rk45.Step(t, dt, y, yOut, system);
    public static Rk45StepResultFloat Rk45Step(float t, float dt, ReadOnlySpan<float> y, Span<float> yOut, OdeSystemFloat system) => LAL.OdeCore.Rk45.Step(t, dt, y, yOut, system);
    public static Rk45StepResult Rk45Step(double t, double dt, ReadOnlySpan<Complex> y, Span<Complex> yOut, OdeSystemComplex system) => LAL.OdeCore.Rk45.Step(t, dt, y, yOut, system);

    public static BdfStepResult StepBackwardEuler(double t, double dt, ReadOnlySpan<double> y, Span<double> yOut, OdeSystem system, int maxIterations = 8, double tolerance = 1e-10) => LAL.OdeCore.Bdf.StepBackwardEuler(t, dt, y, yOut, system, maxIterations, tolerance);
    public static BdfStepResultFloat StepBackwardEuler(float t, float dt, ReadOnlySpan<float> y, Span<float> yOut, OdeSystemFloat system, int maxIterations = 8, float tolerance = 1e-6f) => LAL.OdeCore.Bdf.StepBackwardEuler(t, dt, y, yOut, system, maxIterations, tolerance);
    public static BdfStepResult StepBackwardEuler(double t, double dt, ReadOnlySpan<Complex> y, Span<Complex> yOut, OdeSystemComplex system, int maxIterations = 8, double tolerance = 1e-10) => LAL.OdeCore.Bdf.StepBackwardEuler(t, dt, y, yOut, system, maxIterations, tolerance);

    public static RadauStepResult StepOneStage(double t, double dt, ReadOnlySpan<double> y, Span<double> yOut, OdeSystem system, int maxIterations = 8, double tolerance = 1e-10) => LAL.OdeCore.Radau.StepOneStage(t, dt, y, yOut, system, maxIterations, tolerance);
    public static RadauStepResultFloat StepOneStage(float t, float dt, ReadOnlySpan<float> y, Span<float> yOut, OdeSystemFloat system, int maxIterations = 8, float tolerance = 1e-6f) => LAL.OdeCore.Radau.StepOneStage(t, dt, y, yOut, system, maxIterations, tolerance);
    public static RadauStepResult StepOneStage(double t, double dt, ReadOnlySpan<Complex> y, Span<Complex> yOut, OdeSystemComplex system, int maxIterations = 8, double tolerance = 1e-10) => LAL.OdeCore.Radau.StepOneStage(t, dt, y, yOut, system, maxIterations, tolerance);

    public static void InterpolateLinear(ReadOnlySpan<double> y0, ReadOnlySpan<double> y1, double theta, Span<double> yOut) => LAL.OdeCore.DenseOutput.InterpolateLinear(y0, y1, theta, yOut);
    public static void InterpolateLinear(ReadOnlySpan<float> y0, ReadOnlySpan<float> y1, float theta, Span<float> yOut) => LAL.OdeCore.DenseOutput.InterpolateLinear(y0, y1, theta, yOut);
    public static void InterpolateLinear(ReadOnlySpan<Complex> y0, ReadOnlySpan<Complex> y1, double theta, Span<Complex> yOut) => LAL.OdeCore.DenseOutput.InterpolateLinear(y0, y1, theta, yOut);

    public static void EstimateForwardDifference(double t, ReadOnlySpan<double> y, Span<double> jacobian, OdeSystem system, double epsilon = 1e-6, bool allowParallel = false) => LAL.OdeCore.JacobianEstimator.EstimateForwardDifference(t, y, jacobian, system, epsilon, allowParallel);
    public static void EstimateForwardDifference(float t, ReadOnlySpan<float> y, Span<float> jacobian, OdeSystemFloat system, float epsilon = 1e-4f, bool allowParallel = false) => LAL.OdeCore.JacobianEstimator.EstimateForwardDifference(t, y, jacobian, system, epsilon, allowParallel);
    public static void EstimateForwardDifference(double t, ReadOnlySpan<Complex> y, Span<Complex> jacobian, OdeSystemComplex system, double epsilon = 1e-6, bool allowParallel = false) => LAL.OdeCore.JacobianEstimator.EstimateForwardDifference(t, y, jacobian, system, epsilon, allowParallel);

    public static StepControlResult ProposeStep(double currentStep, double estimatedError, double tolerance, double safety = 0.9, double minScale = 0.2, double maxScale = 5.0) => LAL.OdeCore.StepController.Propose(currentStep, estimatedError, tolerance, safety, minScale, maxScale);
    public static StepControlResultFloat ProposeStep(float currentStep, float estimatedError, float tolerance, float safety = 0.9f, float minScale = 0.2f, float maxScale = 5.0f) => LAL.OdeCore.StepController.Propose(currentStep, estimatedError, tolerance, safety, minScale, maxScale);
}

public static class NumericalApi
{
    public static NewtonResult Newton(Func<double, double> f, Func<double, double> df, double initialGuess) => LAL.NumericalCore.RootFinding.Newton.Solve(f, df, initialGuess);
    public static SecantResult Secant(Func<double, double> f, double x0, double x1) => LAL.NumericalCore.RootFinding.Secant.Solve(f, x0, x1);
    public static BrentResult Brent(Func<double, double> f, double lower, double upper) => LAL.NumericalCore.RootFinding.Brent.Solve(f, lower, upper);
    public static double Trapezoidal(Func<double, double> f, double a, double b, int intervals) => LAL.NumericalCore.Integration.BasicQuadrature.Trapezoidal(f, a, b, intervals);
    public static GradientDescentResult GradientDescent(Func<double, double> f, Func<double, double> gradient, double initialX) => LAL.NumericalCore.Optimization.GradientDescent.SolveScalar(f, gradient, initialX);
    public static double Covariance(ReadOnlySpan<double> x, ReadOnlySpan<double> y) => LAL.NumericalCore.Statistics.Covariance.Compute(x, y);
    public static double Correlation(ReadOnlySpan<double> x, ReadOnlySpan<double> y) => LAL.NumericalCore.Statistics.Covariance.Correlation(x, y);
    public static double CentralDiff(Func<double, double> f, double x, double h = 1e-6) => LAL.NumericalCore.Differentiation.FiniteDifference.Central(f, x, h);

    public static double GaussianIntegrate(Func<double, double> f, double a, double b, int order = 2) => LAL.NumericalCore.Integration.GaussianQuadrature.Integrate(f, a, b, order);
    public static float GaussianIntegrate(Func<float, float> f, float a, float b, int order = 2) => LAL.NumericalCore.Integration.GaussianQuadrature.Integrate(f, a, b, order);
    public static Complex GaussianIntegrate(Func<double, Complex> f, double a, double b, int order = 2) => LAL.NumericalCore.Integration.GaussianQuadrature.Integrate(f, a, b, order);

    public static LbfgsResult LbfgsSolveScalar(Func<double, double> f, Func<double, double> gradient, double initialX, double tolerance = 1e-8, int maxIterations = 200, int historySize = 5) => LAL.NumericalCore.Optimization.Lbfgs.SolveScalar(f, gradient, initialX, tolerance, maxIterations, historySize);
    public static LbfgsResultFloat LbfgsSolveScalar(Func<float, float> f, Func<float, float> gradient, float initialX, float tolerance = 1e-5f, int maxIterations = 200, int historySize = 5) => LAL.NumericalCore.Optimization.Lbfgs.SolveScalar(f, gradient, initialX, tolerance, maxIterations, historySize);
    public static LbfgsComplexResult LbfgsSolveScalar(Func<Complex, double> f, Func<Complex, Complex> gradient, Complex initialX, double tolerance = 1e-8, int maxIterations = 200, int historySize = 5) => LAL.NumericalCore.Optimization.Lbfgs.SolveScalar(f, gradient, initialX, tolerance, maxIterations, historySize);

    public static double PearsonCompute(ReadOnlySpan<double> x, ReadOnlySpan<double> y, bool allowParallel = false) => LAL.NumericalCore.Statistics.PearsonCorrelation.Compute(x, y, allowParallel);
    public static float PearsonCompute(ReadOnlySpan<float> x, ReadOnlySpan<float> y, bool allowParallel = false) => LAL.NumericalCore.Statistics.PearsonCorrelation.Compute(x, y, allowParallel);
    public static Complex PearsonCompute(ReadOnlySpan<Complex> x, ReadOnlySpan<Complex> y, bool allowParallel = false) => LAL.NumericalCore.Statistics.PearsonCorrelation.Compute(x, y, allowParallel);

    public static bool RbfComputeGaussianWeights(ReadOnlySpan<double> centers, ReadOnlySpan<double> values, double epsilon, Span<double> weights, double singularTolerance = 1e-12, bool allowParallel = false) => LAL.NumericalCore.Interpolation.Rbf.ComputeGaussianWeights(centers, values, epsilon, weights, singularTolerance, allowParallel);
    public static bool RbfComputeGaussianWeights(ReadOnlySpan<float> centers, ReadOnlySpan<float> values, float epsilon, Span<float> weights, float singularTolerance = 1e-6f, bool allowParallel = false) => LAL.NumericalCore.Interpolation.Rbf.ComputeGaussianWeights(centers, values, epsilon, weights, singularTolerance, allowParallel);
    public static bool RbfComputeGaussianWeights(ReadOnlySpan<double> centers, ReadOnlySpan<Complex> values, double epsilon, Span<Complex> weights, double singularTolerance = 1e-12, bool allowParallel = false) => LAL.NumericalCore.Interpolation.Rbf.ComputeGaussianWeights(centers, values, epsilon, weights, singularTolerance, allowParallel);
    public static double RbfEvaluateGaussian(ReadOnlySpan<double> centers, ReadOnlySpan<double> weights, double x, double epsilon) => LAL.NumericalCore.Interpolation.Rbf.EvaluateGaussian(centers, weights, x, epsilon);
    public static float RbfEvaluateGaussian(ReadOnlySpan<float> centers, ReadOnlySpan<float> weights, float x, float epsilon) => LAL.NumericalCore.Interpolation.Rbf.EvaluateGaussian(centers, weights, x, epsilon);
    public static Complex RbfEvaluateGaussian(ReadOnlySpan<double> centers, ReadOnlySpan<Complex> weights, double x, double epsilon) => LAL.NumericalCore.Interpolation.Rbf.EvaluateGaussian(centers, weights, x, epsilon);

    public static uint RngNormalizeSeed(uint seed) => LAL.NumericalCore.Random.Rng.NormalizeSeed(seed);
    public static uint RngNextUInt(ref uint state) => LAL.NumericalCore.Random.Rng.NextUInt(ref state);
    public static double RngNextUniform(ref uint state) => LAL.NumericalCore.Random.Rng.NextUniform(ref state);
    public static double RngNextNormal(ref uint state) => LAL.NumericalCore.Random.Rng.NextNormal(ref state);
    public static float RngNextUniformFloat(ref uint state) => LAL.NumericalCore.Random.Rng.NextUniformFloat(ref state);
    public static float RngNextNormalFloat(ref uint state) => LAL.NumericalCore.Random.Rng.NextNormalFloat(ref state);
    public static Complex RngNextComplexUniform(ref uint state) => LAL.NumericalCore.Random.Rng.NextComplexUniform(ref state);
    public static Complex RngNextComplexNormal(ref uint state) => LAL.NumericalCore.Random.Rng.NextComplexNormal(ref state);

    public static void ComputeNaturalSecondDerivatives(ReadOnlySpan<double> xs, ReadOnlySpan<double> ys, Span<double> secondDerivatives) => LAL.NumericalCore.Interpolation.Spline.ComputeNaturalSecondDerivatives(xs, ys, secondDerivatives);
    public static void ComputeNaturalSecondDerivatives(ReadOnlySpan<float> xs, ReadOnlySpan<float> ys, Span<float> secondDerivatives) => LAL.NumericalCore.Interpolation.Spline.ComputeNaturalSecondDerivatives(xs, ys, secondDerivatives);
    public static void ComputeNaturalSecondDerivatives(ReadOnlySpan<double> xs, ReadOnlySpan<Complex> ys, Span<Complex> secondDerivatives) => LAL.NumericalCore.Interpolation.Spline.ComputeNaturalSecondDerivatives(xs, ys, secondDerivatives);
    public static double EvaluateNaturalCubic(ReadOnlySpan<double> xs, ReadOnlySpan<double> ys, ReadOnlySpan<double> secondDerivatives, double x) => LAL.NumericalCore.Interpolation.Spline.EvaluateNaturalCubic(xs, ys, secondDerivatives, x);
    public static float EvaluateNaturalCubic(ReadOnlySpan<float> xs, ReadOnlySpan<float> ys, ReadOnlySpan<float> secondDerivatives, float x) => LAL.NumericalCore.Interpolation.Spline.EvaluateNaturalCubic(xs, ys, secondDerivatives, x);
    public static Complex EvaluateNaturalCubic(ReadOnlySpan<double> xs, ReadOnlySpan<Complex> ys, ReadOnlySpan<Complex> secondDerivatives, double x) => LAL.NumericalCore.Interpolation.Spline.EvaluateNaturalCubic(xs, ys, secondDerivatives, x);

    public static double ZigguratNextU01(ref uint state) => LAL.NumericalCore.Statistics.Ziggurat.NextU01(ref state);
    public static float ZigguratNextU01Float(ref uint state) => LAL.NumericalCore.Statistics.Ziggurat.NextU01Float(ref state);
    public static double ZigguratNextUniform(ref uint state, double minInclusive, double maxExclusive) => LAL.NumericalCore.Statistics.Ziggurat.NextUniform(ref state, minInclusive, maxExclusive);
    public static float ZigguratNextUniform(ref uint state, float minInclusive, float maxExclusive) => LAL.NumericalCore.Statistics.Ziggurat.NextUniform(ref state, minInclusive, maxExclusive);
    public static double ZigguratNextNormal(ref uint state) => LAL.NumericalCore.Statistics.Ziggurat.NextNormal(ref state);
    public static float ZigguratNextNormalFloat(ref uint state) => LAL.NumericalCore.Statistics.Ziggurat.NextNormalFloat(ref state);
    public static Complex ZigguratNextComplexU01(ref uint state) => LAL.NumericalCore.Statistics.Ziggurat.NextComplexU01(ref state);
    public static Complex ZigguratNextComplexUniform(ref uint state) => LAL.NumericalCore.Statistics.Ziggurat.NextComplexUniform(ref state);
    public static Complex ZigguratNextComplexUniform(ref uint state, double minRealInclusive, double maxRealExclusive, double minImagInclusive, double maxImagExclusive) => LAL.NumericalCore.Statistics.Ziggurat.NextComplexUniform(ref state, minRealInclusive, maxRealExclusive, minImagInclusive, maxImagExclusive);
    public static Complex ZigguratNextComplexNormal(ref uint state) => LAL.NumericalCore.Statistics.Ziggurat.NextComplexNormal(ref state);
}
