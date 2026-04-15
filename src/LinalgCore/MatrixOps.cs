using System.Numerics;

namespace LAL.LinalgCore;

internal static class MatrixOps
{
    public static void Add(ReadOnlySpan<double> left, ReadOnlySpan<double> right, Span<double> destination, int rows, int cols)
    {
        ValidateBinary(left, right, destination, rows, cols);

        for (int i = 0; i < destination.Length; i++)
        {
            destination[i] = left[i] + right[i];
        }
    }

    public static void Subtract(ReadOnlySpan<double> left, ReadOnlySpan<double> right, Span<double> destination, int rows, int cols)
    {
        ValidateBinary(left, right, destination, rows, cols);

        for (int i = 0; i < destination.Length; i++)
        {
            destination[i] = left[i] - right[i];
        }
    }

    public static void Multiply(ReadOnlySpan<double> matrix, int rows, int cols, ReadOnlySpan<double> vector, Span<double> destination)
    {
        Gemv.Multiply(matrix, rows, cols, vector, destination);
    }

    public static void Multiply(
        ReadOnlySpan<double> left,
        int leftRows,
        int sharedDim,
        ReadOnlySpan<double> right,
        int rightCols,
        Span<double> destination)
    {
        Gemm.Multiply(left, right, destination, leftRows, rightCols, sharedDim, alpha: 1d, beta: 0d);
    }

    public static double Dot(ReadOnlySpan<double> left, ReadOnlySpan<double> right, int rows, int cols)
    {
        if (rows <= 0 || cols <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(rows), "Rows and cols must be positive.");
        }

        int expected = rows * cols;
        if (left.Length != expected || right.Length != expected)
        {
            throw new ArgumentException("Matrix lengths must both equal rows*cols.");
        }

        return global::LAL.LinalgCore.Dot.Dotu(left, right);
    }

    public static void Add(ReadOnlySpan<float> left, ReadOnlySpan<float> right, Span<float> destination, int rows, int cols)
    {
        ValidateBinary(left, right, destination, rows, cols);

        for (int i = 0; i < destination.Length; i++)
        {
            destination[i] = left[i] + right[i];
        }
    }

    public static void Subtract(ReadOnlySpan<float> left, ReadOnlySpan<float> right, Span<float> destination, int rows, int cols)
    {
        ValidateBinary(left, right, destination, rows, cols);

        for (int i = 0; i < destination.Length; i++)
        {
            destination[i] = left[i] - right[i];
        }
    }

    public static void Multiply(ReadOnlySpan<float> matrix, int rows, int cols, ReadOnlySpan<float> vector, Span<float> destination)
    {
        Gemv.Multiply(matrix, rows, cols, vector, destination);
    }

    public static void Multiply(
        ReadOnlySpan<float> left,
        int leftRows,
        int sharedDim,
        ReadOnlySpan<float> right,
        int rightCols,
        Span<float> destination)
    {
        Gemm.Multiply(left, right, destination, leftRows, rightCols, sharedDim, alpha: 1f, beta: 0f);
    }

    public static float Dot(ReadOnlySpan<float> left, ReadOnlySpan<float> right, int rows, int cols)
    {
        if (rows <= 0 || cols <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(rows), "Rows and cols must be positive.");
        }

        int expected = rows * cols;
        if (left.Length != expected || right.Length != expected)
        {
            throw new ArgumentException("Matrix lengths must both equal rows*cols.");
        }

        return global::LAL.LinalgCore.Dot.Dotu(left, right);
    }

    public static void Add(ReadOnlySpan<Complex> left, ReadOnlySpan<Complex> right, Span<Complex> destination, int rows, int cols)
    {
        ValidateBinary(left, right, destination, rows, cols);

        for (int i = 0; i < destination.Length; i++)
        {
            destination[i] = left[i] + right[i];
        }
    }

    public static void Subtract(ReadOnlySpan<Complex> left, ReadOnlySpan<Complex> right, Span<Complex> destination, int rows, int cols)
    {
        ValidateBinary(left, right, destination, rows, cols);

        for (int i = 0; i < destination.Length; i++)
        {
            destination[i] = left[i] - right[i];
        }
    }

    public static void Multiply(ReadOnlySpan<Complex> matrix, int rows, int cols, ReadOnlySpan<Complex> vector, Span<Complex> destination)
    {
        Gemv.Multiply(matrix, rows, cols, vector, destination);
    }

    public static void Multiply(
        ReadOnlySpan<Complex> left,
        int leftRows,
        int sharedDim,
        ReadOnlySpan<Complex> right,
        int rightCols,
        Span<Complex> destination)
    {
        Gemm.Multiply(left, right, destination, leftRows, rightCols, sharedDim, alpha: Complex.One, beta: Complex.Zero);
    }

    public static Complex Dot(ReadOnlySpan<Complex> left, ReadOnlySpan<Complex> right, int rows, int cols)
    {
        if (rows <= 0 || cols <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(rows), "Rows and cols must be positive.");
        }

        int expected = rows * cols;
        if (left.Length != expected || right.Length != expected)
        {
            throw new ArgumentException("Matrix lengths must both equal rows*cols.");
        }

        return global::LAL.LinalgCore.Dot.Dotu(left, right);
    }

    private static void ValidateBinary(ReadOnlySpan<float> left, ReadOnlySpan<float> right, Span<float> destination, int rows, int cols)
    {
        if (rows <= 0 || cols <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(rows), "Rows and cols must be positive.");
        }

        int expected = rows * cols;
        if (left.Length != expected || right.Length != expected || destination.Length != expected)
        {
            throw new ArgumentException("Matrix lengths must all equal rows*cols.");
        }
    }

    private static void ValidateBinary(ReadOnlySpan<Complex> left, ReadOnlySpan<Complex> right, Span<Complex> destination, int rows, int cols)
    {
        if (rows <= 0 || cols <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(rows), "Rows and cols must be positive.");
        }

        int expected = rows * cols;
        if (left.Length != expected || right.Length != expected || destination.Length != expected)
        {
            throw new ArgumentException("Matrix lengths must all equal rows*cols.");
        }
    }

    private static void ValidateBinary(ReadOnlySpan<double> left, ReadOnlySpan<double> right, Span<double> destination, int rows, int cols)
    {
        if (rows <= 0 || cols <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(rows), "Rows and cols must be positive.");
        }

        int expected = rows * cols;
        if (left.Length != expected || right.Length != expected || destination.Length != expected)
        {
            throw new ArgumentException("Matrix lengths must all equal rows*cols.");
        }
    }
}
