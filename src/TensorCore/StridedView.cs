namespace LAL.TensorCore;

internal static class StridedView
{
    public static ReadOnlySpan<T> Slice1D<T>(ReadOnlySpan<T> data, int start, int length)
    {
        return data.Slice(start, length);
    }

    public static Span<T> Slice1D<T>(Span<T> data, int start, int length)
    {
        return data.Slice(start, length);
    }

    public static int Slice1D<T>(ReadOnlySpan<T> data, int start, int stop, int step, Span<T> destination)
    {
        if (step == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(step), "Step must not be zero.");
        }

        if (start < 0 || start > data.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(start));
        }

        if (stop < 0 || stop > data.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(stop));
        }

        int count = 0;
        if (step > 0)
        {
            for (int i = start; i < stop; i += step)
            {
                if (count >= destination.Length)
                {
                    throw new ArgumentException("Destination span is too small for sliced result.", nameof(destination));
                }

                destination[count++] = data[i];
            }
        }
        else
        {
            for (int i = start; i > stop; i += step)
            {
                if (count >= destination.Length)
                {
                    throw new ArgumentException("Destination span is too small for sliced result.", nameof(destination));
                }

                destination[count++] = data[i];
            }
        }

        return count;
    }

    public static int[] ComputeSlicedShape(ReadOnlySpan<int> shape, int axis, int start, int length)
    {
        if ((uint)axis >= (uint)shape.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(axis));
        }

        if (start < 0 || length < 0 || start + length > shape[axis])
        {
            throw new ArgumentOutOfRangeException(nameof(length), "Slice exceeds axis range.");
        }

        int[] result = shape.ToArray();
        result[axis] = length;
        return result;
    }

    public static int[] PermuteShape(ReadOnlySpan<int> shape, ReadOnlySpan<int> permutation)
    {
        if (shape.Length != permutation.Length)
        {
            throw new ArgumentException("Permutation rank must match shape rank.");
        }

        bool[] seen = new bool[shape.Length];
        int[] result = new int[shape.Length];

        for (int i = 0; i < permutation.Length; i++)
        {
            int axis = permutation[i];
            if ((uint)axis >= (uint)shape.Length || seen[axis])
            {
                throw new ArgumentException("Permutation must contain each axis exactly once.");
            }

            seen[axis] = true;
            result[i] = shape[axis];
        }

        return result;
    }

    public static int[] ExpandDims(ReadOnlySpan<int> shape, int axis)
    {
        if (axis < 0 || axis > shape.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(axis));
        }

        int[] result = new int[shape.Length + 1];
        int read = 0;

        for (int i = 0; i < result.Length; i++)
        {
            if (i == axis)
            {
                result[i] = 1;
            }
            else
            {
                result[i] = shape[read++];
            }
        }

        return result;
    }

    public static int[] Squeeze(ReadOnlySpan<int> shape, int axis = -1)
    {
        if (axis >= shape.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(axis));
        }

        if (axis >= 0)
        {
            if (shape[axis] != 1)
            {
                throw new ArgumentException("Selected axis cannot be squeezed because its size is not 1.");
            }

            int[] result = new int[shape.Length - 1];
            int w = 0;
            for (int i = 0; i < shape.Length; i++)
            {
                if (i != axis)
                {
                    result[w++] = shape[i];
                }
            }

            return result.Length == 0 ? new[] { 1 } : result;
        }

        List<int> dims = new(shape.Length);
        for (int i = 0; i < shape.Length; i++)
        {
            if (shape[i] != 1)
            {
                dims.Add(shape[i]);
            }
        }

        return dims.Count == 0 ? new[] { 1 } : dims.ToArray();
    }
}

