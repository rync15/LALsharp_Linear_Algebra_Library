using System.Buffers;
using System.Numerics;
using System.Threading.Tasks;

namespace LAL.NumericalCore.Statistics;

internal static class Covariance
{
    private const int ParallelLengthThreshold = 32_768;

    public static double Compute(ReadOnlySpan<double> x, ReadOnlySpan<double> y, bool sample = true, bool allowParallel = false)
    {
        if (x.Length != y.Length)
        {
            throw new ArgumentException("Input lengths must match.");
        }

        int n = x.Length;
        if (n == 0)
        {
            throw new ArgumentException("Inputs must not be empty.");
        }

        if (sample && n < 2)
        {
            throw new ArgumentException("Sample covariance requires at least two points.");
        }

        if (allowParallel && n >= ParallelLengthThreshold)
        {
            double[] xBuffer = ArrayPool<double>.Shared.Rent(n);
            double[] yBuffer = ArrayPool<double>.Shared.Rent(n);
            x.CopyTo(xBuffer);
            y.CopyTo(yBuffer);

            try
            {
                return ComputeParallel(xBuffer, yBuffer, n, sample);
            }
            finally
            {
                ArrayPool<double>.Shared.Return(xBuffer, clearArray: false);
                ArrayPool<double>.Shared.Return(yBuffer, clearArray: false);
            }
        }

        double meanX = SumVectorized(x) / n;
        double meanY = SumVectorized(y) / n;

        double sum = CovarianceSumVectorized(x, y, meanX, meanY);

        return sum / (sample ? (n - 1) : n);
    }

    public static double Correlation(ReadOnlySpan<double> x, ReadOnlySpan<double> y, bool allowParallel = false)
    {
        ValidateCorrelationInputs(x.Length, y.Length);
        int n = x.Length;

        if (allowParallel && n >= ParallelLengthThreshold)
        {
            double[] xBuffer = ArrayPool<double>.Shared.Rent(n);
            double[] yBuffer = ArrayPool<double>.Shared.Rent(n);
            x.CopyTo(xBuffer);
            y.CopyTo(yBuffer);

            try
            {
                return ComputeCorrelationParallel(xBuffer, yBuffer, n);
            }
            finally
            {
                ArrayPool<double>.Shared.Return(xBuffer, clearArray: false);
                ArrayPool<double>.Shared.Return(yBuffer, clearArray: false);
            }
        }

        return ComputeCorrelationSequential(x, y, n);
    }

    public static float Compute(ReadOnlySpan<float> x, ReadOnlySpan<float> y, bool sample = true, bool allowParallel = false)
    {
        if (x.Length != y.Length)
        {
            throw new ArgumentException("Input lengths must match.");
        }

        int n = x.Length;
        if (n == 0)
        {
            throw new ArgumentException("Inputs must not be empty.");
        }

        if (sample && n < 2)
        {
            throw new ArgumentException("Sample covariance requires at least two points.");
        }

        if (allowParallel && n >= ParallelLengthThreshold)
        {
            float[] xBuffer = ArrayPool<float>.Shared.Rent(n);
            float[] yBuffer = ArrayPool<float>.Shared.Rent(n);
            x.CopyTo(xBuffer);
            y.CopyTo(yBuffer);

            try
            {
                return ComputeParallel(xBuffer, yBuffer, n, sample);
            }
            finally
            {
                ArrayPool<float>.Shared.Return(xBuffer, clearArray: false);
                ArrayPool<float>.Shared.Return(yBuffer, clearArray: false);
            }
        }

        float meanX = SumVectorized(x) / n;
        float meanY = SumVectorized(y) / n;

        float sum = CovarianceSumVectorized(x, y, meanX, meanY);

        return sum / (sample ? (n - 1) : n);
    }

    public static float Correlation(ReadOnlySpan<float> x, ReadOnlySpan<float> y, bool allowParallel = false)
    {
        ValidateCorrelationInputs(x.Length, y.Length);
        int n = x.Length;

        if (allowParallel && n >= ParallelLengthThreshold)
        {
            float[] xBuffer = ArrayPool<float>.Shared.Rent(n);
            float[] yBuffer = ArrayPool<float>.Shared.Rent(n);
            x.CopyTo(xBuffer);
            y.CopyTo(yBuffer);

            try
            {
                return ComputeCorrelationParallel(xBuffer, yBuffer, n);
            }
            finally
            {
                ArrayPool<float>.Shared.Return(xBuffer, clearArray: false);
                ArrayPool<float>.Shared.Return(yBuffer, clearArray: false);
            }
        }

        return ComputeCorrelationSequential(x, y, n);
    }

