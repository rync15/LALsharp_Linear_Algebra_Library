using System.Numerics;

namespace LAL.LinalgCore;

internal static class DenseSolver
{
    public static bool Solve(ReadOnlySpan<double> matrix, int n, ReadOnlySpan<double> b, Span<double> x, double singularTolerance = 1e-12)
    {
        if (n <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(n), "Matrix size must be positive.");
        }

        if (matrix.Length != n * n)
        {
            throw new ArgumentException("Matrix length must be n*n.", nameof(matrix));
        }

        if (b.Length != n)
        {
            throw new ArgumentException("Right-hand-side length must be n.", nameof(b));
        }

        if (x.Length != n)
        {
            throw new ArgumentException("Solution length must be n.", nameof(x));
        }

        return Lu.FactorAndSolve(matrix, n, b, x, singularTolerance);
    }

    public static bool Solve(ReadOnlySpan<Complex> matrix, int n, ReadOnlySpan<Complex> b, Span<Complex> x, double singularTolerance = 1e-12)
    {
        if (n <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(n), "Matrix size must be positive.");
        }

        if (matrix.Length != n * n)
        {
            throw new ArgumentException("Matrix length must be n*n.", nameof(matrix));
        }

        if (b.Length != n)
        {
            throw new ArgumentException("Right-hand-side length must be n.", nameof(b));
        }

        if (x.Length != n)
        {
            throw new ArgumentException("Solution length must be n.", nameof(x));
        }

        return Lu.FactorAndSolve(matrix, n, b, x, singularTolerance);
    }
}

