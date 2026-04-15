using System.Buffers;
using System.Numerics;

namespace LAL.OdeCore;

internal static class Rk4
{
    private const int StackallocDimensionThreshold = 64;

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

        int n = y.Length;
        double[]? rented = null;
        Span<double> workspace = n <= StackallocDimensionThreshold
            ? stackalloc double[n * 5]
            : (rented = ArrayPool<double>.Shared.Rent(n * 5)).AsSpan(0, n * 5);

        Span<double> k1 = workspace.Slice(0, n);
        Span<double> k2 = workspace.Slice(n, n);
        Span<double> k3 = workspace.Slice(n * 2, n);
        Span<double> k4 = workspace.Slice(n * 3, n);
        Span<double> temp = workspace.Slice(n * 4, n);

        try
        {
            system(t, y, k1);

            AddScaled(y, dt * 0.5, k1, temp);
            system(t + dt * 0.5, temp, k2);

            AddScaled(y, dt * 0.5, k2, temp);
            system(t + dt * 0.5, temp, k3);

            AddScaled(y, dt, k3, temp);
            system(t + dt, temp, k4);

            CombineRk4(y, dt / 6.0, k1, k2, k3, k4, yOut);
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

        int n = y.Length;
        float[]? rented = null;
        Span<float> workspace = n <= StackallocDimensionThreshold
            ? stackalloc float[n * 5]
            : (rented = ArrayPool<float>.Shared.Rent(n * 5)).AsSpan(0, n * 5);

        Span<float> k1 = workspace.Slice(0, n);
        Span<float> k2 = workspace.Slice(n, n);
        Span<float> k3 = workspace.Slice(n * 2, n);
        Span<float> k4 = workspace.Slice(n * 3, n);
        Span<float> temp = workspace.Slice(n * 4, n);

        try
        {
            system(t, y, k1);

            AddScaled(y, dt * 0.5f, k1, temp);
            system(t + (dt * 0.5f), temp, k2);

            AddScaled(y, dt * 0.5f, k2, temp);
            system(t + (dt * 0.5f), temp, k3);

            AddScaled(y, dt, k3, temp);
            system(t + dt, temp, k4);

            CombineRk4(y, dt / 6f, k1, k2, k3, k4, yOut);
        }
        finally
        {
            if (rented is not null)
            {
                ArrayPool<float>.Shared.Return(rented, clearArray: false);
            }
        }
    }

    public static void Step(double t, double dt, ReadOnlySpan<Complex> y, Span<Complex> yOut, OdeSystemComplex system)
    {
        if (system is null)
        {
            throw new ArgumentNullException(nameof(system));
        }

        if (y.Length != yOut.Length)
        {
            throw new ArgumentException("Input and output vectors must have the same length.");
        }

        int n = y.Length;
        Complex[]? rented = null;
        Span<Complex> workspace = n <= StackallocDimensionThreshold
            ? stackalloc Complex[n * 5]
            : (rented = ArrayPool<Complex>.Shared.Rent(n * 5)).AsSpan(0, n * 5);

        Span<Complex> k1 = workspace.Slice(0, n);
        Span<Complex> k2 = workspace.Slice(n, n);
        Span<Complex> k3 = workspace.Slice(n * 2, n);
        Span<Complex> k4 = workspace.Slice(n * 3, n);
        Span<Complex> temp = workspace.Slice(n * 4, n);

        try
        {
            system(t, y, k1);

            for (int i = 0; i < n; i++)
            {
                temp[i] = y[i] + ((dt * 0.5) * k1[i]);
            }
            system(t + (dt * 0.5), temp, k2);

            for (int i = 0; i < n; i++)
            {
                temp[i] = y[i] + ((dt * 0.5) * k2[i]);
            }
            system(t + (dt * 0.5), temp, k3);

            for (int i = 0; i < n; i++)
            {
                temp[i] = y[i] + (dt * k3[i]);
            }
            system(t + dt, temp, k4);

            for (int i = 0; i < n; i++)
            {
                yOut[i] = y[i] + ((dt / 6.0) * (k1[i] + (2.0 * k2[i]) + (2.0 * k3[i]) + k4[i]));
            }
        }
        finally
        {
            if (rented is not null)
            {
                ArrayPool<Complex>.Shared.Return(rented, clearArray: false);
            }
        }
    }

