using System.Buffers;
using LAL.LinalgCore;
using System.Numerics;
using System.Threading.Tasks;

namespace LAL.NumericalCore.Interpolation;

internal static class Rbf
{
    private const int ParallelKernelSizeThreshold = 96;

    public static bool ComputeGaussianWeights(
        ReadOnlySpan<double> centers,
        ReadOnlySpan<double> values,
        double epsilon,
        Span<double> weights,
        double singularTolerance = 1e-12,
        bool allowParallel = false)
    {
        if (centers.Length == 0)
        {
            throw new ArgumentException("Centers must not be empty.", nameof(centers));
        }

        if (centers.Length != values.Length || centers.Length != weights.Length)
        {
            throw new ArgumentException("Centers, values and weights lengths must match.");
        }

        if (epsilon <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(epsilon), "Epsilon must be positive.");
        }

        int n = centers.Length;
        int kernelLength = n * n;
        double[] kernelBuffer = ArrayPool<double>.Shared.Rent(kernelLength);
        int[]? rentedPivotBuffer = null;
        Span<int> pivots = n <= 128
            ? stackalloc int[n]
            : (rentedPivotBuffer = ArrayPool<int>.Shared.Rent(n)).AsSpan(0, n);

        try
        {
            FillGaussianKernel(centers, epsilon, kernelBuffer, n, allowParallel);

            Span<double> kernel = kernelBuffer.AsSpan(0, kernelLength);
            LuDecompositionResult decompose = Lu.DecomposeInPlace(kernel, n, pivots, singularTolerance);
            if (!decompose.Success)
            {
                return false;
            }

            return Lu.Solve(kernel, n, pivots, values, weights, singularTolerance);
        }
        finally
        {
            ArrayPool<double>.Shared.Return(kernelBuffer, clearArray: false);
            if (rentedPivotBuffer is not null)
            {
                ArrayPool<int>.Shared.Return(rentedPivotBuffer, clearArray: false);
            }
        }
    }

    public static double EvaluateGaussian(ReadOnlySpan<double> centers, ReadOnlySpan<double> weights, double x, double epsilon)
    {
        if (centers.Length == 0)
        {
            throw new ArgumentException("Centers must not be empty.", nameof(centers));
        }

        if (centers.Length != weights.Length)
        {
            throw new ArgumentException("Centers and weights lengths must match.");
        }

        if (epsilon <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(epsilon), "Epsilon must be positive.");
        }

        double sum = 0d;
        for (int i = 0; i < centers.Length; i++)
        {
            double distance = x - centers[i];
            double scaled = epsilon * distance;
            sum = Math.FusedMultiplyAdd(weights[i], Math.Exp(-(scaled * scaled)), sum);
        }

        return sum;
    }

    public static bool ComputeGaussianWeights(
        ReadOnlySpan<float> centers,
        ReadOnlySpan<float> values,
        float epsilon,
        Span<float> weights,
        float singularTolerance = 1e-6f,
        bool allowParallel = false)
    {
        if (centers.Length == 0)
        {
            throw new ArgumentException("Centers must not be empty.", nameof(centers));
        }

        if (centers.Length != values.Length || centers.Length != weights.Length)
        {
            throw new ArgumentException("Centers, values and weights lengths must match.");
        }

        if (epsilon <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(epsilon), "Epsilon must be positive.");
        }

        int n = centers.Length;
        double[]? rentedCenters = null;
        double[]? rentedValues = null;
        double[]? rentedWeights = null;

        Span<double> centers64 = n <= 128
            ? stackalloc double[n]
            : (rentedCenters = ArrayPool<double>.Shared.Rent(n)).AsSpan(0, n);
        Span<double> values64 = n <= 128
            ? stackalloc double[n]
            : (rentedValues = ArrayPool<double>.Shared.Rent(n)).AsSpan(0, n);
        Span<double> weights64 = n <= 128
            ? stackalloc double[n]
            : (rentedWeights = ArrayPool<double>.Shared.Rent(n)).AsSpan(0, n);

        try
        {
            for (int i = 0; i < n; i++)
            {
                centers64[i] = centers[i];
                values64[i] = values[i];
            }

            bool ok = ComputeGaussianWeights(centers64, values64, epsilon, weights64, singularTolerance, allowParallel);
            if (!ok)
            {
                return false;
            }

            for (int i = 0; i < n; i++)
            {
                weights[i] = (float)weights64[i];
            }

            return true;
        }
        finally
        {
            if (rentedCenters is not null)
            {
                ArrayPool<double>.Shared.Return(rentedCenters, clearArray: false);
            }

            if (rentedValues is not null)
            {
                ArrayPool<double>.Shared.Return(rentedValues, clearArray: false);
            }

            if (rentedWeights is not null)
            {
                ArrayPool<double>.Shared.Return(rentedWeights, clearArray: false);
            }
        }
    }

    public static float EvaluateGaussian(ReadOnlySpan<float> centers, ReadOnlySpan<float> weights, float x, float epsilon)
    {
        if (centers.Length == 0)
        {
            throw new ArgumentException("Centers must not be empty.", nameof(centers));
        }

        if (centers.Length != weights.Length)
        {
            throw new ArgumentException("Centers and weights lengths must match.");
        }

        if (epsilon <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(epsilon), "Epsilon must be positive.");
        }

        float sum = 0f;
        for (int i = 0; i < centers.Length; i++)
        {
            float distance = x - centers[i];
            float scaled = epsilon * distance;
            sum = MathF.FusedMultiplyAdd(weights[i], MathF.Exp(-(scaled * scaled)), sum);
        }

        return sum;
    }

    public static bool ComputeGaussianWeights(
        ReadOnlySpan<double> centers,
        ReadOnlySpan<Complex> values,
        double epsilon,
        Span<Complex> weights,
        double singularTolerance = 1e-12,
        bool allowParallel = false)
    {
        if (centers.Length == 0)
        {
            throw new ArgumentException("Centers must not be empty.", nameof(centers));
        }

        if (centers.Length != values.Length || centers.Length != weights.Length)
        {
            throw new ArgumentException("Centers, values and weights lengths must match.");
        }

        if (epsilon <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(epsilon), "Epsilon must be positive.");
        }

        int n = centers.Length;
        int kernelLength = n * n;
        Complex[] kernelBuffer = ArrayPool<Complex>.Shared.Rent(kernelLength);

        try
        {
            FillGaussianKernel(centers, epsilon, kernelBuffer, n, allowParallel);
            return SolveLinearSystemComplex(kernelBuffer.AsSpan(0, kernelLength), n, values, weights, singularTolerance);
        }
        finally
        {
            ArrayPool<Complex>.Shared.Return(kernelBuffer, clearArray: false);
        }
    }

    public static Complex EvaluateGaussian(ReadOnlySpan<double> centers, ReadOnlySpan<Complex> weights, double x, double epsilon)
    {
        if (centers.Length == 0)
        {
            throw new ArgumentException("Centers must not be empty.", nameof(centers));
        }

        if (centers.Length != weights.Length)
        {
            throw new ArgumentException("Centers and weights lengths must match.");
        }

        if (epsilon <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(epsilon), "Epsilon must be positive.");
        }

        Complex sum = Complex.Zero;
        for (int i = 0; i < centers.Length; i++)
        {
            double distance = x - centers[i];
            double scaled = epsilon * distance;
            sum += weights[i] * Math.Exp(-(scaled * scaled));
        }

        return sum;
    }

    private static bool SolveLinearSystemComplex(
        Span<Complex> matrix,
        int n,
        ReadOnlySpan<Complex> rhs,
        Span<Complex> solution,
        double singularTolerance)
    {
        Complex[]? rentedB = null;
        Span<Complex> b = n <= 128
            ? stackalloc Complex[n]
            : (rentedB = ArrayPool<Complex>.Shared.Rent(n)).AsSpan(0, n);

        rhs.CopyTo(b);

        try
        {
            for (int col = 0; col < n; col++)
            {
                int pivot = col;
                double pivotMag = Complex.Abs(matrix[(col * n) + col]);

                for (int row = col + 1; row < n; row++)
                {
                    double mag = Complex.Abs(matrix[(row * n) + col]);
                    if (mag > pivotMag)
                    {
                        pivot = row;
                        pivotMag = mag;
                    }
                }

                if (pivotMag <= singularTolerance)
                {
                    return false;
                }

                if (pivot != col)
                {
                    SwapRows(matrix, n, pivot, col);
                    (b[pivot], b[col]) = (b[col], b[pivot]);
                }

                Complex diag = matrix[(col * n) + col];

                for (int row = col + 1; row < n; row++)
                {
                    Complex factor = matrix[(row * n) + col] / diag;
                    if (factor == Complex.Zero)
                    {
                        continue;
                    }

                    matrix[(row * n) + col] = Complex.Zero;
                    for (int c = col + 1; c < n; c++)
                    {
                        matrix[(row * n) + c] -= factor * matrix[(col * n) + c];
                    }

                    b[row] -= factor * b[col];
                }
            }

            for (int row = n - 1; row >= 0; row--)
            {
                Complex sum = b[row];
                int rowOffset = row * n;
                for (int col = row + 1; col < n; col++)
                {
                    sum -= matrix[rowOffset + col] * solution[col];
                }

                Complex diag = matrix[rowOffset + row];
                if (Complex.Abs(diag) <= singularTolerance)
                {
                    return false;
                }

                solution[row] = sum / diag;
            }

            return true;
        }
        finally
        {
            if (rentedB is not null)
            {
                ArrayPool<Complex>.Shared.Return(rentedB, clearArray: false);
            }
        }
    }

    private static void FillGaussianKernel(ReadOnlySpan<double> centers, double epsilon, double[] kernel, int n, bool allowParallel)
    {
        if (ShouldParallelizeKernel(n, allowParallel))
        {
            double[] centersBuffer = ArrayPool<double>.Shared.Rent(n);
            centers.CopyTo(centersBuffer);

            try
            {
                int workerCap = Math.Max(1, Math.Min(Environment.ProcessorCount, n));
                ParallelOptions options = new() { MaxDegreeOfParallelism = workerCap };
                Parallel.For(0, n, options, i =>
                {
                    int rowOffset = i * n;
                    double ci = centersBuffer[i];
                    for (int j = 0; j < n; j++)
                    {
                        double distance = ci - centersBuffer[j];
                        double scaled = epsilon * distance;
                        kernel[rowOffset + j] = Math.Exp(-(scaled * scaled));
                    }
                });
            }
            finally
            {
                ArrayPool<double>.Shared.Return(centersBuffer, clearArray: false);
            }

            return;
        }

        for (int i = 0; i < n; i++)
        {
            int rowOffset = i * n;
            double ci = centers[i];
            for (int j = 0; j < n; j++)
            {
                double distance = ci - centers[j];
                double scaled = epsilon * distance;
                kernel[rowOffset + j] = Math.Exp(-(scaled * scaled));
            }
        }
    }

    private static void FillGaussianKernel(ReadOnlySpan<double> centers, double epsilon, Complex[] kernel, int n, bool allowParallel)
    {
        if (ShouldParallelizeKernel(n, allowParallel))
        {
            double[] centersBuffer = ArrayPool<double>.Shared.Rent(n);
            centers.CopyTo(centersBuffer);

            try
            {
                int workerCap = Math.Max(1, Math.Min(Environment.ProcessorCount, n));
                ParallelOptions options = new() { MaxDegreeOfParallelism = workerCap };
                Parallel.For(0, n, options, i =>
                {
                    int rowOffset = i * n;
                    double ci = centersBuffer[i];
                    for (int j = 0; j < n; j++)
                    {
                        double distance = ci - centersBuffer[j];
                        double scaled = epsilon * distance;
                        kernel[rowOffset + j] = Math.Exp(-(scaled * scaled));
                    }
                });
            }
            finally
            {
                ArrayPool<double>.Shared.Return(centersBuffer, clearArray: false);
            }

            return;
        }

        for (int i = 0; i < n; i++)
        {
            int rowOffset = i * n;
            double ci = centers[i];
            for (int j = 0; j < n; j++)
            {
                double distance = ci - centers[j];
                double scaled = epsilon * distance;
                kernel[rowOffset + j] = Math.Exp(-(scaled * scaled));
            }
        }
    }

    private static bool ShouldParallelizeKernel(int n, bool allowParallel)
    {
        return allowParallel && n >= ParallelKernelSizeThreshold;
    }

    private static void SwapRows(Span<Complex> matrix, int n, int rowA, int rowB)
    {
        int offsetA = rowA * n;
        int offsetB = rowB * n;

        for (int col = 0; col < n; col++)
        {
            (matrix[offsetA + col], matrix[offsetB + col]) = (matrix[offsetB + col], matrix[offsetA + col]);
        }
    }
}

