using System.Buffers;
using LAL.Core;

namespace LAL.OdeCore;

public delegate void OdeSystem(double t, ReadOnlySpan<double> y, Span<double> dydt);
public delegate void OdeSystemFloat(float t, ReadOnlySpan<float> y, Span<float> dydt);
public delegate void OdeSystemComplex(double t, ReadOnlySpan<System.Numerics.Complex> y, Span<System.Numerics.Complex> dydt);

internal static class Euler
{
    public static void Step(double t, double dt, ReadOnlySpan<double> y, Span<double> yOut, OdeSystem system)
    {
        if (system is null)
        {
            throw new ArgumentNullException(nameof(system));
        }

        if (y.Length != yOut.Length)
        {
            throw new ArgumentException("Input and output vectors must have the same length.");
        }

        double[]? rented = null;
        Span<double> dydt = y.Length <= 256
            ? stackalloc double[y.Length]
            : (rented = ArrayPool<double>.Shared.Rent(y.Length)).AsSpan(0, y.Length);

        try
        {
            system(t, y, dydt);
            Integrate(y, dydt, dt, yOut);
        }
        finally
        {
            if (rented is not null)
            {
                ArrayPool<double>.Shared.Return(rented, clearArray: false);
            }
        }
    }

    public static void Step(float t, float dt, ReadOnlySpan<float> y, Span<float> yOut, OdeSystemFloat system)
    {
        if (system is null)
        {
            throw new ArgumentNullException(nameof(system));
        }

        if (y.Length != yOut.Length)
        {
            throw new ArgumentException("Input and output vectors must have the same length.");
        }

        float[]? rented = null;
        Span<float> dydt = y.Length <= 256
            ? stackalloc float[y.Length]
            : (rented = ArrayPool<float>.Shared.Rent(y.Length)).AsSpan(0, y.Length);

        try
        {
            system(t, y, dydt);
            Integrate(y, dydt, dt, yOut);
        }
        finally
        {
            if (rented is not null)
            {
                ArrayPool<float>.Shared.Return(rented, clearArray: false);
            }
        }
    }

    public static void Step(
        double t,
        double dt,
        ReadOnlySpan<System.Numerics.Complex> y,
        Span<System.Numerics.Complex> yOut,
        OdeSystemComplex system)
    {
        if (system is null)
        {
            throw new ArgumentNullException(nameof(system));
        }

        if (y.Length != yOut.Length)
        {
            throw new ArgumentException("Input and output vectors must have the same length.");
        }

        System.Numerics.Complex[]? rented = null;
        Span<System.Numerics.Complex> dydt = y.Length <= 256
            ? stackalloc System.Numerics.Complex[y.Length]
            : (rented = ArrayPool<System.Numerics.Complex>.Shared.Rent(y.Length)).AsSpan(0, y.Length);

        try
        {
            system(t, y, dydt);

            for (int i = 0; i < y.Length; i++)
            {
                yOut[i] = y[i] + (dt * dydt[i]);
            }
        }
        finally
        {
            if (rented is not null)
            {
                ArrayPool<System.Numerics.Complex>.Shared.Return(rented, clearArray: false);
            }
        }
    }

    private static void Integrate(ReadOnlySpan<double> y, ReadOnlySpan<double> dydt, double dt, Span<double> yOut)
    {
        DataStructurePerformanceSettings settings = DataStructureCompatibility.GetPerformanceSettings();
        PerformancePrimitives.ScaledAdd(y, dydt, dt, yOut, settings);
    }

    private static void Integrate(ReadOnlySpan<float> y, ReadOnlySpan<float> dydt, float dt, Span<float> yOut)
    {
        DataStructurePerformanceSettings settings = DataStructureCompatibility.GetPerformanceSettings();
        PerformancePrimitives.ScaledAdd(y, dydt, dt, yOut, settings);
    }
}

