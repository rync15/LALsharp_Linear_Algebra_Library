using System.Buffers;
using System.Numerics;
using System.Threading.Tasks;

namespace LAL.OdeCore;

internal static class JacobianEstimator
{
    private const int ParallelDimensionThreshold = 64;
    private const int StackallocDimensionThreshold = 64;

    public static void EstimateForwardDifference(
        double t,
        ReadOnlySpan<double> y,
        Span<double> jacobian,
        OdeSystem system,
        double epsilon = 1e-6,
        bool allowParallel = false)
    {
        if (system is null)
        {
            throw new ArgumentNullException(nameof(system));
        }

        if (epsilon <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(epsilon), "Epsilon must be positive.");
        }

        int n = y.Length;
        if (jacobian.Length != n * n)
        {
            throw new ArgumentException("Jacobian output length must be n*n.", nameof(jacobian));
        }

        if ((!allowParallel || n < ParallelDimensionThreshold) && n <= StackallocDimensionThreshold)
        {
            Span<double> yBaseStack = stackalloc double[n];
            Span<double> f0Stack = stackalloc double[n];
            Span<double> fPerturbedStack = stackalloc double[n];
            Span<double> yPerturbedStack = stackalloc double[n];

            y.CopyTo(yBaseStack);
            system(t, yBaseStack, f0Stack);
            yBaseStack.CopyTo(yPerturbedStack);

            for (int col = 0; col < n; col++)
            {
                double original = yPerturbedStack[col];
                double step = epsilon * Math.Max(1.0, Math.Abs(original));
                yPerturbedStack[col] = original + step;

                system(t, yPerturbedStack, fPerturbedStack);

                for (int row = 0; row < n; row++)
                {
                    jacobian[row * n + col] = (fPerturbedStack[row] - f0Stack[row]) / step;
                }

                yPerturbedStack[col] = original;
            }

            return;
        }

        double[] yBase = ArrayPool<double>.Shared.Rent(n);
        double[] f0 = ArrayPool<double>.Shared.Rent(n);
        y.CopyTo(yBase);

