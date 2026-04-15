using System.Buffers;
using System.Numerics;

namespace LAL.OdeCore;

public readonly record struct BdfStepResult(int Iterations, double Residual);
public readonly record struct BdfStepResultFloat(int Iterations, float Residual);

internal static class Bdf
{
    private const int StackallocDimensionThreshold = 128;

    public static BdfStepResult StepBackwardEuler(
        double t,
        double dt,
        ReadOnlySpan<double> y,
        Span<double> yOut,
        OdeSystem system,
        int maxIterations = 8,
        double tolerance = 1e-10)
    {
        if (system is null)
        {
            throw new ArgumentNullException(nameof(system));
        }

        if (dt <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(dt), "Time step must be positive.");
        }

        if (y.Length != yOut.Length)
        {
            throw new ArgumentException("Input and output vectors must have the same length.");
        }

        if (maxIterations <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxIterations), "Max iterations must be positive.");
        }

        if (tolerance <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tolerance), "Tolerance must be positive.");
        }

        int n = y.Length;
        double[]? rented = null;
        Span<double> dydt = n <= StackallocDimensionThreshold
            ? stackalloc double[n]
            : (rented = ArrayPool<double>.Shared.Rent(n)).AsSpan(0, n);

        try
        {
            y.CopyTo(yOut);
            double residual = double.PositiveInfinity;

            for (int iter = 1; iter <= maxIterations; iter++)
            {
                system(t + dt, yOut, dydt);

                residual = UpdateAndComputeResidual(y, yOut, dydt, dt);

                if (residual <= tolerance)
                {
                    return new BdfStepResult(iter, residual);
                }
            }

            return new BdfStepResult(maxIterations, residual);
        }
        finally
        {
            if (rented is not null)
            {
                ArrayPool<double>.Shared.Return(rented, clearArray: false);
            }
        }
    }

    public static BdfStepResultFloat StepBackwardEuler(
        float t,
        float dt,
        ReadOnlySpan<float> y,
        Span<float> yOut,
        OdeSystemFloat system,
        int maxIterations = 8,
        float tolerance = 1e-6f)
    {
        if (system is null)
        {
            throw new ArgumentNullException(nameof(system));
        }

        if (dt <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(dt), "Time step must be positive.");
        }

        if (y.Length != yOut.Length)
        {
            throw new ArgumentException("Input and output vectors must have the same length.");
        }

        if (maxIterations <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxIterations), "Max iterations must be positive.");
        }

        if (tolerance <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tolerance), "Tolerance must be positive.");
        }

        int n = y.Length;
        float[]? rented = null;
        Span<float> dydt = n <= StackallocDimensionThreshold
            ? stackalloc float[n]
            : (rented = ArrayPool<float>.Shared.Rent(n)).AsSpan(0, n);

        try
        {
            y.CopyTo(yOut);
            float residual = float.PositiveInfinity;

            for (int iter = 1; iter <= maxIterations; iter++)
            {
                system(t + dt, yOut, dydt);

                residual = UpdateAndComputeResidual(y, yOut, dydt, dt);

                if (residual <= tolerance)
                {
                    return new BdfStepResultFloat(iter, residual);
                }
            }

            return new BdfStepResultFloat(maxIterations, residual);
        }
        finally
        {
            if (rented is not null)
            {
                ArrayPool<float>.Shared.Return(rented, clearArray: false);
            }
        }
    }

    public static BdfStepResult StepBackwardEuler(
        double t,
        double dt,
        ReadOnlySpan<Complex> y,
        Span<Complex> yOut,
        OdeSystemComplex system,
        int maxIterations = 8,
        double tolerance = 1e-10)
    {
        if (system is null)
        {
            throw new ArgumentNullException(nameof(system));
        }

        if (dt <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(dt), "Time step must be positive.");
        }

        if (y.Length != yOut.Length)
        {
            throw new ArgumentException("Input and output vectors must have the same length.");
        }

        if (maxIterations <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxIterations), "Max iterations must be positive.");
        }

        if (tolerance <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tolerance), "Tolerance must be positive.");
        }

        int n = y.Length;
        Complex[]? rented = null;
        Span<Complex> dydt = n <= StackallocDimensionThreshold
            ? stackalloc Complex[n]
            : (rented = ArrayPool<Complex>.Shared.Rent(n)).AsSpan(0, n);

        try
        {
            y.CopyTo(yOut);
            double residual = double.PositiveInfinity;

            for (int iter = 1; iter <= maxIterations; iter++)
            {
                system(t + dt, yOut, dydt);

                residual = 0d;
                for (int i = 0; i < n; i++)
                {
                    Complex next = y[i] + (dt * dydt[i]);
                    double delta = Complex.Abs(next - yOut[i]);
                    if (delta > residual)
                    {
                        residual = delta;
                    }

                    yOut[i] = next;
                }

                if (residual <= tolerance)
                {
                    return new BdfStepResult(iter, residual);
                }
            }

            return new BdfStepResult(maxIterations, residual);
        }
        finally
        {
            if (rented is not null)
            {
                ArrayPool<Complex>.Shared.Return(rented, clearArray: false);
            }
        }
    }

    private static double UpdateAndComputeResidual(ReadOnlySpan<double> y, Span<double> yOut, ReadOnlySpan<double> dydt, double dt)
    {
        int i = 0;
        double residual = 0d;

        if (Vector.IsHardwareAccelerated && y.Length >= (Vector<double>.Count * 2))
        {
            int width = Vector<double>.Count;
            int end = y.Length - width;
            Vector<double> dtVec = new(dt);
            Vector<double> maxVec = Vector<double>.Zero;

            for (; i <= end; i += width)
            {
                Vector<double> yVec = new(y.Slice(i, width));
                Vector<double> dydtVec = new(dydt.Slice(i, width));
                Vector<double> prevVec = new(yOut.Slice(i, width));
                Vector<double> nextVec = yVec + (dtVec * dydtVec);
                Vector<double> deltaVec = Vector.Abs(nextVec - prevVec);

                maxVec = Vector.Max(maxVec, deltaVec);
                nextVec.CopyTo(yOut.Slice(i, width));
            }

            for (int j = 0; j < width; j++)
            {
                if (maxVec[j] > residual)
                {
                    residual = maxVec[j];
                }
            }
        }

        for (; i < y.Length; i++)
        {
            double next = y[i] + (dt * dydt[i]);
            double delta = Math.Abs(next - yOut[i]);
            if (delta > residual)
            {
                residual = delta;
            }

            yOut[i] = next;
        }

        return residual;
    }

    private static float UpdateAndComputeResidual(ReadOnlySpan<float> y, Span<float> yOut, ReadOnlySpan<float> dydt, float dt)
    {
        int i = 0;
        float residual = 0f;

        if (Vector.IsHardwareAccelerated && y.Length >= (Vector<float>.Count * 2))
        {
            int width = Vector<float>.Count;
            int end = y.Length - width;
            Vector<float> dtVec = new(dt);
            Vector<float> maxVec = Vector<float>.Zero;

            for (; i <= end; i += width)
            {
                Vector<float> yVec = new(y.Slice(i, width));
                Vector<float> dydtVec = new(dydt.Slice(i, width));
                Vector<float> prevVec = new(yOut.Slice(i, width));
                Vector<float> nextVec = yVec + (dtVec * dydtVec);
                Vector<float> deltaVec = Vector.Abs(nextVec - prevVec);

                maxVec = Vector.Max(maxVec, deltaVec);
                nextVec.CopyTo(yOut.Slice(i, width));
            }

            for (int j = 0; j < width; j++)
            {
                if (maxVec[j] > residual)
                {
                    residual = maxVec[j];
                }
            }
        }

        for (; i < y.Length; i++)
        {
            float next = y[i] + (dt * dydt[i]);
            float delta = MathF.Abs(next - yOut[i]);
            if (delta > residual)
            {
                residual = delta;
            }

            yOut[i] = next;
        }

        return residual;
    }
}

