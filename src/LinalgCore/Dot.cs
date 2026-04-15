using LAL.Core;
using System.Numerics;

namespace LAL.LinalgCore;

internal static class Dot
{
    public static double Dotu(ReadOnlySpan<double> left, ReadOnlySpan<double> right)
    {
        if (left.Length != right.Length)
        {
            throw new ArgumentException("Vectors must have the same length.");
        }

        DataStructurePerformanceSettings settings = DataStructureCompatibility.GetPerformanceSettings();
        return PerformancePrimitives.Dot(left, right, settings);
    }

    public static float Dotu(ReadOnlySpan<float> left, ReadOnlySpan<float> right)
    {
        if (left.Length != right.Length)
        {
            throw new ArgumentException("Vectors must have the same length.");
        }

        DataStructurePerformanceSettings settings = DataStructureCompatibility.GetPerformanceSettings();
        return PerformancePrimitives.Dot(left, right, settings);
    }

    public static Complex Dotu(ReadOnlySpan<Complex> left, ReadOnlySpan<Complex> right)
    {
        if (left.Length != right.Length)
        {
            throw new ArgumentException("Vectors must have the same length.");
        }

        Complex sum = Complex.Zero;
        for (int i = 0; i < left.Length; i++)
        {
            sum += left[i] * right[i];
        }

        return sum;
    }

    public static Complex Dotc(ReadOnlySpan<Complex> left, ReadOnlySpan<Complex> right)
    {
        if (left.Length != right.Length)
        {
            throw new ArgumentException("Vectors must have the same length.");
        }

        Complex sum = Complex.Zero;
        for (int i = 0; i < left.Length; i++)
        {
            sum += Complex.Conjugate(left[i]) * right[i];
        }

        return sum;
    }
}

