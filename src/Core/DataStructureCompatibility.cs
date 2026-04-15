using System.Numerics;
using LAL.TensorCore;

namespace LAL.Core;

public static partial class DataStructureCompatibility
{
    public static int[] VectorShape(int length)
    {
        if (length <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(length), "Vector length must be positive.");
        }

        return [length];
    }

    public static int[] MatrixShape(int rows, int cols)
    {
        if (rows <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(rows), "Rows must be positive.");
        }

        if (cols <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(cols), "Cols must be positive.");
        }

        return [rows, cols];
    }

    public static NDBuffer<Complex> CreateComplexVector(int length)
    {
        return new NDBuffer<Complex>(VectorShape(length));
    }

    public static NDBuffer<Complex> CreateComplexMatrix(int rows, int cols)
    {
        return new NDBuffer<Complex>(MatrixShape(rows, cols));
    }

    public static NDBuffer<Complex> WrapComplex(Complex[] storage, ReadOnlySpan<int> shape)
    {
        return new NDBuffer<Complex>(storage, shape);
    }

    public static NDBuffer<Complex> WrapComplex(Complex[] storage, ReadOnlySpan<int> shape, ReadOnlySpan<int> strides)
    {
        return new NDBuffer<Complex>(storage, shape, strides);
    }

    public static int[] NormalizeShape(ReadOnlySpan<int> shape)
    {
        if (shape.Length == 0)
        {
            throw new ArgumentException("Shape must contain at least one dimension.", nameof(shape));
        }

        int[] normalized = shape.ToArray();
        for (int i = 0; i < normalized.Length; i++)
        {
            if (normalized[i] <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(shape), "Shape dimensions must be positive.");
            }
        }

        return normalized;
    }

    public static int GetElementCount(ReadOnlySpan<int> shape)
    {
        if (shape.Length == 0)
        {
            throw new ArgumentException("Shape must contain at least one dimension.", nameof(shape));
        }

        int total = 1;
        for (int i = 0; i < shape.Length; i++)
        {
            int dim = shape[i];
            if (dim <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(shape), "Shape dimensions must be positive.");
            }

            checked
            {
                total *= dim;
            }
        }

        return total;
    }

    public static int[] RowMajorStrides(ReadOnlySpan<int> shape)
    {
        int[] normalized = NormalizeShape(shape);
        return TensorShape.ComputeRowMajorStrides(normalized);
    }

    public static int Offset(ReadOnlySpan<int> indices, ReadOnlySpan<int> shape, ReadOnlySpan<int> strides)
    {
        return TensorShape.GetOffset(indices, shape, strides);
    }

    public static bool TryGetVectorLength(ReadOnlySpan<int> shape, out int length)
    {
        if (shape.Length == 1 && shape[0] > 0)
        {
            length = shape[0];
            return true;
        }

        length = 0;
        return false;
    }

    public static bool TryGetMatrixDimensions(ReadOnlySpan<int> shape, out int rows, out int cols)
    {
        if (shape.Length == 2 && shape[0] > 0 && shape[1] > 0)
        {
            rows = shape[0];
            cols = shape[1];
            return true;
        }

        rows = 0;
        cols = 0;
        return false;
    }

    public static void EnsureBufferMatchesShape<T>(ReadOnlySpan<T> buffer, ReadOnlySpan<int> shape, string? paramName = null)
    {
        int expected = GetElementCount(shape);
        if (buffer.Length != expected)
        {
            throw new ArgumentException($"Buffer length {buffer.Length} does not match shape element count {expected}.", paramName ?? nameof(buffer));
        }
    }

    public static void EnsureVectorCompatible<T>(ReadOnlySpan<T> buffer, ReadOnlySpan<int> shape)
    {
        if (!TryGetVectorLength(shape, out int length))
        {
            throw new ArgumentException("Shape must be rank-1 for vector compatibility.", nameof(shape));
        }

        if (buffer.Length != length)
        {
            throw new ArgumentException("Buffer length must match vector shape length.", nameof(buffer));
        }
    }

    public static void EnsureMatrixCompatible<T>(ReadOnlySpan<T> buffer, ReadOnlySpan<int> shape)
    {
        if (!TryGetMatrixDimensions(shape, out _, out _))
        {
            throw new ArgumentException("Shape must be rank-2 for matrix compatibility.", nameof(shape));
        }

        EnsureBufferMatchesShape(buffer, shape, nameof(buffer));
    }

    public static void EnsureComplexCompatible(ReadOnlySpan<Complex> buffer, ReadOnlySpan<int> shape)
    {
        EnsureBufferMatchesShape(buffer, shape, nameof(buffer));
    }

    public static void EnsureComplexVectorCompatible(ReadOnlySpan<Complex> buffer, ReadOnlySpan<int> shape)
    {
        EnsureVectorCompatible(buffer, shape);
    }

    public static void EnsureComplexMatrixCompatible(ReadOnlySpan<Complex> buffer, ReadOnlySpan<int> shape)
    {
        EnsureMatrixCompatible(buffer, shape);
    }

    public static bool IsRowMajorContiguous(ReadOnlySpan<int> shape, ReadOnlySpan<int> strides)
    {
        if (shape.Length == 0 || shape.Length != strides.Length)
        {
            return false;
        }

        int expectedStride = 1;
        for (int axis = shape.Length - 1; axis >= 0; axis--)
        {
            if (shape[axis] <= 0)
            {
                return false;
            }

            if (strides[axis] != expectedStride)
            {
                return false;
            }

            checked
            {
                expectedStride *= shape[axis];
            }
        }

        return true;
    }
}

