using System.Numerics;

namespace LAL.TensorCore;

internal static class ConcatStack
{
    public static void Concatenate(ReadOnlySpan<double> left, ReadOnlySpan<double> right, Span<double> destination)
    {
        Concatenate1D(left, right, destination);
    }

    public static void Concatenate1D(ReadOnlySpan<double> left, ReadOnlySpan<double> right, Span<double> destination)
    {
        Concatenate1DCore(left, right, destination);
    }

    public static void Stack(ReadOnlySpan<double> topRow, ReadOnlySpan<double> bottomRow, int cols, Span<double> destination)
    {
        StackRows(topRow, bottomRow, cols, destination);
    }

    public static void StackRows(ReadOnlySpan<double> topRow, ReadOnlySpan<double> bottomRow, int cols, Span<double> destination)
    {
        StackRowsCore(topRow, bottomRow, cols, destination);
    }

    public static void Concatenate(ReadOnlySpan<float> left, ReadOnlySpan<float> right, Span<float> destination)
    {
        Concatenate1D(left, right, destination);
    }

    public static void Concatenate1D(ReadOnlySpan<float> left, ReadOnlySpan<float> right, Span<float> destination)
    {
        Concatenate1DCore(left, right, destination);
    }

    public static void Stack(ReadOnlySpan<float> topRow, ReadOnlySpan<float> bottomRow, int cols, Span<float> destination)
    {
        StackRows(topRow, bottomRow, cols, destination);
    }

    public static void StackRows(ReadOnlySpan<float> topRow, ReadOnlySpan<float> bottomRow, int cols, Span<float> destination)
    {
        StackRowsCore(topRow, bottomRow, cols, destination);
    }

    public static void Concatenate(ReadOnlySpan<Complex> left, ReadOnlySpan<Complex> right, Span<Complex> destination)
    {
        Concatenate1D(left, right, destination);
    }

    public static void Concatenate1D(ReadOnlySpan<Complex> left, ReadOnlySpan<Complex> right, Span<Complex> destination)
    {
        Concatenate1DCore(left, right, destination);
    }

    public static void Stack(ReadOnlySpan<Complex> topRow, ReadOnlySpan<Complex> bottomRow, int cols, Span<Complex> destination)
    {
        StackRows(topRow, bottomRow, cols, destination);
    }

    public static void StackRows(ReadOnlySpan<Complex> topRow, ReadOnlySpan<Complex> bottomRow, int cols, Span<Complex> destination)
    {
        StackRowsCore(topRow, bottomRow, cols, destination);
    }

    private static void Concatenate1DCore<T>(ReadOnlySpan<T> left, ReadOnlySpan<T> right, Span<T> destination)
    {
        if (destination.Length != left.Length + right.Length)
        {
            throw new ArgumentException("Destination length must equal left+right lengths.");
        }

        left.CopyTo(destination);
        right.CopyTo(destination.Slice(left.Length));
    }

    private static void StackRowsCore<T>(ReadOnlySpan<T> topRow, ReadOnlySpan<T> bottomRow, int cols, Span<T> destination)
    {
        if (cols <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(cols), "Cols must be positive.");
        }

        if (topRow.Length != cols || bottomRow.Length != cols)
        {
            throw new ArgumentException("Rows must have exactly cols elements.");
        }

        if (destination.Length != 2 * cols)
        {
            throw new ArgumentException("Destination length must be 2*cols.");
        }

        topRow.CopyTo(destination.Slice(0, cols));
        bottomRow.CopyTo(destination.Slice(cols, cols));
    }
}

