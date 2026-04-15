using System.Buffers;
using LAL.Core;
using System.Numerics;
using System.Threading.Tasks;

namespace LAL.LinalgCore;

internal static class Gemv
{
    private const int ParallelRowThreshold = 128;
    private const long ParallelOpThreshold = 128L * 256L;

    [ThreadStatic]
    private static double[]? t_doubleArrayXScratch;

    public static void Multiply(
        double[] a,
        int rows,
        int cols,
        double[] x,
        double[] y,
        double alpha = 1d,
        double beta = 0d)
    {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(x);
        ArgumentNullException.ThrowIfNull(y);

        if (rows <= 0 || cols <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(rows), "Rows and cols must be positive.");
        }

        if (a.Length != rows * cols)
        {
            throw new ArgumentException("Matrix size does not match rows*cols.", nameof(a));
        }

        if (x.Length != cols)
        {
            throw new ArgumentException("Vector x length must equal cols.", nameof(x));
        }

        if (y.Length != rows)
        {
            throw new ArgumentException("Vector y length must equal rows.", nameof(y));
        }

        DataStructurePerformanceSettings settings = DataStructureCompatibility.GetPerformanceSettings();
        bool useParallel = ShouldUseParallel(rows, cols, settings);
        if (useParallel)
        {
            int workerCap = ResolveWorkerCap(rows, settings);
            ParallelOptions options = new() { MaxDegreeOfParallelism = workerCap };

            Parallel.For(
                0,
                rows,
                options,
                () =>
                {
                    double[] localX = GetThreadArrayDoubleXScratch(cols);
                    x.AsSpan(0, cols).CopyTo(localX);
                    return localX;
                },
                (r, _, localX) =>
                {
                    int rowOffset = r * cols;
                    double sum = DotRow(a.AsSpan(rowOffset, cols), localX.AsSpan(0, cols), settings);
                    y[r] = alpha * sum + beta * y[r];
                    return localX;
                },
                _ => { });

            return;
        }

