using System.Numerics;

namespace LAL.LinalgCore;

internal static class Schur
{
    public static void RealSchur2x2(ReadOnlySpan<double> matrix, Span<double> schur, int iterations = 32)
    {
        if (matrix.Length != 4)
        {
            throw new ArgumentException("Input must be a 2x2 matrix in row-major order.", nameof(matrix));
        }

        if (schur.Length != 4)
        {
            throw new ArgumentException("Output must be a 2x2 matrix in row-major order.", nameof(schur));
        }

        if (iterations < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(iterations), "Iterations must be non-negative.");
        }

        double a = matrix[0];
        double b = matrix[1];
        double c = matrix[2];
        double d = matrix[3];

        double trace = a + d;
        double determinant = (a * d) - (b * c);
        double discriminant = (trace * trace) - (4d * determinant);

        if (discriminant >= 0d)
        {
            double root = Math.Sqrt(discriminant);
            double lambda1 = 0.5 * (trace + root);
            double lambda2 = 0.5 * (trace - root);

            schur[0] = lambda1;
            schur[1] = 0d;
            schur[2] = 0d;
            schur[3] = lambda2;
            return;
        }

        double alpha = 0.5 * trace;
        double beta = 0.5 * Math.Sqrt(-discriminant);

        schur[0] = alpha;
        schur[1] = beta;
        schur[2] = -beta;
        schur[3] = alpha;
    }

    public static void ComplexSchur2x2(ReadOnlySpan<Complex> matrix, Span<Complex> schur, int iterations = 32)
    {
        if (matrix.Length != 4)
        {
            throw new ArgumentException("Input must be a 2x2 matrix in row-major order.", nameof(matrix));
        }

        if (schur.Length != 4)
        {
            throw new ArgumentException("Output must be a 2x2 matrix in row-major order.", nameof(schur));
        }

        if (iterations < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(iterations), "Iterations must be non-negative.");
        }

        Complex a = matrix[0];
        Complex b = matrix[1];
        Complex c = matrix[2];
        Complex d = matrix[3];

        Complex trace = a + d;
        Complex determinant = (a * d) - (b * c);
        Complex discriminant = (trace * trace) - (4d * determinant);
        Complex root = Complex.Sqrt(discriminant);

        Complex lambda1 = 0.5 * (trace + root);
        Complex lambda2 = 0.5 * (trace - root);

        schur[0] = lambda1;
        schur[1] = b;
        schur[2] = Complex.Zero;
        schur[3] = lambda2;
    }
}

