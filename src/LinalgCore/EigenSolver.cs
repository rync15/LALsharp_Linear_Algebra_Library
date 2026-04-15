using System.Buffers;
using System.Numerics;

namespace LAL.LinalgCore;

public readonly record struct EigenResult(double Eigenvalue, int Iterations, bool Converged);
public readonly record struct ComplexEigenResult(Complex Eigenvalue, int Iterations, bool Converged);

internal static class EigenSolver
{
    private const int StackallocThreshold = 128;

    public static EigenResult PowerIteration(
        ReadOnlySpan<double> matrix,
        int n,
        Span<double> eigenvector,
        double tolerance = 1e-10,
        int maxIterations = 1_000)
    {
        if (n <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(n), "Matrix size must be positive.");
        }

        if (matrix.Length != n * n)
        {
            throw new ArgumentException("Matrix length must be n*n.", nameof(matrix));
        }

        if (eigenvector.Length != n)
        {
            throw new ArgumentException("Eigenvector length must be n.", nameof(eigenvector));
        }

        if (tolerance <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tolerance), "Tolerance must be positive.");
        }

        if (maxIterations <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxIterations), "Max iterations must be positive.");
        }

        double initial = 1.0 / Math.Sqrt(n);
        for (int i = 0; i < n; i++)
        {
            eigenvector[i] = initial;
        }

        double[]? rented = null;
        Span<double> work = n <= StackallocThreshold
            ? stackalloc double[n]
            : (rented = ArrayPool<double>.Shared.Rent(n)).AsSpan(0, n);
        double lambda = 0d;

        try
        {
            for (int iter = 1; iter <= maxIterations; iter++)
            {
                Multiply(matrix, n, eigenvector, work);

                double norm = Math.Sqrt(DotReal(work, work));
                if (norm <= double.Epsilon)
                {
                    return new EigenResult(0d, iter, false);
                }

                for (int i = 0; i < n; i++)
                {
                    work[i] /= norm;
                }

                double maxDelta = 0d;
                for (int i = 0; i < n; i++)
                {
                    double delta = Math.Abs(work[i] - eigenvector[i]);
                    if (delta > maxDelta)
                    {
                        maxDelta = delta;
                    }

                    eigenvector[i] = work[i];
                }

                Multiply(matrix, n, eigenvector, work);
                lambda = DotReal(eigenvector, work);

                if (maxDelta <= tolerance)
                {
                    return new EigenResult(lambda, iter, true);
                }
            }

            return new EigenResult(lambda, maxIterations, false);
        }
        finally
        {
            if (rented is not null)
            {
                ArrayPool<double>.Shared.Return(rented, clearArray: false);
            }
        }
    }

    public static ComplexEigenResult PowerIteration(
        ReadOnlySpan<Complex> matrix,
        int n,
        Span<Complex> eigenvector,
        double tolerance = 1e-10,
        int maxIterations = 1_000)
    {
        if (n <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(n), "Matrix size must be positive.");
        }

        if (matrix.Length != n * n)
        {
            throw new ArgumentException("Matrix length must be n*n.", nameof(matrix));
        }

        if (eigenvector.Length != n)
        {
            throw new ArgumentException("Eigenvector length must be n.", nameof(eigenvector));
        }

        if (tolerance <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tolerance), "Tolerance must be positive.");
        }

        if (maxIterations <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxIterations), "Max iterations must be positive.");
        }

        double initial = 1.0 / Math.Sqrt(n);
        for (int i = 0; i < n; i++)
        {
            eigenvector[i] = new Complex(initial, 0d);
        }

        Complex[]? rented = null;
        Span<Complex> work = n <= StackallocThreshold
            ? stackalloc Complex[n]
            : (rented = ArrayPool<Complex>.Shared.Rent(n)).AsSpan(0, n);
        Complex lambda = Complex.Zero;

        try
        {
            for (int iter = 1; iter <= maxIterations; iter++)
            {
                Multiply(matrix, n, eigenvector, work);

                double norm = Norms.L2(work);
                if (norm <= double.Epsilon)
                {
                    return new ComplexEigenResult(Complex.Zero, iter, false);
                }

                for (int i = 0; i < n; i++)
                {
                    work[i] /= norm;
                }

                double maxDelta = 0d;
                for (int i = 0; i < n; i++)
                {
                    double delta = Complex.Abs(work[i] - eigenvector[i]);
                    if (delta > maxDelta)
                    {
                        maxDelta = delta;
                    }

                    eigenvector[i] = work[i];
                }

                Multiply(matrix, n, eigenvector, work);
                Complex numerator = Dot.Dotc(eigenvector, work);
                Complex denominator = Dot.Dotc(eigenvector, eigenvector);
                lambda = denominator.Magnitude <= double.Epsilon ? Complex.Zero : numerator / denominator;

                double residualSquared = 0d;
                for (int i = 0; i < n; i++)
                {
                    Complex residual = work[i] - (lambda * eigenvector[i]);
                    double magnitude = residual.Magnitude;
                    residualSquared += magnitude * magnitude;
                }

                double residualNorm = Math.Sqrt(residualSquared);
                if (maxDelta <= tolerance || residualNorm <= tolerance)
                {
                    return new ComplexEigenResult(lambda, iter, true);
                }
            }

            return new ComplexEigenResult(lambda, maxIterations, false);
        }
        finally
        {
            if (rented is not null)
            {
                ArrayPool<Complex>.Shared.Return(rented, clearArray: false);
            }
        }
    }

    private static void Multiply(ReadOnlySpan<double> matrix, int n, ReadOnlySpan<double> vector, Span<double> destination)
    {
        for (int row = 0; row < n; row++)
        {
            double sum = 0d;
            int rowOffset = row * n;
            for (int col = 0; col < n; col++)
            {
                sum += matrix[rowOffset + col] * vector[col];
            }

            destination[row] = sum;
        }
    }

    private static void Multiply(ReadOnlySpan<Complex> matrix, int n, ReadOnlySpan<Complex> vector, Span<Complex> destination)
    {
        for (int row = 0; row < n; row++)
        {
            Complex sum = Complex.Zero;
            int rowOffset = row * n;
            for (int col = 0; col < n; col++)
            {
                sum += matrix[rowOffset + col] * vector[col];
            }

            destination[row] = sum;
        }
    }

    private static double DotReal(ReadOnlySpan<double> left, ReadOnlySpan<double> right)
    {
        double sum = 0d;
        for (int i = 0; i < left.Length; i++)
        {
            sum += left[i] * right[i];
        }

        return sum;
    }
}

