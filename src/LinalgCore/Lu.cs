using System.Buffers;
using System.Numerics;

namespace LAL.LinalgCore;

public readonly record struct LuDecompositionResult(bool Success, int PivotSign);

internal static class Lu
{
    public static LuDecompositionResult DecomposeInPlace(
        Span<double> matrix,
        int n,
        Span<int> pivots,
        double singularTolerance = 1e-12)
    {
        if (n <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(n), "Matrix size must be positive.");
        }

        if (matrix.Length != n * n)
        {
            throw new ArgumentException("Matrix length must be n*n.", nameof(matrix));
        }

        if (pivots.Length != n)
        {
            throw new ArgumentException("Pivot length must be n.", nameof(pivots));
        }

        for (int i = 0; i < n; i++)
        {
            pivots[i] = i;
        }

        int pivotSign = 1;

        for (int k = 0; k < n; k++)
        {
            int pivotRow = k;
            double maxAbs = Math.Abs(matrix[(k * n) + k]);

            for (int r = k + 1; r < n; r++)
            {
                double candidate = Math.Abs(matrix[(r * n) + k]);
                if (candidate > maxAbs)
                {
                    maxAbs = candidate;
                    pivotRow = r;
                }
            }

            if (maxAbs <= singularTolerance)
            {
                return new LuDecompositionResult(false, pivotSign);
            }

            if (pivotRow != k)
            {
                SwapRows(matrix, n, pivotRow, k);
                (pivots[pivotRow], pivots[k]) = (pivots[k], pivots[pivotRow]);
                pivotSign *= -1;
            }

            double diagonal = matrix[(k * n) + k];
            for (int i = k + 1; i < n; i++)
            {
                int ik = (i * n) + k;
                matrix[ik] /= diagonal;
                double factor = matrix[ik];

                for (int j = k + 1; j < n; j++)
                {
                    matrix[(i * n) + j] -= factor * matrix[(k * n) + j];
                }
            }
        }

