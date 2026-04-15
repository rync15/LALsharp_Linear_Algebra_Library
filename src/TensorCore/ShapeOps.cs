using System.Numerics;

namespace LAL.TensorCore;

internal static class ShapeOps
{
    public static void Reshape(ReadOnlySpan<double> source, Span<double> destination)
    {
        ReshapeCore(source, destination);
    }

    public static void Reshape(ReadOnlySpan<float> source, Span<float> destination)
    {
        ReshapeCore(source, destination);
    }

    public static void Reshape(ReadOnlySpan<Complex> source, Span<Complex> destination)
    {
        ReshapeCore(source, destination);
    }

    public static void ReshapeCopy(ReadOnlySpan<double> source, Span<double> destination)
    {
        Reshape(source, destination);
    }

    public static void ReshapeCopy(ReadOnlySpan<float> source, Span<float> destination)
    {
        Reshape(source, destination);
    }

    public static void ReshapeCopy(ReadOnlySpan<Complex> source, Span<Complex> destination)
    {
        Reshape(source, destination);
    }

    public static void Flatten(ReadOnlySpan<double> source, Span<double> destination)
    {
        Reshape(source, destination);
    }

    public static void Flatten(ReadOnlySpan<float> source, Span<float> destination)
    {
        Reshape(source, destination);
    }

    public static void Flatten(ReadOnlySpan<Complex> source, Span<Complex> destination)
    {
        Reshape(source, destination);
    }

    public static void Transpose2D(ReadOnlySpan<double> source, int rows, int cols, Span<double> destination)
    {
        Transpose2DCore(source, rows, cols, destination);
    }

    public static void Transpose2D(ReadOnlySpan<float> source, int rows, int cols, Span<float> destination)
    {
        Transpose2DCore(source, rows, cols, destination);
    }

    public static void Transpose2D(ReadOnlySpan<Complex> source, int rows, int cols, Span<Complex> destination)
    {
        Transpose2DCore(source, rows, cols, destination);
    }

    public static void SwapAxes2D(ReadOnlySpan<double> source, int rows, int cols, Span<double> destination)
    {
        Transpose2D(source, rows, cols, destination);
    }

    public static void SwapAxes2D(ReadOnlySpan<float> source, int rows, int cols, Span<float> destination)
    {
        Transpose2D(source, rows, cols, destination);
    }

    public static void SwapAxes2D(ReadOnlySpan<Complex> source, int rows, int cols, Span<Complex> destination)
    {
        Transpose2D(source, rows, cols, destination);
    }

    public static int[] ExpandDims(ReadOnlySpan<int> shape, int axis)
    {
        return StridedView.ExpandDims(shape, axis);
    }

    public static int[] Squeeze(ReadOnlySpan<int> shape, int axis = -1)
    {
        return StridedView.Squeeze(shape, axis);
    }

    public static int[] SwapAxes(ReadOnlySpan<int> shape, int axisA, int axisB)
    {
        if ((uint)axisA >= (uint)shape.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(axisA), "Axis is out of range for shape rank.");
        }

        if ((uint)axisB >= (uint)shape.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(axisB), "Axis is out of range for shape rank.");
        }

        int[] result = shape.ToArray();
        (result[axisA], result[axisB]) = (result[axisB], result[axisA]);
        return result;
    }

    public static int[] TransposeShape(ReadOnlySpan<int> shape, ReadOnlySpan<int> permutation)
    {
        return StridedView.PermuteShape(shape, permutation);
    }

    private static void ReshapeCore<T>(ReadOnlySpan<T> source, Span<T> destination)
    {
        if (source.Length != destination.Length)
        {
            throw new ArgumentException("Source and destination lengths must match.");
        }

        source.CopyTo(destination);
    }

    private static void Transpose2DCore<T>(ReadOnlySpan<T> source, int rows, int cols, Span<T> destination)
    {
        ValidateMatrix(source.Length, rows, cols, destination.Length);

        for (int r = 0; r < rows; r++)
        {
            int rowOffset = r * cols;
            for (int c = 0; c < cols; c++)
            {
                destination[(c * rows) + r] = source[rowOffset + c];
            }
        }
    }

    private static void ValidateMatrix(int sourceLength, int rows, int cols, int destinationLength)
    {
        if (rows <= 0 || cols <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(rows), "Rows and cols must be positive.");
        }

        if (sourceLength != rows * cols || destinationLength != rows * cols)
        {
            throw new ArgumentException("Matrix lengths must match rows*cols.");
        }
    }
}

