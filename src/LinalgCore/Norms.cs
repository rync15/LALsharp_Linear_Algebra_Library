using System.Numerics;

namespace LAL.LinalgCore;

internal static class Norms
{
    public static double L1(ReadOnlySpan<double> values)
    {
        double sum = 0d;
        for (int i = 0; i < values.Length; i++)
        {
            sum += Math.Abs(values[i]);
        }

        return sum;
    }

    public static double L2Squared(ReadOnlySpan<double> values)
    {
        double sum = 0d;
        for (int i = 0; i < values.Length; i++)
        {
            double v = values[i];
            sum += v * v;
        }

        return sum;
    }

    public static double L2(ReadOnlySpan<double> values)
    {
        return Math.Sqrt(L2Squared(values));
    }

    public static double Infinity(ReadOnlySpan<double> values)
    {
        double max = 0d;
        for (int i = 0; i < values.Length; i++)
        {
            double abs = Math.Abs(values[i]);
            if (abs > max)
            {
                max = abs;
            }
        }

        return max;
    }

    public static float L1(ReadOnlySpan<float> values)
    {
        float sum = 0f;
        for (int i = 0; i < values.Length; i++)
        {
            sum += MathF.Abs(values[i]);
        }

        return sum;
    }

    public static float L2Squared(ReadOnlySpan<float> values)
    {
        float sum = 0f;
        for (int i = 0; i < values.Length; i++)
        {
            float v = values[i];
            sum += v * v;
        }

        return sum;
    }

    public static float L2(ReadOnlySpan<float> values)
    {
        return MathF.Sqrt(L2Squared(values));
    }

    public static float Infinity(ReadOnlySpan<float> values)
    {
        float max = 0f;
        for (int i = 0; i < values.Length; i++)
        {
            float abs = MathF.Abs(values[i]);
            if (abs > max)
            {
                max = abs;
            }
        }

        return max;
    }

    public static double L1(ReadOnlySpan<Complex> values)
    {
        double sum = 0d;
        for (int i = 0; i < values.Length; i++)
        {
            sum += values[i].Magnitude;
        }

        return sum;
    }

    public static double L2Squared(ReadOnlySpan<Complex> values)
    {
        double sum = 0d;
        for (int i = 0; i < values.Length; i++)
        {
            double m = values[i].Magnitude;
            sum += m * m;
        }

        return sum;
    }

    public static double L2(ReadOnlySpan<Complex> values)
    {
        return Math.Sqrt(L2Squared(values));
    }

    public static double Infinity(ReadOnlySpan<Complex> values)
    {
        double max = 0d;
        for (int i = 0; i < values.Length; i++)
        {
            double abs = values[i].Magnitude;
            if (abs > max)
            {
                max = abs;
            }
        }

        return max;
    }
}

