using System.Buffers;
using System.Numerics;

namespace LAL.OdeCore;

public readonly record struct RadauStepResult(int Iterations, double Residual);
public readonly record struct RadauStepResultFloat(int Iterations, float Residual);

internal static class Radau
{
    private const int StackallocDimensionThreshold = 128;

    public static RadauStepResult StepOneStage(
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
        Span<double> workspace = n <= StackallocDimensionThreshold
            ? stackalloc double[n * 2]
            : (rented = ArrayPool<double>.Shared.Rent(n * 2)).AsSpan(0, n * 2);
        Span<double> stage = workspace.Slice(0, n);
        Span<double> dydt = workspace.Slice(n, n);
        y.CopyTo(stage);

        try
        {
            double c = 2.0 / 3.0;
            double residual = double.PositiveInfinity;

            for (int iter = 1; iter <= maxIterations; iter++)
            {
                system(t + (c * dt), stage, dydt);

                residual = UpdateStageAndComputeResidual(y, stage, dydt, c * dt);

                if (residual <= tolerance)
                {
                    system(t + dt, stage, dydt);
                    CombineState(y, dydt, dt, yOut);

                    return new RadauStepResult(iter, residual);
                }
            }

            system(t + dt, stage, dydt);
            CombineState(y, dydt, dt, yOut);

            return new RadauStepResult(maxIterations, residual);
        }
        finally
        {
            if (rented is not null)
            {
                ArrayPool<double>.Shared.Return(rented, clearArray: false);
            }
        }
    }

    public static RadauStepResultFloat StepOneStage(
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
        Span<float> workspace = n <= StackallocDimensionThreshold
            ? stackalloc float[n * 2]
            : (rented = ArrayPool<float>.Shared.Rent(n * 2)).AsSpan(0, n * 2);
        Span<float> stage = workspace.Slice(0, n);
        Span<float> dydt = workspace.Slice(n, n);
        y.CopyTo(stage);

        try
        {
            float c = 2f / 3f;
            float residual = float.PositiveInfinity;

            for (int iter = 1; iter <= maxIterations; iter++)
            {
                system(t + (c * dt), stage, dydt);

                residual = UpdateStageAndComputeResidual(y, stage, dydt, c * dt);

                if (residual <= tolerance)
                {
                    system(t + dt, stage, dydt);
                    CombineState(y, dydt, dt, yOut);

                    return new RadauStepResultFloat(iter, residual);
                }
            }

            system(t + dt, stage, dydt);
            CombineState(y, dydt, dt, yOut);

            return new RadauStepResultFloat(maxIterations, residual);
        }
        finally
        {
            if (rented is not null)
            {
                ArrayPool<float>.Shared.Return(rented, clearArray: false);
            }
        }
    }

    public static RadauStepResult StepOneStage(
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
        Span<Complex> workspace = n <= StackallocDimensionThreshold
            ? stackalloc Complex[n * 2]
            : (rented = ArrayPool<Complex>.Shared.Rent(n * 2)).AsSpan(0, n * 2);
        Span<Complex> stage = workspace.Slice(0, n);
        Span<Complex> dydt = workspace.Slice(n, n);
        y.CopyTo(stage);

        try
        {
            double c = 2.0 / 3.0;
            double residual = double.PositiveInfinity;

            for (int iter = 1; iter <= maxIterations; iter++)
            {
                system(t + (c * dt), stage, dydt);

                residual = 0d;
                for (int i = 0; i < n; i++)
                {
                    Complex next = y[i] + (c * dt * dydt[i]);
                    double delta = Complex.Abs(next - stage[i]);
                    if (delta > residual)
                    {
                        residual = delta;
                    }

                    stage[i] = next;
                }

                if (residual <= tolerance)
                {
                    system(t + dt, stage, dydt);
                    for (int i = 0; i < n; i++)
                    {
                        yOut[i] = y[i] + (dt * dydt[i]);
                    }

                    return new RadauStepResult(iter, residual);
                }
            }

            system(t + dt, stage, dydt);
            for (int i = 0; i < n; i++)
            {
                yOut[i] = y[i] + (dt * dydt[i]);
            }

            return new RadauStepResult(maxIterations, residual);
        }
        finally
        {
            if (rented is not null)
            {
                ArrayPool<Complex>.Shared.Return(rented, clearArray: false);
            }
        }
    }

