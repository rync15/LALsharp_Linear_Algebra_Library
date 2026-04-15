using System.Buffers;
using System.Numerics;

namespace LAL.TensorCore;

internal static class Reductions
{
    private const int StackallocQuantileThreshold = 256;

    public static double Sum(ReadOnlySpan<double> values)
    {
        double sum = 0d;
        double c = 0d;

        for (int i = 0; i < values.Length; i++)
        {
            double y = values[i] - c;
            double t = sum + y;
            c = (t - sum) - y;
            sum = t;
        }

        return sum;
    }

    public static double Mean(ReadOnlySpan<double> values)
    {
        if (values.Length == 0)
        {
            throw new ArgumentException("Values must not be empty.", nameof(values));
        }

        return Sum(values) / values.Length;
    }

    public static double Variance(ReadOnlySpan<double> values, bool sample = false)
    {
        if (values.Length == 0)
        {
            throw new ArgumentException("Values must not be empty.", nameof(values));
        }

        if (sample && values.Length < 2)
        {
            throw new ArgumentException("Sample variance requires at least two values.", nameof(values));
        }

        double mean = Mean(values);
        double acc = 0d;
        double c = 0d;
        for (int i = 0; i < values.Length; i++)
        {
            double d = values[i] - mean;
            double term = (d * d) - c;
            double t = acc + term;
            c = (t - acc) - term;
            acc = t;
        }

        return sample ? acc / (values.Length - 1) : acc / values.Length;
    }

    public static double Max(ReadOnlySpan<double> values)
    {
        if (values.Length == 0)
        {
            throw new ArgumentException("Values must not be empty.", nameof(values));
        }

        double max = values[0];
        if (double.IsNaN(max))
        {
            return double.NaN;
        }

        for (int i = 1; i < values.Length; i++)
        {
            double v = values[i];
            if (double.IsNaN(v))
            {
                return double.NaN;
            }

            if (v > max)
            {
                max = v;
            }
        }

        return max;
    }

    public static double Min(ReadOnlySpan<double> values)
    {
        if (values.Length == 0)
        {
            throw new ArgumentException("Values must not be empty.", nameof(values));
        }

        double min = values[0];
        if (double.IsNaN(min))
        {
            return double.NaN;
        }

        for (int i = 1; i < values.Length; i++)
        {
            double v = values[i];
            if (double.IsNaN(v))
            {
                return double.NaN;
            }

            if (v < min)
            {
                min = v;
            }
        }

        return min;
    }

    public static double Quantile(ReadOnlySpan<double> values, double q)
    {
        if (values.Length == 0)
        {
            throw new ArgumentException("Values must not be empty.", nameof(values));
        }

        if (q < 0d || q > 1d)
        {
            throw new ArgumentOutOfRangeException(nameof(q), "Quantile q must be in [0, 1].");
        }

        for (int i = 0; i < values.Length; i++)
        {
            if (double.IsNaN(values[i]))
            {
                return double.NaN;
            }
        }

        if (values.Length <= StackallocQuantileThreshold)
        {
            Span<double> sorted = stackalloc double[values.Length];
            values.CopyTo(sorted);
            sorted.Sort();
            return InterpolateQuantile(sorted, q);
        }

        double[] rented = ArrayPool<double>.Shared.Rent(values.Length);
        try
        {
            Span<double> sorted = rented.AsSpan(0, values.Length);
            values.CopyTo(sorted);
            sorted.Sort();
            return InterpolateQuantile(sorted, q);
        }
        finally
        {
            ArrayPool<double>.Shared.Return(rented);
        }
    }

    public static double Median(ReadOnlySpan<double> values)
    {
        return Quantile(values, 0.5d);
    }

    public static double SumNanSafe(ReadOnlySpan<double> values)
    {
        double sum = 0d;
        double c = 0d;

        for (int i = 0; i < values.Length; i++)
        {
            double v = values[i];
            if (double.IsNaN(v))
            {
                continue;
            }

            double y = v - c;
            double t = sum + y;
            c = (t - sum) - y;
            sum = t;
        }

        return sum;
    }

    public static double MeanNanSafe(ReadOnlySpan<double> values)
    {
        int count = 0;
        double sum = 0d;
        double c = 0d;

        for (int i = 0; i < values.Length; i++)
        {
            double v = values[i];
            if (double.IsNaN(v))
            {
                continue;
            }

            double y = v - c;
            double t = sum + y;
            c = (t - sum) - y;
            sum = t;
            count++;
        }

        if (count == 0)
        {
            return double.NaN;
        }

        return sum / count;
    }

