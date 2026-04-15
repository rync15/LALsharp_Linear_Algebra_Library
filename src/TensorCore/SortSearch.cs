using System.Buffers;
using System.Numerics;

namespace LAL.TensorCore;

internal static class SortSearch
{
    public static void Argsort(ReadOnlySpan<double> values, Span<int> indices, bool ascending)
    {
        Argsort(values, indices);

        if (!ascending)
        {
            indices.Reverse();
        }
    }

    public static void Argsort(ReadOnlySpan<double> values, Span<int> indices)
    {
        if (indices.Length != values.Length)
        {
            throw new ArgumentException("Indices length must match values length.");
        }

        int length = values.Length;
        int[] rentedIndices = ArrayPool<int>.Shared.Rent(length);
        double[] rentedValues = ArrayPool<double>.Shared.Rent(length);

        try
        {
            for (int i = 0; i < length; i++)
            {
                rentedIndices[i] = i;
                rentedValues[i] = values[i];
            }

            Array.Sort(
                rentedIndices,
                0,
                length,
                Comparer<int>.Create((a, b) => rentedValues[a].CompareTo(rentedValues[b])));

            rentedIndices.AsSpan(0, length).CopyTo(indices);
        }
        finally
        {
            ArrayPool<double>.Shared.Return(rentedValues);
            ArrayPool<int>.Shared.Return(rentedIndices);
        }
    }

    public static int SearchSorted(ReadOnlySpan<double> sortedValues, double value)
    {
        int lo = 0;
        int hi = sortedValues.Length;

        while (lo < hi)
        {
            int mid = lo + ((hi - lo) / 2);
            if (sortedValues[mid] < value)
            {
                lo = mid + 1;
            }
            else
            {
                hi = mid;
            }
        }

        return lo;
    }

    public static int[] NonZero(ReadOnlySpan<double> values)
    {
        int count = 0;
        for (int i = 0; i < values.Length; i++)
        {
            if (values[i] != 0d)
            {
                count++;
            }
        }

        int[] result = new int[count];
        int write = 0;
        for (int i = 0; i < values.Length; i++)
        {
            if (values[i] != 0d)
            {
                result[write++] = i;
            }
        }

        return result;
    }

    public static int NonZero(ReadOnlySpan<double> values, Span<int> destination)
    {
        int count = 0;
        for (int i = 0; i < values.Length; i++)
        {
            if (values[i] == 0d)
            {
                continue;
            }

            if (count >= destination.Length)
            {
                throw new ArgumentException("Destination span is too small for non-zero indices.", nameof(destination));
            }

            destination[count++] = i;
        }

        return count;
    }

    public static void Argsort(ReadOnlySpan<float> values, Span<int> indices, bool ascending)
    {
        Argsort(values, indices);

        if (!ascending)
        {
            indices.Reverse();
        }
    }

    public static void Argsort(ReadOnlySpan<float> values, Span<int> indices)
    {
        if (indices.Length != values.Length)
        {
            throw new ArgumentException("Indices length must match values length.");
        }

        int length = values.Length;
        int[] rentedIndices = ArrayPool<int>.Shared.Rent(length);
        float[] rentedValues = ArrayPool<float>.Shared.Rent(length);

        try
        {
            for (int i = 0; i < length; i++)
            {
                rentedIndices[i] = i;
                rentedValues[i] = values[i];
            }

            Array.Sort(
                rentedIndices,
                0,
                length,
                Comparer<int>.Create((a, b) => rentedValues[a].CompareTo(rentedValues[b])));

            rentedIndices.AsSpan(0, length).CopyTo(indices);
        }
        finally
        {
            ArrayPool<float>.Shared.Return(rentedValues);
            ArrayPool<int>.Shared.Return(rentedIndices);
        }
    }

    public static int SearchSorted(ReadOnlySpan<float> sortedValues, float value)
    {
        int lo = 0;
        int hi = sortedValues.Length;

        while (lo < hi)
        {
            int mid = lo + ((hi - lo) / 2);
            if (sortedValues[mid] < value)
            {
                lo = mid + 1;
            }
            else
            {
                hi = mid;
            }
        }

        return lo;
    }