public sealed class NDBuffer<T>
{
    private readonly T[] _storage;

    public int[] Shape { get; }

    public int[] Strides { get; }

    public int Length => _storage.Length;

    public bool IsRowMajorContiguous => DataStructureCompatibility.IsRowMajorContiguous(Shape, Strides);

    public NDBuffer(ReadOnlySpan<int> shape)
    {
        Shape = DataStructureCompatibility.NormalizeShape(shape);
        Strides = TensorShape.ComputeRowMajorStrides(Shape);
        _storage = new T[DataStructureCompatibility.GetElementCount(Shape)];
    }

    public NDBuffer(T[] storage, ReadOnlySpan<int> shape)
    {
        ArgumentNullException.ThrowIfNull(storage);

        Shape = DataStructureCompatibility.NormalizeShape(shape);
        Strides = TensorShape.ComputeRowMajorStrides(Shape);
        DataStructureCompatibility.EnsureBufferMatchesShape<T>(storage, Shape, nameof(storage));
        _storage = storage;
    }

    public NDBuffer(T[] storage, ReadOnlySpan<int> shape, ReadOnlySpan<int> strides)
    {
        ArgumentNullException.ThrowIfNull(storage);

        Shape = DataStructureCompatibility.NormalizeShape(shape);
        if (strides.Length != Shape.Length)
        {
            throw new ArgumentException("Strides rank must match shape rank.", nameof(strides));
        }

        Strides = strides.ToArray();
        for (int i = 0; i < Strides.Length; i++)
        {
            if (Strides[i] < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(strides), "Stride values must be non-negative.");
            }
        }

        DataStructureCompatibility.EnsureBufferMatchesShape<T>(storage, Shape, nameof(storage));
        _storage = storage;
    }

    public Span<T> AsSpan()
    {
        return _storage;
    }

    internal T[] DangerousStorage => _storage;

    public ReadOnlySpan<T> AsReadOnlySpan()
    {
        return _storage;
    }

    public int GetOffset(ReadOnlySpan<int> indices)
    {
        return TensorShape.GetOffset(indices, Shape, Strides);
    }

    public ref T At(ReadOnlySpan<int> indices)
    {
        int offset = GetOffset(indices);
        return ref _storage[offset];
    }

    public T Get(ReadOnlySpan<int> indices)
    {
        return _storage[GetOffset(indices)];
    }

    public void Set(ReadOnlySpan<int> indices, T value)
    {
        _storage[GetOffset(indices)] = value;
    }

    public bool TryGetVectorLength(out int length)
    {
        return DataStructureCompatibility.TryGetVectorLength(Shape, out length);
    }

    public bool TryGetMatrixDimensions(out int rows, out int cols)
    {
        return DataStructureCompatibility.TryGetMatrixDimensions(Shape, out rows, out cols);
    }
}