    public static Complex Compute(ReadOnlySpan<Complex> x, ReadOnlySpan<Complex> y, bool sample = true, bool allowParallel = false)
    {
        if (x.Length != y.Length)
        {
            throw new ArgumentException("Input lengths must match.");
        }

        int n = x.Length;
        if (n == 0)
        {
            throw new ArgumentException("Inputs must not be empty.");
        }

        if (sample && n < 2)
        {
            throw new ArgumentException("Sample covariance requires at least two points.");
        }

        if (allowParallel && n >= ParallelLengthThreshold)
        {
            Complex[] xBuffer = ArrayPool<Complex>.Shared.Rent(n);
            Complex[] yBuffer = ArrayPool<Complex>.Shared.Rent(n);
            x.CopyTo(xBuffer);
            y.CopyTo(yBuffer);

            try
            {
                return ComputeParallel(xBuffer, yBuffer, n, sample);
            }
            finally
            {
                ArrayPool<Complex>.Shared.Return(xBuffer, clearArray: false);
                ArrayPool<Complex>.Shared.Return(yBuffer, clearArray: false);
            }
        }

        Complex meanX = Complex.Zero;
        Complex meanY = Complex.Zero;

        for (int i = 0; i < n; i++)
        {
            meanX += x[i];
            meanY += y[i];
        }

        meanX /= n;
        meanY /= n;

        Complex sum = Complex.Zero;
        for (int i = 0; i < n; i++)
        {
            sum += (x[i] - meanX) * Complex.Conjugate(y[i] - meanY);
        }

        return sum / (sample ? (n - 1) : n);
    }

    public static Complex Correlation(ReadOnlySpan<Complex> x, ReadOnlySpan<Complex> y, bool allowParallel = false)
    {
        ValidateCorrelationInputs(x.Length, y.Length);
        int n = x.Length;

        if (allowParallel && n >= ParallelLengthThreshold)
        {
            Complex[] xBuffer = ArrayPool<Complex>.Shared.Rent(n);
            Complex[] yBuffer = ArrayPool<Complex>.Shared.Rent(n);
            x.CopyTo(xBuffer);
            y.CopyTo(yBuffer);

            try
            {
                return ComputeCorrelationParallel(xBuffer, yBuffer, n);
            }
            finally
            {
                ArrayPool<Complex>.Shared.Return(xBuffer, clearArray: false);
                ArrayPool<Complex>.Shared.Return(yBuffer, clearArray: false);
            }
        }

        return ComputeCorrelationSequential(x, y, n);
    }

    private static double ComputeParallel(double[] x, double[] y, int n, bool sample)
    {
        int workerCap = Math.Max(1, Math.Min(Environment.ProcessorCount, n));
        ParallelOptions options = new() { MaxDegreeOfParallelism = workerCap };

        object gate = new();
        double sumX = 0d;
        double sumY = 0d;

        Parallel.For(
            0,
            n,
            options,
            () => (SumX: 0d, SumY: 0d),
            (i, _, local) =>
            {
                local.SumX += x[i];
                local.SumY += y[i];
                return local;
            },
            local =>
            {
                lock (gate)
                {
                    sumX += local.SumX;
                    sumY += local.SumY;
                }
            });

        double meanX = sumX / n;
        double meanY = sumY / n;

        double sum = 0d;
        Parallel.For(
            0,
            n,
            options,
            () => 0d,
            (i, _, local) => Math.FusedMultiplyAdd(x[i] - meanX, y[i] - meanY, local),
            local =>
            {
                lock (gate)
                {
                    sum += local;
                }
            });

        return sum / (sample ? (n - 1) : n);
    }