    public static double VarianceNanSafe(ReadOnlySpan<double> values, bool sample = false)
    {
        int count = 0;
        for (int i = 0; i < values.Length; i++)
        {
            if (!double.IsNaN(values[i]))
            {
                count++;
            }
        }

        if (count == 0)
        {
            return double.NaN;
        }

        if (sample && count < 2)
        {
            return double.NaN;
        }

        double mean = MeanNanSafe(values);
        double acc = 0d;
        double c = 0d;
        int used = 0;

        for (int i = 0; i < values.Length; i++)
        {
            double v = values[i];
            if (double.IsNaN(v))
            {
                continue;
            }

            double d = v - mean;
            double term = (d * d) - c;
            double t = acc + term;
            c = (t - acc) - term;
            acc = t;
            used++;
        }

        return sample ? acc / (used - 1) : acc / used;
    }

    public static double MaxNanSafe(ReadOnlySpan<double> values)
    {
        bool hasValue = false;
        double max = double.NegativeInfinity;

        for (int i = 0; i < values.Length; i++)
        {
            double v = values[i];
            if (double.IsNaN(v))
            {
                continue;
            }

            if (!hasValue || v > max)
            {
                max = v;
                hasValue = true;
            }
        }

        return hasValue ? max : double.NaN;
    }

    public static double MinNanSafe(ReadOnlySpan<double> values)
    {
        bool hasValue = false;
        double min = double.PositiveInfinity;

        for (int i = 0; i < values.Length; i++)
        {
            double v = values[i];
            if (double.IsNaN(v))
            {
                continue;
            }

            if (!hasValue || v < min)
            {
                min = v;
                hasValue = true;
            }
        }

        return hasValue ? min : double.NaN;
    }

    public static float Sum(ReadOnlySpan<float> values)
    {
        float sum = 0f;
        float c = 0f;

        for (int i = 0; i < values.Length; i++)
        {
            float y = values[i] - c;
            float t = sum + y;
            c = (t - sum) - y;
            sum = t;
        }

        return sum;
    }

    public static float Mean(ReadOnlySpan<float> values)
    {
        if (values.Length == 0)
        {
            throw new ArgumentException("Values must not be empty.", nameof(values));
        }

        return Sum(values) / values.Length;
    }

    public static float Variance(ReadOnlySpan<float> values, bool sample = false)
    {
        if (values.Length == 0)
        {
            throw new ArgumentException("Values must not be empty.", nameof(values));
        }

        if (sample && values.Length < 2)
        {
            throw new ArgumentException("Sample variance requires at least two values.", nameof(values));
        }

        float mean = Mean(values);
        float acc = 0f;
        float c = 0f;
        for (int i = 0; i < values.Length; i++)
        {
            float d = values[i] - mean;
            float term = (d * d) - c;
            float t = acc + term;
            c = (t - acc) - term;
            acc = t;
        }

        return sample ? acc / (values.Length - 1) : acc / values.Length;
    }

    public static float Max(ReadOnlySpan<float> values)
    {
        if (values.Length == 0)
        {
            throw new ArgumentException("Values must not be empty.", nameof(values));
        }

        float max = values[0];
        if (float.IsNaN(max))
        {
            return float.NaN;
        }

        for (int i = 1; i < values.Length; i++)
        {
            float v = values[i];
            if (float.IsNaN(v))
            {
                return float.NaN;
            }

            if (v > max)
            {
                max = v;
            }
        }

        return max;
    }

    public static float Min(ReadOnlySpan<float> values)
    {
        if (values.Length == 0)
        {
            throw new ArgumentException("Values must not be empty.", nameof(values));
        }

        float min = values[0];
        if (float.IsNaN(min))
        {
            return float.NaN;
        }

        for (int i = 1; i < values.Length; i++)
        {
            float v = values[i];
            if (float.IsNaN(v))
            {
                return float.NaN;
            }

            if (v < min)
            {
                min = v;
            }
        }

        return min;
    }

    public static float Quantile(ReadOnlySpan<float> values, float q)
    {
        if (values.Length == 0)
        {
            throw new ArgumentException("Values must not be empty.", nameof(values));
        }

        if (q < 0f || q > 1f)
        {
            throw new ArgumentOutOfRangeException(nameof(q), "Quantile q must be in [0, 1].");
        }

        for (int i = 0; i < values.Length; i++)
        {
            if (float.IsNaN(values[i]))
            {
                return float.NaN;
            }
        }

        if (values.Length <= StackallocQuantileThreshold)
        {
            Span<float> sorted = stackalloc float[values.Length];
            values.CopyTo(sorted);
            sorted.Sort();
            return InterpolateQuantile(sorted, q);
        }

        float[] rented = ArrayPool<float>.Shared.Rent(values.Length);
        try
        {
            Span<float> sorted = rented.AsSpan(0, values.Length);
            values.CopyTo(sorted);
            sorted.Sort();
            return InterpolateQuantile(sorted, q);
        }
        finally
        {
            ArrayPool<float>.Shared.Return(rented);
        }
    }

    public static float Median(ReadOnlySpan<float> values)
    {
        return Quantile(values, 0.5f);
    }