    private static void AddScaled(ReadOnlySpan<double> y, double scale, ReadOnlySpan<double> k, Span<double> destination)
    {
        int i = 0;
        if (Vector.IsHardwareAccelerated && y.Length >= (Vector<double>.Count * 2))
        {
            int width = Vector<double>.Count;
            int end = y.Length - width;
            Vector<double> scaleVec = new(scale);

            for (; i <= end; i += width)
            {
                Vector<double> yVec = new(y.Slice(i, width));
                Vector<double> kVec = new(k.Slice(i, width));
                (yVec + (scaleVec * kVec)).CopyTo(destination.Slice(i, width));
            }
        }

        for (; i < y.Length; i++)
        {
            destination[i] = y[i] + (scale * k[i]);
        }
    }

    private static void AddScaled(ReadOnlySpan<float> y, float scale, ReadOnlySpan<float> k, Span<float> destination)
    {
        int i = 0;
        if (Vector.IsHardwareAccelerated && y.Length >= (Vector<float>.Count * 2))
        {
            int width = Vector<float>.Count;
            int end = y.Length - width;
            Vector<float> scaleVec = new(scale);

            for (; i <= end; i += width)
            {
                Vector<float> yVec = new(y.Slice(i, width));
                Vector<float> kVec = new(k.Slice(i, width));
                (yVec + (scaleVec * kVec)).CopyTo(destination.Slice(i, width));
            }
        }

        for (; i < y.Length; i++)
        {
            destination[i] = y[i] + (scale * k[i]);
        }
    }

    private static void CombineRk4(
        ReadOnlySpan<double> y,
        double scale,
        ReadOnlySpan<double> k1,
        ReadOnlySpan<double> k2,
        ReadOnlySpan<double> k3,
        ReadOnlySpan<double> k4,
        Span<double> destination)
    {
        int i = 0;
        if (Vector.IsHardwareAccelerated && y.Length >= (Vector<double>.Count * 2))
        {
            int width = Vector<double>.Count;
            int end = y.Length - width;
            Vector<double> scaleVec = new(scale);
            Vector<double> two = new(2.0);

            for (; i <= end; i += width)
            {
                Vector<double> yVec = new(y.Slice(i, width));
                Vector<double> k1Vec = new(k1.Slice(i, width));
                Vector<double> k2Vec = new(k2.Slice(i, width));
                Vector<double> k3Vec = new(k3.Slice(i, width));
                Vector<double> k4Vec = new(k4.Slice(i, width));
                Vector<double> sum = k1Vec + (two * k2Vec) + (two * k3Vec) + k4Vec;
                (yVec + (scaleVec * sum)).CopyTo(destination.Slice(i, width));
            }
        }

        for (; i < y.Length; i++)
        {
            destination[i] = y[i] + (scale * (k1[i] + (2.0 * k2[i]) + (2.0 * k3[i]) + k4[i]));
        }
    }

    private static void CombineRk4(
        ReadOnlySpan<float> y,
        float scale,
        ReadOnlySpan<float> k1,
        ReadOnlySpan<float> k2,
        ReadOnlySpan<float> k3,
        ReadOnlySpan<float> k4,
        Span<float> destination)
    {
        int i = 0;
        if (Vector.IsHardwareAccelerated && y.Length >= (Vector<float>.Count * 2))
        {
            int width = Vector<float>.Count;
            int end = y.Length - width;
            Vector<float> scaleVec = new(scale);
            Vector<float> two = new(2f);

            for (; i <= end; i += width)
            {
                Vector<float> yVec = new(y.Slice(i, width));
                Vector<float> k1Vec = new(k1.Slice(i, width));
                Vector<float> k2Vec = new(k2.Slice(i, width));
                Vector<float> k3Vec = new(k3.Slice(i, width));
                Vector<float> k4Vec = new(k4.Slice(i, width));
                Vector<float> sum = k1Vec + (two * k2Vec) + (two * k3Vec) + k4Vec;
                (yVec + (scaleVec * sum)).CopyTo(destination.Slice(i, width));
            }
        }

        for (; i < y.Length; i++)
        {
            destination[i] = y[i] + (scale * (k1[i] + (2f * k2[i]) + (2f * k3[i]) + k4[i]));
        }
    }
}

