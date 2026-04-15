using System.Buffers;
using System.Numerics;

namespace LAL.LinalgCore;

internal static class MatrixAnalysis
{
    private const int StackallocThreshold = 64;
    private const int StackallocPivotThreshold = 128;

    public static double Determinant(ReadOnlySpan<double> matrix, int n, double singularTolerance = 1e-12)
    {
        if (n <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(n), "Matrix size must be positive.");
        }

        if (matrix.Length != n * n)
        {
            throw new ArgumentException("Matrix length must be n*n.", nameof(matrix));
        }

        int matrixLength = n * n;
        double[] luBuffer = ArrayPool<double>.Shared.Rent(matrixLength);
        int[]? rentedPivotBuffer = null;
        Span<int> pivots = n <= StackallocPivotThreshold
            ? stackalloc int[n]
            : (rentedPivotBuffer = ArrayPool<int>.Shared.Rent(n)).AsSpan(0, n);

        matrix.CopyTo(luBuffer);

        try
        {
            Span<double> lu = luBuffer.AsSpan(0, matrixLength);
            LuDecompositionResult factor = Lu.DecomposeInPlace(lu, n, pivots, singularTolerance);
            if (!factor.Success)
            {
                return 0d;
            }

            double det = factor.PivotSign;
            for (int i = 0; i < n; i++)
            {
                det *= lu[(i * n) + i];
            }

            return det;
        }
        finally
        {
            ArrayPool<double>.Shared.Return(luBuffer, clearArray: false);
            if (rentedPivotBuffer is not null)
            {
                ArrayPool<int>.Shared.Return(rentedPivotBuffer, clearArray: false);
            }
        }
    }

    public static bool Inverse(ReadOnlySpan<double> matrix, int n, Span<double> inverse, double singularTolerance = 1e-12)
    {
        if (n <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(n), "Matrix size must be positive.");
        }

        if (matrix.Length != n * n)
        {
            throw new ArgumentException("Matrix length must be n*n.", nameof(matrix));
        }

        if (inverse.Length != n * n)
        {
            throw new ArgumentException("Inverse output length must be n*n.", nameof(inverse));
        }

        int matrixLength = n * n;
        double[] luBuffer = ArrayPool<double>.Shared.Rent(matrixLength);
        int[]? rentedPivotBuffer = null;
        Span<int> pivots = n <= StackallocPivotThreshold
            ? stackalloc int[n]
            : (rentedPivotBuffer = ArrayPool<int>.Shared.Rent(n)).AsSpan(0, n);

        double[]? rentedWorkspace = null;
        Span<double> workspace = n <= StackallocThreshold
            ? stackalloc double[n * 2]
            : (rentedWorkspace = ArrayPool<double>.Shared.Rent(n * 2)).AsSpan(0, n * 2);
        Span<double> e = workspace.Slice(0, n);
        Span<double> column = workspace.Slice(n, n);

        matrix.CopyTo(luBuffer);

        try
        {
            Span<double> lu = luBuffer.AsSpan(0, matrixLength);
            LuDecompositionResult factor = Lu.DecomposeInPlace(lu, n, pivots, singularTolerance);
            if (!factor.Success)
            {
                return false;
            }

            for (int col = 0; col < n; col++)
            {
                e.Clear();
                e[col] = 1d;

                if (!Lu.Solve(lu, n, pivots, e, column, singularTolerance))
                {
                    return false;
                }

                for (int row = 0; row < n; row++)
                {
                    inverse[(row * n) + col] = column[row];
                }
            }

            return true;
        }
        finally
        {
            ArrayPool<double>.Shared.Return(luBuffer, clearArray: false);
            if (rentedPivotBuffer is not null)
            {
                ArrayPool<int>.Shared.Return(rentedPivotBuffer, clearArray: false);
            }

            if (rentedWorkspace is not null)
            {
                ArrayPool<double>.Shared.Return(rentedWorkspace, clearArray: false);
            }
        }
    }

    public static double Trace(ReadOnlySpan<double> matrix, int n)
    {
        if (n <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(n), "Matrix size must be positive.");
        }

        if (matrix.Length != n * n)
        {
            throw new ArgumentException("Matrix length must be n*n.", nameof(matrix));
        }

        double trace = 0d;
        for (int i = 0; i < n; i++)
        {
            trace += matrix[(i * n) + i];
        }

        return trace;
    }

    public static int Rank(ReadOnlySpan<double> matrix, int rows, int cols, double tolerance = 1e-10)
    {
        if (rows <= 0 || cols <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(rows), "Rows and cols must be positive.");
        }

        if (matrix.Length != rows * cols)
        {
            throw new ArgumentException("Matrix length must be rows*cols.", nameof(matrix));
        }

        if (tolerance <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tolerance), "Tolerance must be positive.");
        }

        int matrixLength = rows * cols;
        double[] workBuffer = ArrayPool<double>.Shared.Rent(matrixLength);
        matrix.CopyTo(workBuffer);
        Span<double> work = workBuffer.AsSpan(0, matrixLength);
        int rank = 0;
        int pivotRow = 0;

        try
        {
            for (int col = 0; col < cols && pivotRow < rows; col++)
            {
                int bestRow = pivotRow;
                double maxAbs = Math.Abs(work[(pivotRow * cols) + col]);
                for (int row = pivotRow + 1; row < rows; row++)
                {
                    double value = Math.Abs(work[(row * cols) + col]);
                    if (value > maxAbs)
                    {
                        maxAbs = value;
                        bestRow = row;
                    }
                }

                if (maxAbs <= tolerance)
                {
                    continue;
                }

                if (bestRow != pivotRow)
                {
                    SwapRows(work, cols, bestRow, pivotRow);
                }

                double pivot = work[(pivotRow * cols) + col];
                for (int row = pivotRow + 1; row < rows; row++)
                {
                    double factor = work[(row * cols) + col] / pivot;
                    if (Math.Abs(factor) <= double.Epsilon)
                    {
                        continue;
                    }

                    int rowOffset = row * cols;
                    int pivotOffset = pivotRow * cols;
                    for (int c = col; c < cols; c++)
                    {
                        work[rowOffset + c] -= factor * work[pivotOffset + c];
                    }
                }

                rank++;
                pivotRow++;
            }

            return rank;
        }
        finally
        {
            ArrayPool<double>.Shared.Return(workBuffer, clearArray: false);
        }
    }

    public static bool PseudoInverseSquare(ReadOnlySpan<double> matrix, int n, Span<double> pseudoInverse, double singularTolerance = 1e-12)
    {
        return Inverse(matrix, n, pseudoInverse, singularTolerance);
    }

    public static bool PseudoInverse(
        ReadOnlySpan<double> matrix,
        int rows,
        int cols,
        Span<double> pseudoInverse,
        double singularTolerance = 1e-12)
    {
        if (rows <= 0 || cols <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(rows), "Rows and cols must be positive.");
        }

        if (matrix.Length != rows * cols)
        {
            throw new ArgumentException("Matrix length must be rows*cols.", nameof(matrix));
        }

        if (pseudoInverse.Length != cols * rows)
        {
            throw new ArgumentException("Pseudo-inverse length must be cols*rows.", nameof(pseudoInverse));
        }

        if (rows == cols)
        {
            return Inverse(matrix, rows, pseudoInverse, singularTolerance);
        }

        if (rows > cols)
        {
            int square = cols * cols;
            double[] ataBuffer = ArrayPool<double>.Shared.Rent(square);
            double[] invAtaBuffer = ArrayPool<double>.Shared.Rent(square);
            Span<double> ata = ataBuffer.AsSpan(0, square);
            Span<double> invAta = invAtaBuffer.AsSpan(0, square);

            try
            {
                for (int r = 0; r < cols; r++)
                {
                    for (int c = 0; c < cols; c++)
                    {
                        double sum = 0d;
                        for (int k = 0; k < rows; k++)
                        {
                            sum += matrix[(k * cols) + r] * matrix[(k * cols) + c];
                        }

                        ata[(r * cols) + c] = sum;
                    }
                }

                if (!Inverse(ata, cols, invAta, singularTolerance))
                {
                    return false;
                }

                for (int r = 0; r < cols; r++)
                {
                    for (int c = 0; c < rows; c++)
                    {
                        double sum = 0d;
                        for (int k = 0; k < cols; k++)
                        {
                            sum += invAta[(r * cols) + k] * matrix[(c * cols) + k];
                        }

                        pseudoInverse[(r * rows) + c] = sum;
                    }
                }

                return true;
            }
            finally
            {
                ArrayPool<double>.Shared.Return(ataBuffer, clearArray: false);
                ArrayPool<double>.Shared.Return(invAtaBuffer, clearArray: false);
            }
        }

        int squareRows = rows * rows;
        double[] aatBuffer = ArrayPool<double>.Shared.Rent(squareRows);
        double[] invAatBuffer = ArrayPool<double>.Shared.Rent(squareRows);
        Span<double> aat = aatBuffer.AsSpan(0, squareRows);
        Span<double> invAat = invAatBuffer.AsSpan(0, squareRows);

        try
        {
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < rows; c++)
                {
                    double sum = 0d;
                    for (int k = 0; k < cols; k++)
                    {
                        sum += matrix[(r * cols) + k] * matrix[(c * cols) + k];
                    }

                    aat[(r * rows) + c] = sum;
                }
            }

            if (!Inverse(aat, rows, invAat, singularTolerance))
            {
                return false;
            }

            for (int r = 0; r < cols; r++)
            {
                for (int c = 0; c < rows; c++)
                {
                    double sum = 0d;
                    for (int k = 0; k < rows; k++)
                    {
                        sum += matrix[(k * cols) + r] * invAat[(k * rows) + c];
                    }

                    pseudoInverse[(r * rows) + c] = sum;
                }
            }

            return true;
        }
        finally
        {
            ArrayPool<double>.Shared.Return(aatBuffer, clearArray: false);
            ArrayPool<double>.Shared.Return(invAatBuffer, clearArray: false);
        }
    }

    public static EigenResult DominantEigenvalue(
        ReadOnlySpan<double> matrix,
        int n,
        Span<double> eigenvector,
        double tolerance = 1e-10,
        int maxIterations = 1_000)
    {
        return EigenSolver.PowerIteration(matrix, n, eigenvector, tolerance, maxIterations);
    }

    public static void Eigenvalues2x2(ReadOnlySpan<double> matrix, Span<Complex> eigenvalues)
    {
        if (matrix.Length != 4)
        {
            throw new ArgumentException("Matrix length must be 4 for a 2x2 matrix.", nameof(matrix));
        }

        if (eigenvalues.Length < 2)
        {
            throw new ArgumentException("Eigenvalue output must have at least two slots.", nameof(eigenvalues));
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
            eigenvalues[0] = new Complex((trace + root) * 0.5d, 0d);
            eigenvalues[1] = new Complex((trace - root) * 0.5d, 0d);
            return;
        }

        double imag = Math.Sqrt(-discriminant) * 0.5d;
        double real = trace * 0.5d;
        eigenvalues[0] = new Complex(real, imag);
        eigenvalues[1] = new Complex(real, -imag);
    }

    private static void SwapRows(Span<double> matrix, int cols, int rowA, int rowB)
    {
        for (int col = 0; col < cols; col++)
        {
            int idxA = (rowA * cols) + col;
            int idxB = (rowB * cols) + col;
            (matrix[idxA], matrix[idxB]) = (matrix[idxB], matrix[idxA]);
        }
    }
}

