namespace LAL.TensorCore;

internal static class TensorShape
{
    public static void ValidateShape(ReadOnlySpan<int> shape)
    {
        if (shape.Length == 0)
        {
            throw new ArgumentException("Shape must contain at least one dimension.", nameof(shape));
        }

        for (int i = 0; i < shape.Length; i++)
        {
            if (shape[i] <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(shape), "All dimensions must be positive.");
            }
        }
    }

    public static int[] ComputeRowMajorStrides(ReadOnlySpan<int> shape)
    {
        ValidateShape(shape);

        int[] strides = new int[shape.Length];
        int stride = 1;

        for (int i = shape.Length - 1; i >= 0; i--)
        {
            strides[i] = stride;
            checked
            {
                stride *= shape[i];
            }
        }

        return strides;
    }

    public static int GetElementCount(ReadOnlySpan<int> shape)
    {
        ValidateShape(shape);

        int count = 1;
        for (int i = 0; i < shape.Length; i++)
        {
            checked
            {
                count *= shape[i];
            }
        }

        return count;
    }

    public static int GetOffset(ReadOnlySpan<int> indices, ReadOnlySpan<int> shape, ReadOnlySpan<int> strides)
    {
        if (indices.Length != shape.Length || shape.Length != strides.Length)
        {
            throw new ArgumentException("Indices, shape, and strides must have matching lengths.");
        }

        int offset = 0;
        for (int i = 0; i < indices.Length; i++)
        {
            if ((uint)indices[i] >= (uint)shape[i])
            {
                throw new ArgumentOutOfRangeException(nameof(indices), "Index is out of range for given shape.");
            }

            checked
            {
                offset += indices[i] * strides[i];
            }
        }

        return offset;
    }
}