    public static int[] NonZero(ReadOnlySpan<float> values)
    {
        int count = 0;
        for (int i = 0; i < values.Length; i++)
        {
            if (values[i] != 0f)
            {
                count++;
            }
        }

        int[] result = new int[count];
        int write = 0;
        for (int i = 0; i < values.Length; i++)
        {
            if (values[i] != 0f)
            {
                result[write++] = i;
            }
        }

        return result;
    }

    public static int NonZero(ReadOnlySpan<float> values, Span<int> destination)
    {
        int count = 0;
        for (int i = 0; i < values.Length; i++)
        {
            if (values[i] == 0f)
            {
                continue;
            }

            if (count >= destination.Length)
            {
                throw new ArgumentException("Destination span is too small for non-zero indices.", nameof(destination));
            }

            destination[count++] = i;
        }

        return count;
    }

    public static void Argsort(ReadOnlySpan<Complex> values, Span<int> indices, bool ascending)
    {
        Argsort(values, indices);

        if (!ascending)
        {
            indices.Reverse();
        }
    }

    public static void Argsort(ReadOnlySpan<Complex> values, Span<int> indices)
    {
        if (indices.Length != values.Length)
        {
            throw new ArgumentException("Indices length must match values length.");
        }

        int length = values.Length;
        int[] rentedIndices = ArrayPool<int>.Shared.Rent(length);
        Complex[] rentedValues = ArrayPool<Complex>.Shared.Rent(length);

        try
        {
            for (int i = 0; i < length; i++)
            {
                rentedIndices[i] = i;
                rentedValues[i] = values[i];
            }

            Array.Sort(
                rentedIndices,
                0,
                length,
                Comparer<int>.Create((a, b) => rentedValues[a].Magnitude.CompareTo(rentedValues[b].Magnitude)));

            rentedIndices.AsSpan(0, length).CopyTo(indices);
        }
        finally
        {
            ArrayPool<Complex>.Shared.Return(rentedValues);
            ArrayPool<int>.Shared.Return(rentedIndices);
        }
    }

    public static int SearchSorted(ReadOnlySpan<Complex> sortedValues, Complex value)
    {
        double targetMagnitude = value.Magnitude;
        int lo = 0;
        int hi = sortedValues.Length;

        while (lo < hi)
        {
            int mid = lo + ((hi - lo) / 2);
            if (sortedValues[mid].Magnitude < targetMagnitude)
            {
                lo = mid + 1;
            }
            else
            {
                hi = mid;
            }
        }

        return lo;
    }

    public static int[] NonZero(ReadOnlySpan<Complex> values)
    {
        int count = 0;
        for (int i = 0; i < values.Length; i++)
        {
            if (values[i] != Complex.Zero)
            {
                count++;
            }
        }

        int[] result = new int[count];
        int write = 0;
        for (int i = 0; i < values.Length; i++)
        {
            if (values[i] != Complex.Zero)
            {
                result[write++] = i;
            }
        }

        return result;
    }

    public static int NonZero(ReadOnlySpan<Complex> values, Span<int> destination)
    {
        int count = 0;
        for (int i = 0; i < values.Length; i++)
        {
            if (values[i] == Complex.Zero)
            {
                continue;
            }

            if (count >= destination.Length)
            {
                throw new ArgumentException("Destination span is too small for non-zero indices.", nameof(destination));
            }

            destination[count++] = i;
        }

        return count;
    }

    public static int[] Where(ReadOnlySpan<bool> mask)
    {
        int count = 0;
        for (int i = 0; i < mask.Length; i++)
        {
            if (mask[i])
            {
                count++;
            }
        }

        int[] result = new int[count];
        int write = 0;
        for (int i = 0; i < mask.Length; i++)
        {
            if (mask[i])
            {
                result[write++] = i;
            }
        }

        return result;
    }

    public static int Where(ReadOnlySpan<bool> mask, Span<int> destination)
    {
        int count = 0;
        for (int i = 0; i < mask.Length; i++)
        {
            if (!mask[i])
            {
                continue;
            }

            if (count >= destination.Length)
            {
                throw new ArgumentException("Destination span is too small for where indices.", nameof(destination));
            }

            destination[count++] = i;
        }

        return count;
    }