    private static float ComputeParallel(float[] x, float[] y, int n, bool sample)
    {
        int workerCap = Math.Max(1, Math.Min(Environment.ProcessorCount, n));
        ParallelOptions options = new() { MaxDegreeOfParallelism = workerCap };

        object gate = new();
        double sumX = 0d;
        double sumY = 0d;

        Parallel.For(
            0,
            n,
            options,
            () => (SumX: 0d, SumY: 0d),
            (i, _, local) =>
            {
                local.SumX += x[i];
                local.SumY += y[i];
                return local;
            },
            local =>
            {
                lock (gate)
                {
                    sumX += local.SumX;
                    sumY += local.SumY;
                }
            });

        double meanX = sumX / n;
        double meanY = sumY / n;

        double sum = 0d;
        Parallel.For(
            0,
            n,
            options,
            () => 0d,
            (i, _, local) => Math.FusedMultiplyAdd(x[i] - meanX, y[i] - meanY, local),
            local =>
            {
                lock (gate)
                {
                    sum += local;
                }
            });

        return (float)(sum / (sample ? (n - 1) : n));
    }

    private static Complex ComputeParallel(Complex[] x, Complex[] y, int n, bool sample)
    {
        int workerCap = Math.Max(1, Math.Min(Environment.ProcessorCount, n));
        ParallelOptions options = new() { MaxDegreeOfParallelism = workerCap };

        object gate = new();
        Complex sumX = Complex.Zero;
        Complex sumY = Complex.Zero;

        Parallel.For(
            0,
            n,
            options,
            () => (SumX: Complex.Zero, SumY: Complex.Zero),
            (i, _, local) =>
            {
                local.SumX += x[i];
                local.SumY += y[i];
                return local;
            },
            local =>
            {
                lock (gate)
                {
                    sumX += local.SumX;
                    sumY += local.SumY;
                }
            });

        Complex meanX = sumX / n;
        Complex meanY = sumY / n;

        Complex sum = Complex.Zero;
        Parallel.For(
            0,
            n,
            options,
            () => Complex.Zero,
            (i, _, local) => local + ((x[i] - meanX) * Complex.Conjugate(y[i] - meanY)),
            local =>
            {
                lock (gate)
                {
                    sum += local;
                }
            });

        return sum / (sample ? (n - 1) : n);
    }

    private static double ComputeCorrelationSequential(ReadOnlySpan<double> x, ReadOnlySpan<double> y, int n)
    {
        double meanX = SumVectorized(x) / n;
        double meanY = SumVectorized(y) / n;
        (double covSum, double varXSum, double varYSum) = CenteredSumsVectorized(x, y, meanX, meanY);

        double denom = Math.Sqrt(varXSum * varYSum);
        if (denom <= double.Epsilon)
        {
            return 0d;
        }

        return covSum / denom;
    }

    private static float ComputeCorrelationSequential(ReadOnlySpan<float> x, ReadOnlySpan<float> y, int n)
    {
        float meanX = SumVectorized(x) / n;
        float meanY = SumVectorized(y) / n;
        (float covSum, float varXSum, float varYSum) = CenteredSumsVectorized(x, y, meanX, meanY);

        float denom = MathF.Sqrt(varXSum * varYSum);
        if (denom <= float.Epsilon)
        {
            return 0f;
        }

        return covSum / denom;
    }

    private static Complex ComputeCorrelationSequential(ReadOnlySpan<Complex> x, ReadOnlySpan<Complex> y, int n)
    {
        Complex sumX = Complex.Zero;
        Complex sumY = Complex.Zero;

        for (int i = 0; i < n; i++)
        {
            sumX += x[i];
            sumY += y[i];
        }

        Complex meanX = sumX / n;
        Complex meanY = sumY / n;

        Complex covSum = Complex.Zero;
        double varXSum = 0d;
        double varYSum = 0d;

        for (int i = 0; i < n; i++)
        {
            Complex dx = x[i] - meanX;
            Complex dy = y[i] - meanY;
            covSum += dx * Complex.Conjugate(dy);
            varXSum = Math.FusedMultiplyAdd(dx.Real, dx.Real, varXSum);
            varXSum = Math.FusedMultiplyAdd(dx.Imaginary, dx.Imaginary, varXSum);
            varYSum = Math.FusedMultiplyAdd(dy.Real, dy.Real, varYSum);
            varYSum = Math.FusedMultiplyAdd(dy.Imaginary, dy.Imaginary, varYSum);
        }

        double denom = Math.Sqrt(varXSum * varYSum);
        if (denom <= double.Epsilon)
        {
            return Complex.Zero;
        }

        return covSum / denom;
    }