        try
        {
            system(t, yBase.AsSpan(0, n), f0.AsSpan(0, n));

            if (allowParallel && n >= ParallelDimensionThreshold)
            {
                double[] jacobianBuffer = ArrayPool<double>.Shared.Rent(n * n);
                int workerCap = Math.Max(1, Math.Min(Environment.ProcessorCount, n));
                ParallelOptions options = new() { MaxDegreeOfParallelism = workerCap };

                try
                {
                    Parallel.For(
                        0,
                        n,
                        options,
                        () => ArrayPool<double>.Shared.Rent(n * 2),
                        (col, _, localBuffer) =>
                        {
                            Span<double> yPerturbedLocal = localBuffer.AsSpan(0, n);
                            Span<double> fPerturbedLocal = localBuffer.AsSpan(n, n);

                            yBase.AsSpan(0, n).CopyTo(yPerturbedLocal);

                            double original = yPerturbedLocal[col];
                            double step = epsilon * Math.Max(1.0, Math.Abs(original));
                            yPerturbedLocal[col] = original + step;

                            system(t, yPerturbedLocal, fPerturbedLocal);

                            for (int row = 0; row < n; row++)
                            {
                                jacobianBuffer[row * n + col] = (fPerturbedLocal[row] - f0[row]) / step;
                            }

                            return localBuffer;
                        },
                        localBuffer => ArrayPool<double>.Shared.Return(localBuffer, clearArray: false));

                    jacobianBuffer.AsSpan(0, n * n).CopyTo(jacobian);
                }
                finally
                {
                    ArrayPool<double>.Shared.Return(jacobianBuffer, clearArray: false);
                }

                return;
            }

            double[] fPerturbed = ArrayPool<double>.Shared.Rent(n);
            double[] yPerturbed = ArrayPool<double>.Shared.Rent(n);
            try
            {
                yBase.AsSpan(0, n).CopyTo(yPerturbed);

                for (int col = 0; col < n; col++)
                {
                    double original = yPerturbed[col];
                    double step = epsilon * Math.Max(1.0, Math.Abs(original));
                    yPerturbed[col] = original + step;

                    system(t, yPerturbed.AsSpan(0, n), fPerturbed.AsSpan(0, n));

                    for (int row = 0; row < n; row++)
                    {
                        jacobian[row * n + col] = (fPerturbed[row] - f0[row]) / step;
                    }

                    yPerturbed[col] = original;
                }
            }
            finally
            {
                ArrayPool<double>.Shared.Return(fPerturbed, clearArray: false);
                ArrayPool<double>.Shared.Return(yPerturbed, clearArray: false);
            }
        }
        finally
        {
            ArrayPool<double>.Shared.Return(yBase, clearArray: false);
            ArrayPool<double>.Shared.Return(f0, clearArray: false);
        }
    }

    public static void EstimateForwardDifference(
        float t,
        ReadOnlySpan<float> y,
        Span<float> jacobian,
        OdeSystemFloat system,
        float epsilon = 1e-4f,
        bool allowParallel = false)
    {
        if (system is null)
        {
            throw new ArgumentNullException(nameof(system));
        }

        if (epsilon <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(epsilon), "Epsilon must be positive.");
        }

        int n = y.Length;
        if (jacobian.Length != n * n)
        {
            throw new ArgumentException("Jacobian output length must be n*n.", nameof(jacobian));
        }

        if ((!allowParallel || n < ParallelDimensionThreshold) && n <= StackallocDimensionThreshold)
        {
            Span<float> yBaseStack = stackalloc float[n];
            Span<float> f0Stack = stackalloc float[n];
            Span<float> fPerturbedStack = stackalloc float[n];
            Span<float> yPerturbedStack = stackalloc float[n];

            y.CopyTo(yBaseStack);
            system(t, yBaseStack, f0Stack);
            yBaseStack.CopyTo(yPerturbedStack);

            for (int col = 0; col < n; col++)
            {
                float original = yPerturbedStack[col];
                float step = epsilon * MathF.Max(1f, MathF.Abs(original));
                yPerturbedStack[col] = original + step;

                system(t, yPerturbedStack, fPerturbedStack);

                for (int row = 0; row < n; row++)
                {
                    jacobian[row * n + col] = (fPerturbedStack[row] - f0Stack[row]) / step;
                }

                yPerturbedStack[col] = original;
            }

            return;
        }

        float[] yBase = ArrayPool<float>.Shared.Rent(n);
        float[] f0 = ArrayPool<float>.Shared.Rent(n);
        y.CopyTo(yBase);

        try
        {
            system(t, yBase.AsSpan(0, n), f0.AsSpan(0, n));

            if (allowParallel && n >= ParallelDimensionThreshold)
            {
                float[] jacobianBuffer = ArrayPool<float>.Shared.Rent(n * n);
                int workerCap = Math.Max(1, Math.Min(Environment.ProcessorCount, n));
                ParallelOptions options = new() { MaxDegreeOfParallelism = workerCap };

                try
                {
                    Parallel.For(
                        0,
                        n,
                        options,
                        () => ArrayPool<float>.Shared.Rent(n * 2),
                        (col, _, localBuffer) =>
                        {
                            Span<float> yPerturbedLocal = localBuffer.AsSpan(0, n);
                            Span<float> fPerturbedLocal = localBuffer.AsSpan(n, n);

                            yBase.AsSpan(0, n).CopyTo(yPerturbedLocal);

                            float original = yPerturbedLocal[col];
                            float step = epsilon * MathF.Max(1f, MathF.Abs(original));
                            yPerturbedLocal[col] = original + step;

                            system(t, yPerturbedLocal, fPerturbedLocal);

                            for (int row = 0; row < n; row++)
                            {
                                jacobianBuffer[row * n + col] = (fPerturbedLocal[row] - f0[row]) / step;
                            }

                            return localBuffer;
                        },
                        localBuffer => ArrayPool<float>.Shared.Return(localBuffer, clearArray: false));

                    jacobianBuffer.AsSpan(0, n * n).CopyTo(jacobian);
                }
                finally
                {
                    ArrayPool<float>.Shared.Return(jacobianBuffer, clearArray: false);
                }

                return;
            }

            float[] fPerturbed = ArrayPool<float>.Shared.Rent(n);
            float[] yPerturbed = ArrayPool<float>.Shared.Rent(n);
            try
            {
                yBase.AsSpan(0, n).CopyTo(yPerturbed);

                for (int col = 0; col < n; col++)
                {
                    float original = yPerturbed[col];
                    float step = epsilon * MathF.Max(1f, MathF.Abs(original));
                    yPerturbed[col] = original + step;

                    system(t, yPerturbed.AsSpan(0, n), fPerturbed.AsSpan(0, n));

                    for (int row = 0; row < n; row++)
                    {
                        jacobian[row * n + col] = (fPerturbed[row] - f0[row]) / step;
                    }

                    yPerturbed[col] = original;
                }
            }
            finally
            {
                ArrayPool<float>.Shared.Return(fPerturbed, clearArray: false);
                ArrayPool<float>.Shared.Return(yPerturbed, clearArray: false);
            }
        }
        finally
        {
            ArrayPool<float>.Shared.Return(yBase, clearArray: false);
            ArrayPool<float>.Shared.Return(f0, clearArray: false);
        }
    }

    public static void EstimateForwardDifference(
        double t,
        ReadOnlySpan<Complex> y,
        Span<Complex> jacobian,
        OdeSystemComplex system,
        double epsilon = 1e-6,
        bool allowParallel = false)
    {
        if (system is null)
        {
            throw new ArgumentNullException(nameof(system));
        }

        if (epsilon <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(epsilon), "Epsilon must be positive.");
        }

        int n = y.Length;
        if (jacobian.Length != n * n)
        {
            throw new ArgumentException("Jacobian output length must be n*n.", nameof(jacobian));
        }

        if ((!allowParallel || n < ParallelDimensionThreshold) && n <= StackallocDimensionThreshold)
        {
            Span<Complex> yBaseStack = stackalloc Complex[n];
            Span<Complex> f0Stack = stackalloc Complex[n];
            Span<Complex> fPerturbedStack = stackalloc Complex[n];
            Span<Complex> yPerturbedStack = stackalloc Complex[n];

            y.CopyTo(yBaseStack);
            system(t, yBaseStack, f0Stack);
            yBaseStack.CopyTo(yPerturbedStack);

            for (int col = 0; col < n; col++)
            {
                Complex original = yPerturbedStack[col];
                double step = epsilon * Math.Max(1.0, Complex.Abs(original));
                yPerturbedStack[col] = original + new Complex(step, 0d);

                system(t, yPerturbedStack, fPerturbedStack);

                for (int row = 0; row < n; row++)
                {
                    jacobian[row * n + col] = (fPerturbedStack[row] - f0Stack[row]) / step;
                }

                yPerturbedStack[col] = original;
            }

            return;
        }

        Complex[] yBase = ArrayPool<Complex>.Shared.Rent(n);
        Complex[] f0 = ArrayPool<Complex>.Shared.Rent(n);
        y.CopyTo(yBase);

        try
        {
            system(t, yBase.AsSpan(0, n), f0.AsSpan(0, n));

            if (allowParallel && n >= ParallelDimensionThreshold)
            {
                Complex[] jacobianBuffer = ArrayPool<Complex>.Shared.Rent(n * n);
                int workerCap = Math.Max(1, Math.Min(Environment.ProcessorCount, n));
                ParallelOptions options = new() { MaxDegreeOfParallelism = workerCap };

                try
                {
                    Parallel.For(
                        0,
                        n,
                        options,
                        () => ArrayPool<Complex>.Shared.Rent(n * 2),
                        (col, _, localBuffer) =>
                        {
                            Span<Complex> yPerturbedLocal = localBuffer.AsSpan(0, n);
                            Span<Complex> fPerturbedLocal = localBuffer.AsSpan(n, n);

                            yBase.AsSpan(0, n).CopyTo(yPerturbedLocal);

                            Complex original = yPerturbedLocal[col];
                            double step = epsilon * Math.Max(1.0, Complex.Abs(original));
                            yPerturbedLocal[col] = original + new Complex(step, 0d);

                            system(t, yPerturbedLocal, fPerturbedLocal);

                            for (int row = 0; row < n; row++)
                            {
                                jacobianBuffer[row * n + col] = (fPerturbedLocal[row] - f0[row]) / step;
                            }

                            return localBuffer;
                        },
                        localBuffer => ArrayPool<Complex>.Shared.Return(localBuffer, clearArray: false));

                    jacobianBuffer.AsSpan(0, n * n).CopyTo(jacobian);
                }
                finally
                {
                    ArrayPool<Complex>.Shared.Return(jacobianBuffer, clearArray: false);
                }

                return;
            }

            Complex[] fPerturbed = ArrayPool<Complex>.Shared.Rent(n);
            Complex[] yPerturbed = ArrayPool<Complex>.Shared.Rent(n);
            try
            {
                yBase.AsSpan(0, n).CopyTo(yPerturbed);

                for (int col = 0; col < n; col++)
                {
                    Complex original = yPerturbed[col];
                    double step = epsilon * Math.Max(1.0, Complex.Abs(original));
                    yPerturbed[col] = original + new Complex(step, 0d);

                    system(t, yPerturbed.AsSpan(0, n), fPerturbed.AsSpan(0, n));

                    for (int row = 0; row < n; row++)
                    {
                        jacobian[row * n + col] = (fPerturbed[row] - f0[row]) / step;
                    }

                    yPerturbed[col] = original;
                }
            }
            finally
            {
                ArrayPool<Complex>.Shared.Return(fPerturbed, clearArray: false);
                ArrayPool<Complex>.Shared.Return(yPerturbed, clearArray: false);
            }
        }
        finally
        {
            ArrayPool<Complex>.Shared.Return(yBase, clearArray: false);
            ArrayPool<Complex>.Shared.Return(f0, clearArray: false);
        }
    }
}