    public static int ArgMax(ReadOnlySpan<double> values)
    {
        if (values.Length == 0)
        {
            throw new ArgumentException("Values must not be empty.", nameof(values));
        }

        int bestIndex = 0;
        double bestValue = values[0];

        for (int i = 1; i < values.Length; i++)
        {
            if (values[i] > bestValue)
            {
                bestValue = values[i];
                bestIndex = i;
            }
        }

        return bestIndex;
    }

    public static int ArgMax(ReadOnlySpan<float> values)
    {
        if (values.Length == 0)
        {
            throw new ArgumentException("Values must not be empty.", nameof(values));
        }

        int bestIndex = 0;
        float bestValue = values[0];

        for (int i = 1; i < values.Length; i++)
        {
            if (values[i] > bestValue)
            {
                bestValue = values[i];
                bestIndex = i;
            }
        }

        return bestIndex;
    }

    public static int ArgMax(ReadOnlySpan<Complex> values)
    {
        if (values.Length == 0)
        {
            throw new ArgumentException("Values must not be empty.", nameof(values));
        }

        int bestIndex = 0;
        double bestMagnitude = values[0].Magnitude;

        for (int i = 1; i < values.Length; i++)
        {
            double magnitude = values[i].Magnitude;
            if (magnitude > bestMagnitude)
            {
                bestMagnitude = magnitude;
                bestIndex = i;
            }
        }

        return bestIndex;
    }

    public static int ArgMin(ReadOnlySpan<double> values)
    {
        if (values.Length == 0)
        {
            throw new ArgumentException("Values must not be empty.", nameof(values));
        }

        int bestIndex = 0;
        double bestValue = values[0];

        for (int i = 1; i < values.Length; i++)
        {
            if (values[i] < bestValue)
            {
                bestValue = values[i];
                bestIndex = i;
            }
        }

        return bestIndex;
    }

    public static int ArgMin(ReadOnlySpan<float> values)
    {
        if (values.Length == 0)
        {
            throw new ArgumentException("Values must not be empty.", nameof(values));
        }

        int bestIndex = 0;
        float bestValue = values[0];

        for (int i = 1; i < values.Length; i++)
        {
            if (values[i] < bestValue)
            {
                bestValue = values[i];
                bestIndex = i;
            }
        }

        return bestIndex;
    }

    public static int ArgMin(ReadOnlySpan<Complex> values)
    {
        if (values.Length == 0)
        {
            throw new ArgumentException("Values must not be empty.", nameof(values));
        }

        int bestIndex = 0;
        double bestMagnitude = values[0].Magnitude;

        for (int i = 1; i < values.Length; i++)
        {
            double magnitude = values[i].Magnitude;
            if (magnitude < bestMagnitude)
            {
                bestMagnitude = magnitude;
                bestIndex = i;
            }
        }

        return bestIndex;
    }

    public static int[] Lexsort(ReadOnlySpan<double> primaryKey, ReadOnlySpan<double> secondaryKey)
    {
        int[] indices = new int[primaryKey.Length];
        Lexsort(primaryKey, secondaryKey, indices);
        return indices;
    }

    public static void Lexsort(ReadOnlySpan<double> primaryKey, ReadOnlySpan<double> secondaryKey, Span<int> indices)
    {
        if (primaryKey.Length != secondaryKey.Length || primaryKey.Length != indices.Length)
        {
            throw new ArgumentException("Keys and indices lengths must match.");
        }

        int length = indices.Length;
        int[] rentedIndices = ArrayPool<int>.Shared.Rent(length);
        double[] rentedPrimary = ArrayPool<double>.Shared.Rent(length);
        double[] rentedSecondary = ArrayPool<double>.Shared.Rent(length);

        try
        {
            for (int i = 0; i < length; i++)
            {
                rentedIndices[i] = i;
                rentedPrimary[i] = primaryKey[i];
                rentedSecondary[i] = secondaryKey[i];
            }

            Array.Sort(
                rentedIndices,
                0,
                length,
                Comparer<int>.Create((a, b) =>
                {
                    int cmp = rentedPrimary[a].CompareTo(rentedPrimary[b]);
                    if (cmp != 0)
                    {
                        return cmp;
                    }

                    return rentedSecondary[a].CompareTo(rentedSecondary[b]);
                }));

            rentedIndices.AsSpan(0, length).CopyTo(indices);
        }
        finally
        {
            ArrayPool<double>.Shared.Return(rentedSecondary);
            ArrayPool<double>.Shared.Return(rentedPrimary);
            ArrayPool<int>.Shared.Return(rentedIndices);
        }
    }

