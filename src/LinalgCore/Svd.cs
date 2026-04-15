using System.Numerics;

namespace LAL.LinalgCore;

internal static class Svd
{
    public static void SingularValues(
        ReadOnlySpan<double> matrix,
        int rows,
        int cols,
        Span<double> singularValues,
        int maxSweeps = 32)
    {
        if (rows <= 0 || cols <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(rows), "Rows and cols must be positive.");
        }

        if (matrix.Length != rows * cols)
        {
            throw new ArgumentException("Matrix length must be rows*cols.", nameof(matrix));
        }

        int outputCount = Math.Min(rows, cols);
        if (singularValues.Length != outputCount)
        {
            throw new ArgumentException("Singular value output length must be min(rows, cols).", nameof(singularValues));
        }

        if (maxSweeps <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxSweeps), "Max sweeps must be positive.");
        }

        double[] gram = new double[cols * cols];
        for (int row = 0; row < rows; row++)
        {
            int rowOffset = row * cols;
            for (int i = 0; i < cols; i++)
            {
                double left = matrix[rowOffset + i];
                int gramOffset = i * cols;
                for (int j = 0; j < cols; j++)
                {
                    gram[gramOffset + j] += left * matrix[rowOffset + j];
                }
            }
        }

        double[] eigenvalues = new double[cols];
        ComputeEigenvaluesSymmetricJacobi(gram, cols, eigenvalues, maxSweeps);

        Array.Sort(eigenvalues);
        for (int i = 0; i < outputCount; i++)
        {
            int source = cols - 1 - i;
            singularValues[i] = Math.Sqrt(Math.Max(0d, eigenvalues[source]));
        }
    }

    public static void SingularValues(
        ReadOnlySpan<Complex> matrix,
        int rows,
        int cols,
        Span<double> singularValues,
        int maxSweeps = 32)
    {
        if (rows <= 0 || cols <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(rows), "Rows and cols must be positive.");
        }

        if (matrix.Length != rows * cols)
        {
            throw new ArgumentException("Matrix length must be rows*cols.", nameof(matrix));
        }

        int outputCount = Math.Min(rows, cols);
        if (singularValues.Length != outputCount)
        {
            throw new ArgumentException("Singular value output length must be min(rows, cols).", nameof(singularValues));
        }

        if (maxSweeps <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxSweeps), "Max sweeps must be positive.");
        }

        int realRows = checked(rows * 2);
        int realCols = checked(cols * 2);
        double[] embedded = new double[realRows * realCols];

        for (int row = 0; row < rows; row++)
        {
            int topOffset = row * realCols;
            int bottomOffset = (row + rows) * realCols;
            int rowOffset = row * cols;

            for (int col = 0; col < cols; col++)
            {
                Complex value = matrix[rowOffset + col];
                int left = col;
                int right = col + cols;

                embedded[topOffset + left] = value.Real;
                embedded[topOffset + right] = -value.Imaginary;
                embedded[bottomOffset + left] = value.Imaginary;
                embedded[bottomOffset + right] = value.Real;
            }
        }

        double[] embeddedSingular = new double[Math.Min(realRows, realCols)];
        SingularValues(embedded, realRows, realCols, embeddedSingular, maxSweeps);

        for (int i = 0; i < outputCount; i++)
        {
            singularValues[i] = embeddedSingular[i * 2];
        }
    }

    private static void ComputeEigenvaluesSymmetricJacobi(double[] matrix, int n, double[] eigenvalues, int maxSweeps)
    {
        for (int sweep = 0; sweep < maxSweeps * n; sweep++)
        {
            int p = 0;
            int q = 1;
            double maxOffDiagonal = 0d;

            for (int i = 0; i < n; i++)
            {
                for (int j = i + 1; j < n; j++)
                {
                    double absValue = Math.Abs(matrix[(i * n) + j]);
                    if (absValue > maxOffDiagonal)
                    {
                        maxOffDiagonal = absValue;
                        p = i;
                        q = j;
                    }
                }
            }

            if (maxOffDiagonal <= 1e-12)
            {
                break;
            }

            double app = matrix[(p * n) + p];
            double aqq = matrix[(q * n) + q];
            double apq = matrix[(p * n) + q];

            double phi = 0.5 * Math.Atan2(2d * apq, aqq - app);
            double c = Math.Cos(phi);
            double s = Math.Sin(phi);

            for (int k = 0; k < n; k++)
            {
                double mkp = matrix[(k * n) + p];
                double mkq = matrix[(k * n) + q];
                matrix[(k * n) + p] = (c * mkp) - (s * mkq);
                matrix[(k * n) + q] = (s * mkp) + (c * mkq);
            }

            for (int k = 0; k < n; k++)
            {
                double mpk = matrix[(p * n) + k];
                double mqk = matrix[(q * n) + k];
                matrix[(p * n) + k] = (c * mpk) - (s * mqk);
                matrix[(q * n) + k] = (s * mpk) + (c * mqk);
            }

            matrix[(p * n) + q] = 0d;
            matrix[(q * n) + p] = 0d;
        }

        for (int i = 0; i < n; i++)
        {
            eigenvalues[i] = matrix[(i * n) + i];
        }
    }
}

