using System.Numerics;

namespace LAL.LinalgCore;

internal static class Qr
{
    public static bool DecomposeThin(
        ReadOnlySpan<double> matrix,
        int rows,
        int cols,
        Span<double> q,
        Span<double> r,
        double tolerance = 1e-12)
    {
        if (rows <= 0 || cols <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(rows), "Rows and cols must be positive.");
        }

        if (matrix.Length != rows * cols)
        {
            throw new ArgumentException("Matrix length must be rows*cols.", nameof(matrix));
        }

        if (q.Length != rows * cols)
        {
            throw new ArgumentException("Q length must be rows*cols.", nameof(q));
        }

        if (r.Length != cols * cols)
        {
            throw new ArgumentException("R length must be cols*cols.", nameof(r));
        }

        if (tolerance <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tolerance), "Tolerance must be positive.");
        }

        matrix.CopyTo(q);
        r.Clear();

        for (int k = 0; k < cols; k++)
        {
            double normSquared = 0d;
            for (int i = 0; i < rows; i++)
            {
                double value = q[(i * cols) + k];
                normSquared += value * value;
            }

            double norm = Math.Sqrt(normSquared);
            if (norm <= tolerance)
            {
                return false;
            }

            r[(k * cols) + k] = norm;
            for (int i = 0; i < rows; i++)
            {
                q[(i * cols) + k] /= norm;
            }

            for (int j = k + 1; j < cols; j++)
            {
                double dot = 0d;
                for (int i = 0; i < rows; i++)
                {
                    dot += q[(i * cols) + k] * q[(i * cols) + j];
                }

                r[(k * cols) + j] = dot;

                for (int i = 0; i < rows; i++)
                {
                    q[(i * cols) + j] -= dot * q[(i * cols) + k];
                }
            }
        }

        return true;
    }

    public static bool DecomposeThin(
        ReadOnlySpan<Complex> matrix,
        int rows,
        int cols,
        Span<Complex> q,
        Span<Complex> r,
        double tolerance = 1e-12)
    {
        if (rows <= 0 || cols <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(rows), "Rows and cols must be positive.");
        }

        if (matrix.Length != rows * cols)
        {
            throw new ArgumentException("Matrix length must be rows*cols.", nameof(matrix));
        }

        if (q.Length != rows * cols)
        {
            throw new ArgumentException("Q length must be rows*cols.", nameof(q));
        }

        if (r.Length != cols * cols)
        {
            throw new ArgumentException("R length must be cols*cols.", nameof(r));
        }

        if (tolerance <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tolerance), "Tolerance must be positive.");
        }

        matrix.CopyTo(q);
        r.Clear();

        for (int k = 0; k < cols; k++)
        {
            double normSquared = 0d;
            for (int i = 0; i < rows; i++)
            {
                Complex value = q[(i * cols) + k];
                normSquared += value.Magnitude * value.Magnitude;
            }

            double norm = Math.Sqrt(normSquared);
            if (norm <= tolerance)
            {
                return false;
            }

            r[(k * cols) + k] = new Complex(norm, 0d);
            for (int i = 0; i < rows; i++)
            {
                q[(i * cols) + k] /= norm;
            }

            for (int j = k + 1; j < cols; j++)
            {
                Complex dot = Complex.Zero;
                for (int i = 0; i < rows; i++)
                {
                    dot += Complex.Conjugate(q[(i * cols) + k]) * q[(i * cols) + j];
                }

                r[(k * cols) + j] = dot;

                for (int i = 0; i < rows; i++)
                {
                    q[(i * cols) + j] -= dot * q[(i * cols) + k];
                }
            }
        }

        return true;
    }
}