    private static double ComputeCorrelationParallel(double[] x, double[] y, int n)
    {
        int workerCap = Math.Max(1, Math.Min(Environment.ProcessorCount, n));
        ParallelOptions options = new() { MaxDegreeOfParallelism = workerCap };

        object gate = new();
        double sumX = 0d;
        double sumY = 0d;

        Parallel.For(
            0,
            n,
            options,
            () => (SumX: 0d, SumY: 0d),
            (i, _, local) =>
            {
                local.SumX += x[i];
                local.SumY += y[i];
                return local;
            },
            local =>
            {
                lock (gate)
                {
                    sumX += local.SumX;
                    sumY += local.SumY;
                }
            });

        double meanX = sumX / n;
        double meanY = sumY / n;

        double covSum = 0d;
        double varXSum = 0d;
        double varYSum = 0d;

        Parallel.For(
            0,
            n,
            options,
            () => (Cov: 0d, VarX: 0d, VarY: 0d),
            (i, _, local) =>
            {
                double dx = x[i] - meanX;
                double dy = y[i] - meanY;
                local.Cov = Math.FusedMultiplyAdd(dx, dy, local.Cov);
                local.VarX = Math.FusedMultiplyAdd(dx, dx, local.VarX);
                local.VarY = Math.FusedMultiplyAdd(dy, dy, local.VarY);
                return local;
            },
            local =>
            {
                lock (gate)
                {
                    covSum += local.Cov;
                    varXSum += local.VarX;
                    varYSum += local.VarY;
                }
            });

        double denom = Math.Sqrt(varXSum * varYSum);
        if (denom <= double.Epsilon)
        {
            return 0d;
        }

        return covSum / denom;
    }

    private static float ComputeCorrelationParallel(float[] x, float[] y, int n)
    {
        int workerCap = Math.Max(1, Math.Min(Environment.ProcessorCount, n));
        ParallelOptions options = new() { MaxDegreeOfParallelism = workerCap };

        object gate = new();
        double sumX = 0d;
        double sumY = 0d;

        Parallel.For(
            0,
            n,
            options,
            () => (SumX: 0d, SumY: 0d),
            (i, _, local) =>
            {
                local.SumX += x[i];
                local.SumY += y[i];
                return local;
            },
            local =>
            {
                lock (gate)
                {
                    sumX += local.SumX;
                    sumY += local.SumY;
                }
            });

        double meanX = sumX / n;
        double meanY = sumY / n;

        double covSum = 0d;
        double varXSum = 0d;
        double varYSum = 0d;

        Parallel.For(
            0,
            n,
            options,
            () => (Cov: 0d, VarX: 0d, VarY: 0d),
            (i, _, local) =>
            {
                double dx = x[i] - meanX;
                double dy = y[i] - meanY;
                local.Cov = Math.FusedMultiplyAdd(dx, dy, local.Cov);
                local.VarX = Math.FusedMultiplyAdd(dx, dx, local.VarX);
                local.VarY = Math.FusedMultiplyAdd(dy, dy, local.VarY);
                return local;
            },
            local =>
            {
                lock (gate)
                {
                    covSum += local.Cov;
                    varXSum += local.VarX;
                    varYSum += local.VarY;
                }
            });

        double denom = Math.Sqrt(varXSum * varYSum);
        if (denom <= float.Epsilon)
        {
            return 0f;
        }

        return (float)(covSum / denom);
    }

