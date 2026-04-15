using System.Buffers;
using System.Numerics;

namespace LAL.OdeCore;

public readonly record struct Rk45StepResult(double EstimatedError);
public readonly record struct Rk45StepResultFloat(float EstimatedError);

internal static class Rk45
{
    private const int StackallocDimensionThreshold = 48;

    public static Rk45StepResult Step(double t, double dt, ReadOnlySpan<double> y, Span<double> yOut, OdeSystem system)
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
            ? stackalloc double[n * 9]
            : (rented = ArrayPool<double>.Shared.Rent(n * 9)).AsSpan(0, n * 9);

        Span<double> k1 = workspace.Slice(0, n);
        Span<double> k2 = workspace.Slice(n, n);
        Span<double> k3 = workspace.Slice(n * 2, n);
        Span<double> k4 = workspace.Slice(n * 3, n);
        Span<double> k5 = workspace.Slice(n * 4, n);
        Span<double> k6 = workspace.Slice(n * 5, n);
        Span<double> k7 = workspace.Slice(n * 6, n);
        Span<double> temp = workspace.Slice(n * 7, n);
        Span<double> y4 = workspace.Slice(n * 8, n);

        try
        {
            system(t, y, k1);

            Advance(temp, y, dt, k1, 1.0 / 5.0);
            system(t + dt * (1.0 / 5.0), temp, k2);

            for (int i = 0; i < n; i++)
            {
                temp[i] = y[i] + dt * ((3.0 / 40.0) * k1[i] + (9.0 / 40.0) * k2[i]);
            }
            system(t + dt * (3.0 / 10.0), temp, k3);

            for (int i = 0; i < n; i++)
            {
                temp[i] = y[i] + dt * ((44.0 / 45.0) * k1[i] + (-56.0 / 15.0) * k2[i] + (32.0 / 9.0) * k3[i]);
            }
            system(t + dt * (4.0 / 5.0), temp, k4);

            for (int i = 0; i < n; i++)
            {
                temp[i] = y[i] + dt * ((19372.0 / 6561.0) * k1[i] + (-25360.0 / 2187.0) * k2[i] + (64448.0 / 6561.0) * k3[i] + (-212.0 / 729.0) * k4[i]);
            }
            system(t + dt * (8.0 / 9.0), temp, k5);

            for (int i = 0; i < n; i++)
            {
                temp[i] = y[i] + dt * ((9017.0 / 3168.0) * k1[i] + (-355.0 / 33.0) * k2[i] + (46732.0 / 5247.0) * k3[i] + (49.0 / 176.0) * k4[i] + (-5103.0 / 18656.0) * k5[i]);
            }
            system(t + dt, temp, k6);

            for (int i = 0; i < n; i++)
            {
                temp[i] = y[i] + dt * ((35.0 / 384.0) * k1[i] + (500.0 / 1113.0) * k3[i] + (125.0 / 192.0) * k4[i] + (-2187.0 / 6784.0) * k5[i] + (11.0 / 84.0) * k6[i]);
            }
            system(t + dt, temp, k7);

            for (int i = 0; i < n; i++)
            {
                yOut[i] = y[i] + dt * ((35.0 / 384.0) * k1[i] + (500.0 / 1113.0) * k3[i] + (125.0 / 192.0) * k4[i] + (-2187.0 / 6784.0) * k5[i] + (11.0 / 84.0) * k6[i]);

                y4[i] = y[i] + dt * ((5179.0 / 57600.0) * k1[i] + (7571.0 / 16695.0) * k3[i] + (393.0 / 640.0) * k4[i] + (-92097.0 / 339200.0) * k5[i] + (187.0 / 2100.0) * k6[i] + (1.0 / 40.0) * k7[i]);
            }

            double maxError = MaxAbsDiff(yOut, y4);

            return new Rk45StepResult(maxError);
        }
        finally
        {
            if (rented is not null)
            {
                ArrayPool<double>.Shared.Return(rented, clearArray: false);
            }
        }
    }

    public static Rk45StepResultFloat Step(float t, float dt, ReadOnlySpan<float> y, Span<float> yOut, OdeSystemFloat system)
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
            ? stackalloc float[n * 9]
            : (rented = ArrayPool<float>.Shared.Rent(n * 9)).AsSpan(0, n * 9);

        Span<float> k1 = workspace.Slice(0, n);
        Span<float> k2 = workspace.Slice(n, n);
        Span<float> k3 = workspace.Slice(n * 2, n);
        Span<float> k4 = workspace.Slice(n * 3, n);
        Span<float> k5 = workspace.Slice(n * 4, n);
        Span<float> k6 = workspace.Slice(n * 5, n);
        Span<float> k7 = workspace.Slice(n * 6, n);
        Span<float> temp = workspace.Slice(n * 7, n);
        Span<float> y4 = workspace.Slice(n * 8, n);

        try
        {
            system(t, y, k1);

            Advance(temp, y, dt, k1, 1f / 5f);
            system(t + (dt * (1f / 5f)), temp, k2);

            for (int i = 0; i < n; i++)
            {
                temp[i] = y[i] + (dt * ((3f / 40f) * k1[i] + (9f / 40f) * k2[i]));
            }
            system(t + (dt * (3f / 10f)), temp, k3);

            for (int i = 0; i < n; i++)
            {
                temp[i] = y[i] + (dt * ((44f / 45f) * k1[i] + (-56f / 15f) * k2[i] + (32f / 9f) * k3[i]));
            }
            system(t + (dt * (4f / 5f)), temp, k4);

            for (int i = 0; i < n; i++)
            {
                temp[i] = y[i] + (dt * ((19372f / 6561f) * k1[i] + (-25360f / 2187f) * k2[i] + (64448f / 6561f) * k3[i] + (-212f / 729f) * k4[i]));
            }
            system(t + (dt * (8f / 9f)), temp, k5);

            for (int i = 0; i < n; i++)
            {
                temp[i] = y[i] + (dt * ((9017f / 3168f) * k1[i] + (-355f / 33f) * k2[i] + (46732f / 5247f) * k3[i] + (49f / 176f) * k4[i] + (-5103f / 18656f) * k5[i]));
            }
            system(t + dt, temp, k6);

            for (int i = 0; i < n; i++)
            {
                temp[i] = y[i] + (dt * ((35f / 384f) * k1[i] + (500f / 1113f) * k3[i] + (125f / 192f) * k4[i] + (-2187f / 6784f) * k5[i] + (11f / 84f) * k6[i]));
            }
            system(t + dt, temp, k7);

            for (int i = 0; i < n; i++)
            {
                yOut[i] = y[i] + (dt * ((35f / 384f) * k1[i] + (500f / 1113f) * k3[i] + (125f / 192f) * k4[i] + (-2187f / 6784f) * k5[i] + (11f / 84f) * k6[i]));

                y4[i] = y[i] + (dt * ((5179f / 57600f) * k1[i] + (7571f / 16695f) * k3[i] + (393f / 640f) * k4[i] + (-92097f / 339200f) * k5[i] + (187f / 2100f) * k6[i] + (1f / 40f) * k7[i]));
            }

            float maxError = MaxAbsDiff(yOut, y4);

            return new Rk45StepResultFloat(maxError);
        }
        finally
        {
            if (rented is not null)
            {
                ArrayPool<float>.Shared.Return(rented, clearArray: false);
            }
        }
    }

    public static Rk45StepResult Step(double t, double dt, ReadOnlySpan<Complex> y, Span<Complex> yOut, OdeSystemComplex system)
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
            ? stackalloc Complex[n * 9]
            : (rented = ArrayPool<Complex>.Shared.Rent(n * 9)).AsSpan(0, n * 9);

        Span<Complex> k1 = workspace.Slice(0, n);
        Span<Complex> k2 = workspace.Slice(n, n);
        Span<Complex> k3 = workspace.Slice(n * 2, n);
        Span<Complex> k4 = workspace.Slice(n * 3, n);
        Span<Complex> k5 = workspace.Slice(n * 4, n);
        Span<Complex> k6 = workspace.Slice(n * 5, n);
        Span<Complex> k7 = workspace.Slice(n * 6, n);
        Span<Complex> temp = workspace.Slice(n * 7, n);
        Span<Complex> y4 = workspace.Slice(n * 8, n);

        try
        {
            system(t, y, k1);

            Advance(temp, y, dt, k1, 1.0 / 5.0);
            system(t + dt * (1.0 / 5.0), temp, k2);

            for (int i = 0; i < n; i++)
            {
                temp[i] = y[i] + dt * ((3.0 / 40.0) * k1[i] + (9.0 / 40.0) * k2[i]);
            }
            system(t + dt * (3.0 / 10.0), temp, k3);

            for (int i = 0; i < n; i++)
            {
                temp[i] = y[i] + dt * ((44.0 / 45.0) * k1[i] + (-56.0 / 15.0) * k2[i] + (32.0 / 9.0) * k3[i]);
            }
            system(t + dt * (4.0 / 5.0), temp, k4);

            for (int i = 0; i < n; i++)
            {
                temp[i] = y[i] + dt * ((19372.0 / 6561.0) * k1[i] + (-25360.0 / 2187.0) * k2[i] + (64448.0 / 6561.0) * k3[i] + (-212.0 / 729.0) * k4[i]);
            }
            system(t + dt * (8.0 / 9.0), temp, k5);

            for (int i = 0; i < n; i++)
            {
                temp[i] = y[i] + dt * ((9017.0 / 3168.0) * k1[i] + (-355.0 / 33.0) * k2[i] + (46732.0 / 5247.0) * k3[i] + (49.0 / 176.0) * k4[i] + (-5103.0 / 18656.0) * k5[i]);
            }
            system(t + dt, temp, k6);

            for (int i = 0; i < n; i++)
            {
                temp[i] = y[i] + dt * ((35.0 / 384.0) * k1[i] + (500.0 / 1113.0) * k3[i] + (125.0 / 192.0) * k4[i] + (-2187.0 / 6784.0) * k5[i] + (11.0 / 84.0) * k6[i]);
            }
            system(t + dt, temp, k7);

            for (int i = 0; i < n; i++)
            {
                yOut[i] = y[i] + dt * ((35.0 / 384.0) * k1[i] + (500.0 / 1113.0) * k3[i] + (125.0 / 192.0) * k4[i] + (-2187.0 / 6784.0) * k5[i] + (11.0 / 84.0) * k6[i]);

                y4[i] = y[i] + dt * ((5179.0 / 57600.0) * k1[i] + (7571.0 / 16695.0) * k3[i] + (393.0 / 640.0) * k4[i] + (-92097.0 / 339200.0) * k5[i] + (187.0 / 2100.0) * k6[i] + (1.0 / 40.0) * k7[i]);
            }

            double maxError = 0d;
            for (int i = 0; i < n; i++)
            {
                double e = Complex.Abs(yOut[i] - y4[i]);
                if (e > maxError)
                {
                    maxError = e;
                }
            }

            return new Rk45StepResult(maxError);
        }
        finally
        {
            if (rented is not null)
            {
                ArrayPool<Complex>.Shared.Return(rented, clearArray: false);
            }
        }
    }

    private static void Advance(Span<double> destination, ReadOnlySpan<double> y, double dt, ReadOnlySpan<double> k, double coeff)
    {
        int i = 0;
        double scale = dt * coeff;
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

    private static void Advance(Span<float> destination, ReadOnlySpan<float> y, float dt, ReadOnlySpan<float> k, float coeff)
    {
        int i = 0;
        float scale = dt * coeff;
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

    private static void Advance(Span<Complex> destination, ReadOnlySpan<Complex> y, double dt, ReadOnlySpan<Complex> k, double coeff)
    {
        for (int i = 0; i < y.Length; i++)
        {
            destination[i] = y[i] + ((dt * coeff) * k[i]);
        }
    }

    private static double MaxAbsDiff(ReadOnlySpan<double> x, ReadOnlySpan<double> y)
    {
        int i = 0;
        double maxError = 0d;

        if (Vector.IsHardwareAccelerated && x.Length >= (Vector<double>.Count * 2))
        {
            int width = Vector<double>.Count;
            int end = x.Length - width;
            Vector<double> maxVec = Vector<double>.Zero;

            for (; i <= end; i += width)
            {
                Vector<double> diff = Vector.Abs(new Vector<double>(x.Slice(i, width)) - new Vector<double>(y.Slice(i, width)));
                maxVec = Vector.Max(maxVec, diff);
            }

            for (int j = 0; j < width; j++)
            {
                if (maxVec[j] > maxError)
                {
                    maxError = maxVec[j];
                }
            }
        }

        for (; i < x.Length; i++)
        {
            double e = Math.Abs(x[i] - y[i]);
            if (e > maxError)
            {
                maxError = e;
            }
        }

        return maxError;
    }

    private static float MaxAbsDiff(ReadOnlySpan<float> x, ReadOnlySpan<float> y)
    {
        int i = 0;
        float maxError = 0f;

        if (Vector.IsHardwareAccelerated && x.Length >= (Vector<float>.Count * 2))
        {
            int width = Vector<float>.Count;
            int end = x.Length - width;
            Vector<float> maxVec = Vector<float>.Zero;

            for (; i <= end; i += width)
            {
                Vector<float> diff = Vector.Abs(new Vector<float>(x.Slice(i, width)) - new Vector<float>(y.Slice(i, width)));
                maxVec = Vector.Max(maxVec, diff);
            }

            for (int j = 0; j < width; j++)
            {
                if (maxVec[j] > maxError)
                {
                    maxError = maxVec[j];
                }
            }
        }

        for (; i < x.Length; i++)
        {
            float e = MathF.Abs(x[i] - y[i]);
            if (e > maxError)
            {
                maxError = e;
            }
        }

        return maxError;
    }
}

