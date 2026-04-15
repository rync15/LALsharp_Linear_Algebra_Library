using LAL.Core;
using System.Numerics;

namespace LAL.LinalgCore;

internal static class Axpy
{
    public static void Compute(double alpha, ReadOnlySpan<double> x, Span<double> y)
    {
        if (x.Length != y.Length)
        {
            throw new ArgumentException("Input vectors must have the same length.");
        }

        DataStructurePerformanceSettings settings = DataStructureCompatibility.GetPerformanceSettings();
        PerformancePrimitives.Axpy(alpha, x, y, settings);
    }

    public static void Compute(float alpha, ReadOnlySpan<float> x, Span<float> y)
    {
        if (x.Length != y.Length)
        {
            throw new ArgumentException("Input vectors must have the same length.");
        }

        DataStructurePerformanceSettings settings = DataStructureCompatibility.GetPerformanceSettings();
        PerformancePrimitives.Axpy(alpha, x, y, settings);
    }

    public static void Compute(Complex alpha, ReadOnlySpan<Complex> x, Span<Complex> y)
    {
        if (x.Length != y.Length)
        {
            throw new ArgumentException("Input vectors must have the same length.");
        }

        for (int i = 0; i < y.Length; i++)
        {
            y[i] = alpha * x[i] + y[i];
        }
    }
}