    private static Complex ComputeCorrelationParallel(Complex[] x, Complex[] y, int n)
    {
        int workerCap = Math.Max(1, Math.Min(Environment.ProcessorCount, n));
        ParallelOptions options = new() { MaxDegreeOfParallelism = workerCap };

        object gate = new();
        Complex sumX = Complex.Zero;
        Complex sumY = Complex.Zero;

        Parallel.For(
            0,
            n,
            options,
            () => (SumX: Complex.Zero, SumY: Complex.Zero),
            (i, _, local) =>
            {
                local.SumX += x[i];
                local.SumY += y[i];
                return local;
            },
            local =>
            {
                lock (gate)
                {
                    sumX += local.SumX;
                    sumY += local.SumY;
                }
            });

        Complex meanX = sumX / n;
        Complex meanY = sumY / n;

        Complex covSum = Complex.Zero;
        double varXSum = 0d;
        double varYSum = 0d;

        Parallel.For(
            0,
            n,
            options,
            () => (Cov: Complex.Zero, VarX: 0d, VarY: 0d),
            (i, _, local) =>
            {
                Complex dx = x[i] - meanX;
                Complex dy = y[i] - meanY;
                local.Cov += dx * Complex.Conjugate(dy);
                local.VarX = Math.FusedMultiplyAdd(dx.Real, dx.Real, local.VarX);
                local.VarX = Math.FusedMultiplyAdd(dx.Imaginary, dx.Imaginary, local.VarX);
                local.VarY = Math.FusedMultiplyAdd(dy.Real, dy.Real, local.VarY);
                local.VarY = Math.FusedMultiplyAdd(dy.Imaginary, dy.Imaginary, local.VarY);
                return local;
            },
            local =>
            {
                lock (gate)
                {
                    covSum += local.Cov;
                    varXSum += local.VarX;
                    varYSum += local.VarY;
                }
            });

        double denom = Math.Sqrt(varXSum * varYSum);
        if (denom <= double.Epsilon)
        {
            return Complex.Zero;
        }

        return covSum / denom;
    }

    private static (double CovSum, double VarXSum, double VarYSum) CenteredSumsVectorized(ReadOnlySpan<double> x, ReadOnlySpan<double> y, double meanX, double meanY)
    {
        int i = 0;
        double covSum = 0d;
        double varXSum = 0d;
        double varYSum = 0d;

        if (Vector.IsHardwareAccelerated && x.Length >= (Vector<double>.Count * 2))
        {
            int width = Vector<double>.Count;
            int end = x.Length - width;
            Vector<double> meanXVec = new(meanX);
            Vector<double> meanYVec = new(meanY);
            Vector<double> covAcc = Vector<double>.Zero;
            Vector<double> varXAcc = Vector<double>.Zero;
            Vector<double> varYAcc = Vector<double>.Zero;

            for (; i <= end; i += width)
            {
                Vector<double> dx = new Vector<double>(x.Slice(i, width)) - meanXVec;
                Vector<double> dy = new Vector<double>(y.Slice(i, width)) - meanYVec;
                covAcc += dx * dy;
                varXAcc += dx * dx;
                varYAcc += dy * dy;
            }

            for (int j = 0; j < width; j++)
            {
                covSum += covAcc[j];
                varXSum += varXAcc[j];
                varYSum += varYAcc[j];
            }
        }

        for (; i < x.Length; i++)
        {
            double dx = x[i] - meanX;
            double dy = y[i] - meanY;
            covSum = Math.FusedMultiplyAdd(dx, dy, covSum);
            varXSum = Math.FusedMultiplyAdd(dx, dx, varXSum);
            varYSum = Math.FusedMultiplyAdd(dy, dy, varYSum);
        }

        return (covSum, varXSum, varYSum);
    }

    private static (float CovSum, float VarXSum, float VarYSum) CenteredSumsVectorized(ReadOnlySpan<float> x, ReadOnlySpan<float> y, float meanX, float meanY)
    {
        int i = 0;
        float covSum = 0f;
        float varXSum = 0f;
        float varYSum = 0f;

        if (Vector.IsHardwareAccelerated && x.Length >= (Vector<float>.Count * 2))
        {
            int width = Vector<float>.Count;
            int end = x.Length - width;
            Vector<float> meanXVec = new(meanX);
            Vector<float> meanYVec = new(meanY);
            Vector<float> covAcc = Vector<float>.Zero;
            Vector<float> varXAcc = Vector<float>.Zero;
            Vector<float> varYAcc = Vector<float>.Zero;

            for (; i <= end; i += width)
            {
                Vector<float> dx = new Vector<float>(x.Slice(i, width)) - meanXVec;
                Vector<float> dy = new Vector<float>(y.Slice(i, width)) - meanYVec;
                covAcc += dx * dy;
                varXAcc += dx * dx;
                varYAcc += dy * dy;
            }

            for (int j = 0; j < width; j++)
            {
                covSum += covAcc[j];
                varXSum += varXAcc[j];
                varYSum += varYAcc[j];
            }
        }

        for (; i < x.Length; i++)
        {
            float dx = x[i] - meanX;
            float dy = y[i] - meanY;
            covSum = MathF.FusedMultiplyAdd(dx, dy, covSum);
            varXSum = MathF.FusedMultiplyAdd(dx, dx, varXSum);
            varYSum = MathF.FusedMultiplyAdd(dy, dy, varYSum);
        }

        return (covSum, varXSum, varYSum);
    }