        return new LuDecompositionResult(true, pivotSign);
    }

    public static LuDecompositionResult DecomposeInPlace(
        Span<Complex> matrix,
        int n,
        Span<int> pivots,
        double singularTolerance = 1e-12)
    {
        if (n <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(n), "Matrix size must be positive.");
        }

        if (matrix.Length != n * n)
        {
            throw new ArgumentException("Matrix length must be n*n.", nameof(matrix));
        }

        if (pivots.Length != n)
        {
            throw new ArgumentException("Pivot length must be n.", nameof(pivots));
        }

        for (int i = 0; i < n; i++)
        {
            pivots[i] = i;
        }

        int pivotSign = 1;

        for (int k = 0; k < n; k++)
        {
            int pivotRow = k;
            double maxAbs = Complex.Abs(matrix[(k * n) + k]);

            for (int r = k + 1; r < n; r++)
            {
                double candidate = Complex.Abs(matrix[(r * n) + k]);
                if (candidate > maxAbs)
                {
                    maxAbs = candidate;
                    pivotRow = r;
                }
            }

            if (maxAbs <= singularTolerance)
            {
                return new LuDecompositionResult(false, pivotSign);
            }

            if (pivotRow != k)
            {
                SwapRows(matrix, n, pivotRow, k);
                (pivots[pivotRow], pivots[k]) = (pivots[k], pivots[pivotRow]);
                pivotSign *= -1;
            }

            Complex diagonal = matrix[(k * n) + k];
            for (int i = k + 1; i < n; i++)
            {
                int ik = (i * n) + k;
                matrix[ik] /= diagonal;
                Complex factor = matrix[ik];

                for (int j = k + 1; j < n; j++)
                {
                    matrix[(i * n) + j] -= factor * matrix[(k * n) + j];
                }
            }
        }

        return new LuDecompositionResult(true, pivotSign);
    }

    public static bool Solve(
        ReadOnlySpan<double> lu,
        int n,
        ReadOnlySpan<int> pivots,
        ReadOnlySpan<double> b,
        Span<double> x,
        double singularTolerance = 1e-12)
    {
        if (n <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(n), "Matrix size must be positive.");
        }

        if (lu.Length != n * n)
        {
            throw new ArgumentException("LU length must be n*n.", nameof(lu));
        }

        if (pivots.Length != n)
        {
            throw new ArgumentException("Pivot length must be n.", nameof(pivots));
        }

        if (b.Length != n)
        {
            throw new ArgumentException("Right-hand-side length must be n.", nameof(b));
        }

        if (x.Length != n)
        {
            throw new ArgumentException("Solution length must be n.", nameof(x));
        }

        for (int i = 0; i < n; i++)
        {
            int sourceIndex = pivots[i];
            x[i] = b[sourceIndex];
        }

        for (int i = 0; i < n; i++)
        {
            double sum = x[i];
            for (int j = 0; j < i; j++)
            {
                sum -= lu[(i * n) + j] * x[j];
            }

            x[i] = sum;
        }

        for (int i = n - 1; i >= 0; i--)
        {
            double diagonal = lu[(i * n) + i];
            if (Math.Abs(diagonal) <= singularTolerance)
            {
                return false;
            }

            double sum = x[i];
            for (int j = i + 1; j < n; j++)
            {
                sum -= lu[(i * n) + j] * x[j];
            }

            x[i] = sum / diagonal;
        }

        return true;
    }

    public static bool Solve(
        ReadOnlySpan<Complex> lu,
        int n,
        ReadOnlySpan<int> pivots,
        ReadOnlySpan<Complex> b,
        Span<Complex> x,
        double singularTolerance = 1e-12)
    {
        if (n <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(n), "Matrix size must be positive.");
        }

        if (lu.Length != n * n)
        {
            throw new ArgumentException("LU length must be n*n.", nameof(lu));
        }

        if (pivots.Length != n)
        {
            throw new ArgumentException("Pivot length must be n.", nameof(pivots));
        }

        if (b.Length != n)
        {
            throw new ArgumentException("Right-hand-side length must be n.", nameof(b));
        }

        if (x.Length != n)
        {
            throw new ArgumentException("Solution length must be n.", nameof(x));
        }

        for (int i = 0; i < n; i++)
        {
            int sourceIndex = pivots[i];
            x[i] = b[sourceIndex];
        }

        for (int i = 0; i < n; i++)
        {
            Complex sum = x[i];
            for (int j = 0; j < i; j++)
            {
                sum -= lu[(i * n) + j] * x[j];
            }

            x[i] = sum;
        }

        for (int i = n - 1; i >= 0; i--)
        {
            Complex diagonal = lu[(i * n) + i];
            if (diagonal.Magnitude <= singularTolerance)
            {
                return false;
            }

            Complex sum = x[i];
            for (int j = i + 1; j < n; j++)
            {
                sum -= lu[(i * n) + j] * x[j];
            }

            x[i] = sum / diagonal;
        }

        return true;
    }

    public static bool FactorAndSolve(
        ReadOnlySpan<double> matrix,
        int n,
        ReadOnlySpan<double> b,
        Span<double> x,
        double singularTolerance = 1e-12)
    {
        if (n <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(n), "Matrix size must be positive.");
        }

        int matrixLength = n * n;
        double[] luBuffer = ArrayPool<double>.Shared.Rent(matrixLength);
        int[]? rentedPivotBuffer = null;
        Span<int> pivots = n <= 128
            ? stackalloc int[n]
            : (rentedPivotBuffer = ArrayPool<int>.Shared.Rent(n)).AsSpan(0, n);

        matrix.CopyTo(luBuffer);

        try
        {
            Span<double> lu = luBuffer.AsSpan(0, matrixLength);
            LuDecompositionResult result = DecomposeInPlace(lu, n, pivots, singularTolerance);
            if (!result.Success)
            {
                return false;
            }

            return Solve(lu, n, pivots, b, x, singularTolerance);
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

    public static bool FactorAndSolve(
        ReadOnlySpan<Complex> matrix,
        int n,
        ReadOnlySpan<Complex> b,
        Span<Complex> x,
        double singularTolerance = 1e-12)
    {
        if (n <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(n), "Matrix size must be positive.");
        }

        int matrixLength = n * n;
        Complex[] luBuffer = ArrayPool<Complex>.Shared.Rent(matrixLength);
        int[]? rentedPivotBuffer = null;
        Span<int> pivots = n <= 128
            ? stackalloc int[n]
            : (rentedPivotBuffer = ArrayPool<int>.Shared.Rent(n)).AsSpan(0, n);

        matrix.CopyTo(luBuffer);

        try
        {
            Span<Complex> lu = luBuffer.AsSpan(0, matrixLength);
            LuDecompositionResult result = DecomposeInPlace(lu, n, pivots, singularTolerance);
            if (!result.Success)
            {
                return false;
            }

            return Solve(lu, n, pivots, b, x, singularTolerance);
        }
        finally
        {
            ArrayPool<Complex>.Shared.Return(luBuffer, clearArray: false);
            if (rentedPivotBuffer is not null)
            {
                ArrayPool<int>.Shared.Return(rentedPivotBuffer, clearArray: false);
            }
        }
    }

    private static void SwapRows(Span<double> matrix, int n, int rowA, int rowB)
    {
        for (int col = 0; col < n; col++)
        {
            int idxA = (rowA * n) + col;
            int idxB = (rowB * n) + col;
            (matrix[idxA], matrix[idxB]) = (matrix[idxB], matrix[idxA]);
        }
    }

    private static void SwapRows(Span<Complex> matrix, int n, int rowA, int rowB)
    {
        for (int col = 0; col < n; col++)
        {
            int idxA = (rowA * n) + col;
            int idxB = (rowB * n) + col;
            (matrix[idxA], matrix[idxB]) = (matrix[idxB], matrix[idxA]);
        }
    }
}