    private static double UpdateStageAndComputeResidual(ReadOnlySpan<double> y, Span<double> stage, ReadOnlySpan<double> dydt, double scale)
    {
        int i = 0;
        double residual = 0d;

        if (Vector.IsHardwareAccelerated && y.Length >= (Vector<double>.Count * 2))
        {
            int width = Vector<double>.Count;
            int end = y.Length - width;
            Vector<double> scaleVec = new(scale);
            Vector<double> maxVec = Vector<double>.Zero;

            for (; i <= end; i += width)
            {
                Vector<double> yVec = new(y.Slice(i, width));
                Vector<double> dydtVec = new(dydt.Slice(i, width));
                Vector<double> prevVec = new(stage.Slice(i, width));
                Vector<double> nextVec = yVec + (scaleVec * dydtVec);
                Vector<double> deltaVec = Vector.Abs(nextVec - prevVec);

                maxVec = Vector.Max(maxVec, deltaVec);
                nextVec.CopyTo(stage.Slice(i, width));
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
            double next = y[i] + (scale * dydt[i]);
            double delta = Math.Abs(next - stage[i]);
            if (delta > residual)
            {
                residual = delta;
            }

            stage[i] = next;
        }

        return residual;
    }

    private static float UpdateStageAndComputeResidual(ReadOnlySpan<float> y, Span<float> stage, ReadOnlySpan<float> dydt, float scale)
    {
        int i = 0;
        float residual = 0f;

        if (Vector.IsHardwareAccelerated && y.Length >= (Vector<float>.Count * 2))
        {
            int width = Vector<float>.Count;
            int end = y.Length - width;
            Vector<float> scaleVec = new(scale);
            Vector<float> maxVec = Vector<float>.Zero;

            for (; i <= end; i += width)
            {
                Vector<float> yVec = new(y.Slice(i, width));
                Vector<float> dydtVec = new(dydt.Slice(i, width));
                Vector<float> prevVec = new(stage.Slice(i, width));
                Vector<float> nextVec = yVec + (scaleVec * dydtVec);
                Vector<float> deltaVec = Vector.Abs(nextVec - prevVec);

                maxVec = Vector.Max(maxVec, deltaVec);
                nextVec.CopyTo(stage.Slice(i, width));
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
            float next = y[i] + (scale * dydt[i]);
            float delta = MathF.Abs(next - stage[i]);
            if (delta > residual)
            {
                residual = delta;
            }

            stage[i] = next;
        }

        return residual;
    }

    private static void CombineState(ReadOnlySpan<double> y, ReadOnlySpan<double> dydt, double dt, Span<double> yOut)
    {
        int i = 0;
        if (Vector.IsHardwareAccelerated && y.Length >= (Vector<double>.Count * 2))
        {
            int width = Vector<double>.Count;
            int end = y.Length - width;
            Vector<double> dtVec = new(dt);

            for (; i <= end; i += width)
            {
                Vector<double> yVec = new(y.Slice(i, width));
                Vector<double> dydtVec = new(dydt.Slice(i, width));
                (yVec + (dtVec * dydtVec)).CopyTo(yOut.Slice(i, width));
            }
        }

        for (; i < y.Length; i++)
        {
            yOut[i] = y[i] + (dt * dydt[i]);
        }
    }

    private static void CombineState(ReadOnlySpan<float> y, ReadOnlySpan<float> dydt, float dt, Span<float> yOut)
    {
        int i = 0;
        if (Vector.IsHardwareAccelerated && y.Length >= (Vector<float>.Count * 2))
        {
            int width = Vector<float>.Count;
            int end = y.Length - width;
            Vector<float> dtVec = new(dt);

            for (; i <= end; i += width)
            {
                Vector<float> yVec = new(y.Slice(i, width));
                Vector<float> dydtVec = new(dydt.Slice(i, width));
                (yVec + (dtVec * dydtVec)).CopyTo(yOut.Slice(i, width));
            }
        }

        for (; i < y.Length; i++)
        {
            yOut[i] = y[i] + (dt * dydt[i]);
        }
    }
}