    private static void ValidateCorrelationInputs(int xLength, int yLength)
    {
        if (xLength != yLength)
        {
            throw new ArgumentException("Input lengths must match.");
        }

        if (xLength == 0)
        {
            throw new ArgumentException("Inputs must not be empty.");
        }

        if (xLength < 2)
        {
            throw new ArgumentException("Sample covariance requires at least two points.");
        }
    }

    private static double CovarianceSumVectorized(ReadOnlySpan<double> x, ReadOnlySpan<double> y, double meanX, double meanY)
    {
        int i = 0;
        double sum = 0d;

        if (Vector.IsHardwareAccelerated && x.Length >= (Vector<double>.Count * 2))
        {
            int width = Vector<double>.Count;
            int end = x.Length - width;
            Vector<double> meanXVec = new(meanX);
            Vector<double> meanYVec = new(meanY);
            Vector<double> acc = Vector<double>.Zero;

            for (; i <= end; i += width)
            {
                Vector<double> dx = new Vector<double>(x.Slice(i, width)) - meanXVec;
                Vector<double> dy = new Vector<double>(y.Slice(i, width)) - meanYVec;
                acc += dx * dy;
            }

            for (int j = 0; j < width; j++)
            {
                sum += acc[j];
            }
        }

        for (; i < x.Length; i++)
        {
            sum = Math.FusedMultiplyAdd(x[i] - meanX, y[i] - meanY, sum);
        }

        return sum;
    }

    private static float CovarianceSumVectorized(ReadOnlySpan<float> x, ReadOnlySpan<float> y, float meanX, float meanY)
    {
        int i = 0;
        float sum = 0f;

        if (Vector.IsHardwareAccelerated && x.Length >= (Vector<float>.Count * 2))
        {
            int width = Vector<float>.Count;
            int end = x.Length - width;
            Vector<float> meanXVec = new(meanX);
            Vector<float> meanYVec = new(meanY);
            Vector<float> acc = Vector<float>.Zero;

            for (; i <= end; i += width)
            {
                Vector<float> dx = new Vector<float>(x.Slice(i, width)) - meanXVec;
                Vector<float> dy = new Vector<float>(y.Slice(i, width)) - meanYVec;
                acc += dx * dy;
            }

            for (int j = 0; j < width; j++)
            {
                sum += acc[j];
            }
        }

        for (; i < x.Length; i++)
        {
            sum = MathF.FusedMultiplyAdd(x[i] - meanX, y[i] - meanY, sum);
        }

        return sum;
    }

    private static double SumVectorized(ReadOnlySpan<double> values)
    {
        int i = 0;
        double sum = 0d;

        if (Vector.IsHardwareAccelerated && values.Length >= (Vector<double>.Count * 2))
        {
            int width = Vector<double>.Count;
            int end = values.Length - width;
            Vector<double> acc = Vector<double>.Zero;

            for (; i <= end; i += width)
            {
                acc += new Vector<double>(values.Slice(i, width));
            }

            for (int j = 0; j < width; j++)
            {
                sum += acc[j];
            }
        }

        for (; i < values.Length; i++)
        {
            sum += values[i];
        }

        return sum;
    }

    private static float SumVectorized(ReadOnlySpan<float> values)
    {
        int i = 0;
        float sum = 0f;

        if (Vector.IsHardwareAccelerated && values.Length >= (Vector<float>.Count * 2))
        {
            int width = Vector<float>.Count;
            int end = values.Length - width;
            Vector<float> acc = Vector<float>.Zero;

            for (; i <= end; i += width)
            {
                acc += new Vector<float>(values.Slice(i, width));
            }

            for (int j = 0; j < width; j++)
            {
                sum += acc[j];
            }
        }

        for (; i < values.Length; i++)
        {
            sum += values[i];
        }

        return sum;
    }
}