    public static int[] Lexsort(ReadOnlySpan<float> primaryKey, ReadOnlySpan<float> secondaryKey)
    {
        int[] indices = new int[primaryKey.Length];
        Lexsort(primaryKey, secondaryKey, indices);
        return indices;
    }

    public static void Lexsort(ReadOnlySpan<float> primaryKey, ReadOnlySpan<float> secondaryKey, Span<int> indices)
    {
        if (primaryKey.Length != secondaryKey.Length || primaryKey.Length != indices.Length)
        {
            throw new ArgumentException("Keys and indices lengths must match.");
        }

        int length = indices.Length;
        int[] rentedIndices = ArrayPool<int>.Shared.Rent(length);
        float[] rentedPrimary = ArrayPool<float>.Shared.Rent(length);
        float[] rentedSecondary = ArrayPool<float>.Shared.Rent(length);

        try
        {
            for (int i = 0; i < length; i++)
            {
                rentedIndices[i] = i;
                rentedPrimary[i] = primaryKey[i];
                rentedSecondary[i] = secondaryKey[i];
            }

            Array.Sort(
                rentedIndices,
                0,
                length,
                Comparer<int>.Create((a, b) =>
                {
                    int cmp = rentedPrimary[a].CompareTo(rentedPrimary[b]);
                    if (cmp != 0)
                    {
                        return cmp;
                    }

                    return rentedSecondary[a].CompareTo(rentedSecondary[b]);
                }));

            rentedIndices.AsSpan(0, length).CopyTo(indices);
        }
        finally
        {
            ArrayPool<float>.Shared.Return(rentedSecondary);
            ArrayPool<float>.Shared.Return(rentedPrimary);
            ArrayPool<int>.Shared.Return(rentedIndices);
        }
    }

    public static int[] Lexsort(ReadOnlySpan<Complex> primaryKey, ReadOnlySpan<Complex> secondaryKey)
    {
        int[] indices = new int[primaryKey.Length];
        Lexsort(primaryKey, secondaryKey, indices);
        return indices;
    }

    public static void Lexsort(ReadOnlySpan<Complex> primaryKey, ReadOnlySpan<Complex> secondaryKey, Span<int> indices)
    {
        if (primaryKey.Length != secondaryKey.Length || primaryKey.Length != indices.Length)
        {
            throw new ArgumentException("Keys and indices lengths must match.");
        }

        int length = indices.Length;
        int[] rentedIndices = ArrayPool<int>.Shared.Rent(length);
        Complex[] rentedPrimary = ArrayPool<Complex>.Shared.Rent(length);
        Complex[] rentedSecondary = ArrayPool<Complex>.Shared.Rent(length);

        try
        {
            for (int i = 0; i < length; i++)
            {
                rentedIndices[i] = i;
                rentedPrimary[i] = primaryKey[i];
                rentedSecondary[i] = secondaryKey[i];
            }

            Array.Sort(
                rentedIndices,
                0,
                length,
                Comparer<int>.Create((a, b) =>
                {
                    int cmp = rentedPrimary[a].Magnitude.CompareTo(rentedPrimary[b].Magnitude);
                    if (cmp != 0)
                    {
                        return cmp;
                    }

                    return rentedSecondary[a].Magnitude.CompareTo(rentedSecondary[b].Magnitude);
                }));

            rentedIndices.AsSpan(0, length).CopyTo(indices);
        }
        finally
        {
            ArrayPool<Complex>.Shared.Return(rentedSecondary);
            ArrayPool<Complex>.Shared.Return(rentedPrimary);
            ArrayPool<int>.Shared.Return(rentedIndices);
        }
    }
}