        for (int r = 0; r < rows; r++)
        {
            int rowOffset = r * cols;
            double sum = DotRow(a.AsSpan(rowOffset, cols), x.AsSpan(0, cols), settings);
            y[r] = alpha * sum + beta * y[r];
        }
    }

    public static void Multiply(
        ReadOnlySpan<double> a,
        int rows,
        int cols,
        ReadOnlySpan<double> x,
        Span<double> y,
        double alpha = 1d,
        double beta = 0d)
    {
        if (rows <= 0 || cols <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(rows), "Rows and cols must be positive.");
        }

        if (a.Length != rows * cols)
        {
            throw new ArgumentException("Matrix size does not match rows*cols.", nameof(a));
        }

        if (x.Length != cols)
        {
            throw new ArgumentException("Vector x length must equal cols.", nameof(x));
        }

        if (y.Length != rows)
        {
            throw new ArgumentException("Vector y length must equal rows.", nameof(y));
        }

        DataStructurePerformanceSettings settings = DataStructureCompatibility.GetPerformanceSettings();
        bool useParallel = ShouldUseParallel(rows, cols, settings);
        if (useParallel)
        {
            double[] aBuffer = ArrayPool<double>.Shared.Rent(a.Length);
            double[] xBuffer = ArrayPool<double>.Shared.Rent(x.Length);
            double[] yBuffer = ArrayPool<double>.Shared.Rent(y.Length);
            int workerCap = ResolveWorkerCap(rows, settings);
            ParallelOptions options = new() { MaxDegreeOfParallelism = workerCap };

            a.CopyTo(aBuffer);
            x.CopyTo(xBuffer);
            y.CopyTo(yBuffer);

            try
            {
                Parallel.For(
                    0,
                    rows,
                    options,
                    () =>
                    {
                        double[] localX = ArrayPool<double>.Shared.Rent(cols);
                        xBuffer.AsSpan(0, cols).CopyTo(localX);
                        return localX;
                    },
                    (r, _, localX) =>
                    {
                        int rowOffset = r * cols;
                        double sum = DotRow(aBuffer.AsSpan(rowOffset, cols), localX.AsSpan(0, cols), settings);
                        yBuffer[r] = alpha * sum + beta * yBuffer[r];
                        return localX;
                    },
                    localX => ArrayPool<double>.Shared.Return(localX, clearArray: false));

                yBuffer.AsSpan(0, rows).CopyTo(y);
            }
            finally
            {
                ArrayPool<double>.Shared.Return(aBuffer, clearArray: false);
                ArrayPool<double>.Shared.Return(xBuffer, clearArray: false);
                ArrayPool<double>.Shared.Return(yBuffer, clearArray: false);
            }

            return;
        }

        for (int r = 0; r < rows; r++)
        {
            int rowOffset = r * cols;
            double sum = DotRow(a.Slice(rowOffset, cols), x, settings);

            y[r] = alpha * sum + beta * y[r];
        }
    }

    public static void Multiply(
        ReadOnlySpan<float> a,
        int rows,
        int cols,
        ReadOnlySpan<float> x,
        Span<float> y,
        float alpha = 1f,
        float beta = 0f)
    {
        if (rows <= 0 || cols <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(rows), "Rows and cols must be positive.");
        }

        if (a.Length != rows * cols)
        {
            throw new ArgumentException("Matrix size does not match rows*cols.", nameof(a));
        }

        if (x.Length != cols)
        {
            throw new ArgumentException("Vector x length must equal cols.", nameof(x));
        }

        if (y.Length != rows)
        {
            throw new ArgumentException("Vector y length must equal rows.", nameof(y));
        }

        DataStructurePerformanceSettings settings = DataStructureCompatibility.GetPerformanceSettings();
        bool useParallel = ShouldUseParallel(rows, cols, settings);
        if (useParallel)
        {
            float[] aBuffer = ArrayPool<float>.Shared.Rent(a.Length);
            float[] xBuffer = ArrayPool<float>.Shared.Rent(x.Length);
            float[] yBuffer = ArrayPool<float>.Shared.Rent(y.Length);
            int workerCap = ResolveWorkerCap(rows, settings);
            ParallelOptions options = new() { MaxDegreeOfParallelism = workerCap };

            a.CopyTo(aBuffer);
            x.CopyTo(xBuffer);
            y.CopyTo(yBuffer);

            try
            {
                Parallel.For(
                    0,
                    rows,
                    options,
                    () =>
                    {
                        float[] localX = ArrayPool<float>.Shared.Rent(cols);
                        xBuffer.AsSpan(0, cols).CopyTo(localX);
                        return localX;
                    },
                    (r, _, localX) =>
                    {
                        int rowOffset = r * cols;
                        float sum = DotRow(aBuffer.AsSpan(rowOffset, cols), localX.AsSpan(0, cols), settings);
                        yBuffer[r] = alpha * sum + beta * yBuffer[r];
                        return localX;
                    },
                    localX => ArrayPool<float>.Shared.Return(localX, clearArray: false));

                yBuffer.AsSpan(0, rows).CopyTo(y);
            }
            finally
            {
                ArrayPool<float>.Shared.Return(aBuffer, clearArray: false);
                ArrayPool<float>.Shared.Return(xBuffer, clearArray: false);
                ArrayPool<float>.Shared.Return(yBuffer, clearArray: false);
            }

            return;
        }

        for (int r = 0; r < rows; r++)
        {
            int rowOffset = r * cols;
            float sum = DotRow(a.Slice(rowOffset, cols), x, settings);

            y[r] = alpha * sum + beta * y[r];
        }
    }

    public static void Multiply(
        ReadOnlySpan<Complex> a,
        int rows,
        int cols,
        ReadOnlySpan<Complex> x,
        Span<Complex> y,
        Complex alpha,
        Complex beta)
    {
        if (rows <= 0 || cols <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(rows), "Rows and cols must be positive.");
        }

        if (a.Length != rows * cols)
        {
            throw new ArgumentException("Matrix size does not match rows*cols.", nameof(a));
        }

        if (x.Length != cols)
        {
            throw new ArgumentException("Vector x length must equal cols.", nameof(x));
        }

        if (y.Length != rows)
        {
            throw new ArgumentException("Vector y length must equal rows.", nameof(y));
        }

        DataStructurePerformanceSettings settings = DataStructureCompatibility.GetPerformanceSettings();
        bool useParallel = ShouldUseParallel(rows, cols, settings);
        if (useParallel)
        {
            Complex[] aBuffer = ArrayPool<Complex>.Shared.Rent(a.Length);
            Complex[] xBuffer = ArrayPool<Complex>.Shared.Rent(x.Length);
            Complex[] yBuffer = ArrayPool<Complex>.Shared.Rent(y.Length);
            int workerCap = ResolveWorkerCap(rows, settings);
            ParallelOptions options = new() { MaxDegreeOfParallelism = workerCap };

            a.CopyTo(aBuffer);
            x.CopyTo(xBuffer);
            y.CopyTo(yBuffer);

            try
            {
                Parallel.For(
                    0,
                    rows,
                    options,
                    () =>
                    {
                        Complex[] localX = ArrayPool<Complex>.Shared.Rent(cols);
                        xBuffer.AsSpan(0, cols).CopyTo(localX);
                        return localX;
                    },
                    (r, _, localX) =>
                    {
                        int rowOffset = r * cols;
                        Complex sum = DotRow(aBuffer.AsSpan(rowOffset, cols), localX.AsSpan(0, cols));
                        yBuffer[r] = alpha * sum + beta * yBuffer[r];
                        return localX;
                    },
                    localX => ArrayPool<Complex>.Shared.Return(localX, clearArray: false));

                yBuffer.AsSpan(0, rows).CopyTo(y);
            }
            finally
            {
                ArrayPool<Complex>.Shared.Return(aBuffer, clearArray: false);
                ArrayPool<Complex>.Shared.Return(xBuffer, clearArray: false);
                ArrayPool<Complex>.Shared.Return(yBuffer, clearArray: false);
            }

            return;
        }

        for (int r = 0; r < rows; r++)
        {
            int rowOffset = r * cols;
            Complex sum = DotRow(a.Slice(rowOffset, cols), x);

            y[r] = alpha * sum + beta * y[r];
        }
    }

    public static void Multiply(
        ReadOnlySpan<Complex> a,
        int rows,
        int cols,
        ReadOnlySpan<Complex> x,
        Span<Complex> y)
    {
        Multiply(a, rows, cols, x, y, Complex.One, Complex.Zero);
    }

    private static double DotRow(ReadOnlySpan<double> row, ReadOnlySpan<double> x, in DataStructurePerformanceSettings settings)
    {
        return PerformancePrimitives.Dot(row, x, settings);
    }

    private static float DotRow(ReadOnlySpan<float> row, ReadOnlySpan<float> x, in DataStructurePerformanceSettings settings)
    {
        return PerformancePrimitives.Dot(row, x, settings);
    }

    private static Complex DotRow(ReadOnlySpan<Complex> row, ReadOnlySpan<Complex> x)
    {
        Complex sum = Complex.Zero;
        for (int i = 0; i < row.Length; i++)
        {
            sum += row[i] * x[i];
        }

        return sum;
    }

    private static bool ShouldUseParallel(int rows, int cols, in DataStructurePerformanceSettings settings)
    {
        if (!settings.EnableParallel)
        {
            return false;
        }

        long opCount = (long)rows * cols;
        int effectiveCols = Math.Min(Math.Max(1, cols), 256);
        long strategyThreshold = (long)Math.Max(1, settings.ParallelLengthThreshold) * effectiveCols;
        return rows >= ParallelRowThreshold && opCount >= Math.Max(ParallelOpThreshold, strategyThreshold);
    }

    private static int ResolveWorkerCap(int workItems, in DataStructurePerformanceSettings settings)
    {
        int configured = settings.MaxDegreeOfParallelism ?? Environment.ProcessorCount;
        configured = Math.Max(1, configured);
        return Math.Max(1, Math.Min(configured, workItems));
    }

    private static double[] GetThreadArrayDoubleXScratch(int minLength)
    {
        double[]? buffer = t_doubleArrayXScratch;
        if (buffer is null || buffer.Length < minLength)
        {
            if (buffer is not null)
            {
                ArrayPool<double>.Shared.Return(buffer, clearArray: false);
            }

            buffer = ArrayPool<double>.Shared.Rent(minLength);
            t_doubleArrayXScratch = buffer;
        }

        return buffer;
    }

}

