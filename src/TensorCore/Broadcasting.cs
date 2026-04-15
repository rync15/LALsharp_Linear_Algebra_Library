namespace LAL.TensorCore;

internal static class Broadcasting
{
    public static int[] BroadcastShapes(ReadOnlySpan<int> left, ReadOnlySpan<int> right)
    {
        if (left.Length == 0 || right.Length == 0)
        {
            throw new ArgumentException("Input shapes must not be empty.");
        }

        int rank = Math.Max(left.Length, right.Length);
        int[] output = new int[rank];

        for (int i = 0; i < rank; i++)
        {
            int l = i < rank - left.Length ? 1 : left[i - (rank - left.Length)];
            int r = i < rank - right.Length ? 1 : right[i - (rank - right.Length)];

            if (l != r && l != 1 && r != 1)
            {
                throw new ArgumentException("Shapes are not broadcast-compatible.");
            }

            output[i] = Math.Max(l, r);
        }

        return output;
    }

    public static int[] ComputeBroadcastStrides(
        ReadOnlySpan<int> sourceShape,
        ReadOnlySpan<int> sourceStrides,
        ReadOnlySpan<int> targetShape)
    {
        if (sourceShape.Length != sourceStrides.Length)
        {
            throw new ArgumentException("Source shape and strides must have matching lengths.");
        }

        if (sourceShape.Length > targetShape.Length)
        {
            throw new ArgumentException("Source rank cannot exceed target rank.");
        }

        int[] result = new int[targetShape.Length];
        int rankDiff = targetShape.Length - sourceShape.Length;

        for (int i = targetShape.Length - 1; i >= 0; i--)
        {
            int srcAxis = i - rankDiff;
            if (srcAxis < 0)
            {
                result[i] = 0;
                continue;
            }

            int srcSize = sourceShape[srcAxis];
            int dstSize = targetShape[i];

            if (srcSize == dstSize)
            {
                result[i] = sourceStrides[srcAxis];
            }
            else if (srcSize == 1)
            {
                result[i] = 0;
            }
            else
            {
                throw new ArgumentException("Source shape cannot be broadcast to target shape.");
            }
        }

        return result;
    }
}

