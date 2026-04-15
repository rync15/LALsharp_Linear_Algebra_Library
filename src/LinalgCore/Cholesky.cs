using System.Numerics;

namespace LAL.LinalgCore;

internal static class Cholesky
{
    public static bool DecomposeLower(ReadOnlySpan<double> matrix, int n, Span<double> lower, double positiveTolerance = 1e-12)
    {
        if (n <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(n), "Matrix size must be positive.");
        }

        if (matrix.Length != n * n)
        {
            throw new ArgumentException("Matrix length must be n*n.", nameof(matrix));
        }

        if (lower.Length != n * n)
        {
            throw new ArgumentException("Lower length must be n*n.", nameof(lower));
        }

        lower.Clear();

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j <= i; j++)
            {
                double sum = matrix[(i * n) + j];
                for (int k = 0; k < j; k++)
                {
                    sum -= lower[(i * n) + k] * lower[(j * n) + k];
                }

                if (i == j)
                {
                    if (sum <= positiveTolerance)
                    {
                        return false;
                    }

                    lower[(i * n) + i] = Math.Sqrt(sum);
                }
                else
                {
                    double diagonal = lower[(j * n) + j];
                    if (Math.Abs(diagonal) <= positiveTolerance)
                    {
                        return false;
                    }

                    lower[(i * n) + j] = sum / diagonal;
                }
            }
        }

        return true;
    }

    public static bool DecomposeLower(ReadOnlySpan<Complex> matrix, int n, Span<Complex> lower, double positiveTolerance = 1e-12)
    {
        if (n <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(n), "Matrix size must be positive.");
        }

        if (matrix.Length != n * n)
        {
            throw new ArgumentException("Matrix length must be n*n.", nameof(matrix));
        }

        if (lower.Length != n * n)
        {
            throw new ArgumentException("Lower length must be n*n.", nameof(lower));
        }

        lower.Clear();

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j <= i; j++)
            {
                Complex sum = matrix[(i * n) + j];
                for (int k = 0; k < j; k++)
                {
                    sum -= lower[(i * n) + k] * Complex.Conjugate(lower[(j * n) + k]);
                }

                if (i == j)
                {
                    if (Math.Abs(sum.Imaginary) > positiveTolerance)
                    {
                        return false;
                    }

                    double real = sum.Real;
                    if (real <= positiveTolerance)
                    {
                        return false;
                    }

                    lower[(i * n) + i] = new Complex(Math.Sqrt(real), 0d);
                }
                else
                {
                    Complex diagonal = lower[(j * n) + j];
                    if (diagonal.Magnitude <= positiveTolerance)
                    {
                        return false;
                    }

                    lower[(i * n) + j] = sum / diagonal;
                }
            }
        }

        return true;
    }
}

