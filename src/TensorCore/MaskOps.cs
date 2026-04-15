using System.Numerics;

namespace LAL.TensorCore;

internal static class MaskOps
{
    public static void GreaterThan(ReadOnlySpan<double> values, double threshold, Span<bool> mask)
    {
        Validate(values.Length, mask.Length);
        for (int i = 0; i < values.Length; i++)
        {
            mask[i] = values[i] > threshold;
        }
    }

    public static void LessThan(ReadOnlySpan<double> values, double threshold, Span<bool> mask)
    {
        Validate(values.Length, mask.Length);
        for (int i = 0; i < values.Length; i++)
        {
            mask[i] = values[i] < threshold;
        }
    }

    public static void Equal(ReadOnlySpan<double> values, double target, Span<bool> mask, double epsilon = 1e-12)
    {
        Validate(values.Length, mask.Length);
        for (int i = 0; i < values.Length; i++)
        {
            mask[i] = Math.Abs(values[i] - target) <= epsilon;
        }
    }

    public static void Where(ReadOnlySpan<double> values, ReadOnlySpan<bool> mask, Span<double> destination, double fallback = 0d)
    {
        if (values.Length != mask.Length || values.Length != destination.Length)
        {
            throw new ArgumentException("Values, mask and destination lengths must match.");
        }

        for (int i = 0; i < values.Length; i++)
        {
            destination[i] = mask[i] ? values[i] : fallback;
        }
    }

    public static void GreaterThan(ReadOnlySpan<float> values, float threshold, Span<bool> mask)
    {
        Validate(values.Length, mask.Length);
        for (int i = 0; i < values.Length; i++)
        {
            mask[i] = values[i] > threshold;
        }
    }

    public static void LessThan(ReadOnlySpan<float> values, float threshold, Span<bool> mask)
    {
        Validate(values.Length, mask.Length);
        for (int i = 0; i < values.Length; i++)
        {
            mask[i] = values[i] < threshold;
        }
    }

    public static void Equal(ReadOnlySpan<float> values, float target, Span<bool> mask, float epsilon = 1e-6f)
    {
        Validate(values.Length, mask.Length);
        for (int i = 0; i < values.Length; i++)
        {
            mask[i] = MathF.Abs(values[i] - target) <= epsilon;
        }
    }

    public static void Where(ReadOnlySpan<float> values, ReadOnlySpan<bool> mask, Span<float> destination, float fallback = 0f)
    {
        if (values.Length != mask.Length || values.Length != destination.Length)
        {
            throw new ArgumentException("Values, mask and destination lengths must match.");
        }

        for (int i = 0; i < values.Length; i++)
        {
            destination[i] = mask[i] ? values[i] : fallback;
        }
    }

    public static void GreaterThan(ReadOnlySpan<Complex> values, double threshold, Span<bool> mask)
    {
        Validate(values.Length, mask.Length);
        for (int i = 0; i < values.Length; i++)
        {
            mask[i] = values[i].Magnitude > threshold;
        }
    }

    public static void LessThan(ReadOnlySpan<Complex> values, double threshold, Span<bool> mask)
    {
        Validate(values.Length, mask.Length);
        for (int i = 0; i < values.Length; i++)
        {
            mask[i] = values[i].Magnitude < threshold;
        }
    }

    public static void Equal(ReadOnlySpan<Complex> values, Complex target, Span<bool> mask, double epsilon = 1e-12)
    {
        Validate(values.Length, mask.Length);
        for (int i = 0; i < values.Length; i++)
        {
            mask[i] = Complex.Abs(values[i] - target) <= epsilon;
        }
    }

    public static void Where(ReadOnlySpan<Complex> values, ReadOnlySpan<bool> mask, Span<Complex> destination, Complex fallback)
    {
        if (values.Length != mask.Length || values.Length != destination.Length)
        {
            throw new ArgumentException("Values, mask and destination lengths must match.");
        }

        for (int i = 0; i < values.Length; i++)
        {
            destination[i] = mask[i] ? values[i] : fallback;
        }
    }

    public static void Where(ReadOnlySpan<Complex> values, ReadOnlySpan<bool> mask, Span<Complex> destination)
    {
        Where(values, mask, destination, Complex.Zero);
    }

    private static void Validate(int valuesLength, int maskLength)
    {
        if (valuesLength != maskLength)
        {
            throw new ArgumentException("Values and mask lengths must match.");
        }
    }
}