    public static float SumNanSafe(ReadOnlySpan<float> values)
    {
        float sum = 0f;
        float c = 0f;

        for (int i = 0; i < values.Length; i++)
        {
            float v = values[i];
            if (float.IsNaN(v))
            {
                continue;
            }

            float y = v - c;
            float t = sum + y;
            c = (t - sum) - y;
            sum = t;
        }

        return sum;
    }

    public static float MeanNanSafe(ReadOnlySpan<float> values)
    {
        int count = 0;
        float sum = 0f;
        float c = 0f;

        for (int i = 0; i < values.Length; i++)
        {
            float v = values[i];
            if (float.IsNaN(v))
            {
                continue;
            }

            float y = v - c;
            float t = sum + y;
            c = (t - sum) - y;
            sum = t;
            count++;
        }

        if (count == 0)
        {
            return float.NaN;
        }

        return sum / count;
    }

    public static float VarianceNanSafe(ReadOnlySpan<float> values, bool sample = false)
    {
        int count = 0;
        for (int i = 0; i < values.Length; i++)
        {
            if (!float.IsNaN(values[i]))
            {
                count++;
            }
        }

        if (count == 0)
        {
            return float.NaN;
        }

        if (sample && count < 2)
        {
            return float.NaN;
        }

        float mean = MeanNanSafe(values);
        float acc = 0f;
        float c = 0f;
        int used = 0;

        for (int i = 0; i < values.Length; i++)
        {
            float v = values[i];
            if (float.IsNaN(v))
            {
                continue;
            }

            float d = v - mean;
            float term = (d * d) - c;
            float t = acc + term;
            c = (t - acc) - term;
            acc = t;
            used++;
        }

        return sample ? acc / (used - 1) : acc / used;
    }

    public static float MaxNanSafe(ReadOnlySpan<float> values)
    {
        bool hasValue = false;
        float max = float.NegativeInfinity;

        for (int i = 0; i < values.Length; i++)
        {
            float v = values[i];
            if (float.IsNaN(v))
            {
                continue;
            }

            if (!hasValue || v > max)
            {
                max = v;
                hasValue = true;
            }
        }

        return hasValue ? max : float.NaN;
    }

    public static float MinNanSafe(ReadOnlySpan<float> values)
    {
        bool hasValue = false;
        float min = float.PositiveInfinity;

        for (int i = 0; i < values.Length; i++)
        {
            float v = values[i];
            if (float.IsNaN(v))
            {
                continue;
            }

            if (!hasValue || v < min)
            {
                min = v;
                hasValue = true;
            }
        }

        return hasValue ? min : float.NaN;
    }

    public static Complex Sum(ReadOnlySpan<Complex> values)
    {
        Complex sum = Complex.Zero;
        for (int i = 0; i < values.Length; i++)
        {
            sum += values[i];
        }

        return sum;
    }

    public static Complex Mean(ReadOnlySpan<Complex> values)
    {
        if (values.Length == 0)
        {
            throw new ArgumentException("Values must not be empty.", nameof(values));
        }

        return Sum(values) / values.Length;
    }

    public static Complex SumNanSafe(ReadOnlySpan<Complex> values)
    {
        Complex sum = Complex.Zero;
        for (int i = 0; i < values.Length; i++)
        {
            if (IsComplexNan(values[i]))
            {
                continue;
            }

            sum += values[i];
        }

        return sum;
    }

    public static Complex MeanNanSafe(ReadOnlySpan<Complex> values)
    {
        int count = 0;
        Complex sum = Complex.Zero;

        for (int i = 0; i < values.Length; i++)
        {
            if (IsComplexNan(values[i]))
            {
                continue;
            }

            sum += values[i];
            count++;
        }

        return count == 0 ? new Complex(double.NaN, double.NaN) : sum / count;
    }

    private static bool IsComplexNan(Complex value)
    {
        return double.IsNaN(value.Real) || double.IsNaN(value.Imaginary);
    }

    private static double InterpolateQuantile(ReadOnlySpan<double> sorted, double q)
    {
        if (sorted.Length == 1)
        {
            return sorted[0];
        }

        double position = (sorted.Length - 1) * q;
        int lower = (int)Math.Floor(position);
        int upper = (int)Math.Ceiling(position);

        if (lower == upper)
        {
            return sorted[lower];
        }

        double weight = position - lower;
        return sorted[lower] + ((sorted[upper] - sorted[lower]) * weight);
    }

    private static float InterpolateQuantile(ReadOnlySpan<float> sorted, float q)
    {
        if (sorted.Length == 1)
        {
            return sorted[0];
        }

        float position = (sorted.Length - 1) * q;
        int lower = (int)MathF.Floor(position);
        int upper = (int)MathF.Ceiling(position);

        if (lower == upper)
        {
            return sorted[lower];
        }

        float weight = position - lower;
        return sorted[lower] + ((sorted[upper] - sorted[lower]) * weight);
    }
}

