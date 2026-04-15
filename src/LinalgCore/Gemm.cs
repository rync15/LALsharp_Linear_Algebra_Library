using System.Buffers;
using LAL.Core;
using System.Numerics;
using System.Threading.Tasks;

namespace LAL.LinalgCore;

internal static class Gemm
{
    private const int ParallelRowThreshold = 64;
    private const long ParallelOpThreshold = 64L * 64L * 64L;

    [ThreadStatic]
    private static double[]? t_doubleArrayColumnsScratch;

    [ThreadStatic]
    private static double[]? t_doubleArrayRowScratch;

    public static void Multiply(
        double[] a,
        double[] b,
        double[] c,
        int m,
        int n,
        int k,
        double alpha = 1d,
        double beta = 0d)
    {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);
        ArgumentNullException.ThrowIfNull(c);

        if (m <= 0 || n <= 0 || k <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(m), "m, n, and k must be positive.");
        }

        if (a.Length != m * k)
        {
            throw new ArgumentException("Matrix A size must be m*k.", nameof(a));
        }

        if (b.Length != k * n)
        {
            throw new ArgumentException("Matrix B size must be k*n.", nameof(b));
        }

        if (c.Length != m * n)
        {
            throw new ArgumentException("Matrix C size must be m*n.", nameof(c));
        }

        DataStructurePerformanceSettings settings = DataStructureCompatibility.GetPerformanceSettings();
        bool useParallel = ShouldUseParallel(m, n, k, settings);

        double[] bColumnsBuffer = GetThreadArrayDoubleColumnsScratch(b.Length);
        TransposeToColumnVectors(b, k, n, bColumnsBuffer.AsSpan(0, b.Length));

        if (useParallel)
        {
            int workerCap = ResolveWorkerCap(m, settings);
            ParallelOptions options = new() { MaxDegreeOfParallelism = workerCap };

            Parallel.For(
                0,
                m,
                options,
                () => GetThreadArrayDoubleRowScratch(n),
                (i, _, rowScratch) =>
                {
                    int aRow = i * k;
                    int cRow = i * n;
                    Span<double> scratch = rowScratch.AsSpan(0, n);

                    for (int j = 0; j < n; j++)
                    {
                        scratch[j] = PerformancePrimitives.Dot(a.AsSpan(aRow, k), bColumnsBuffer.AsSpan(j * k, k), settings);
                    }

                    for (int j = 0; j < n; j++)
                    {
                        int idx = cRow + j;
                        c[idx] = (alpha * scratch[j]) + (beta * c[idx]);
                    }

                    return rowScratch;
                },
                _ => { });

            return;
        }

        for (int i = 0; i < m; i++)
        {
            int aRow = i * k;
            int cRow = i * n;

            for (int j = 0; j < n; j++)
            {
                double sum = PerformancePrimitives.Dot(a.AsSpan(aRow, k), bColumnsBuffer.AsSpan(j * k, k), settings);
                int idx = cRow + j;
                c[idx] = (alpha * sum) + (beta * c[idx]);
            }
        }
    }

    public static void Multiply(
        ReadOnlySpan<double> a,
        ReadOnlySpan<double> b,
        Span<double> c,
        int m,
        int n,
        int k,
        double alpha = 1d,
        double beta = 0d)
    {
        if (m <= 0 || n <= 0 || k <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(m), "m, n, and k must be positive.");
        }

        if (a.Length != m * k)
        {
            throw new ArgumentException("Matrix A size must be m*k.", nameof(a));
        }

        if (b.Length != k * n)
        {
            throw new ArgumentException("Matrix B size must be k*n.", nameof(b));
        }

        if (c.Length != m * n)
        {
            throw new ArgumentException("Matrix C size must be m*n.", nameof(c));
        }

        DataStructurePerformanceSettings settings = DataStructureCompatibility.GetPerformanceSettings();
        bool useParallel = ShouldUseParallel(m, n, k, settings);

        double[] bColumnsBuffer = ArrayPool<double>.Shared.Rent(b.Length);
        TransposeToColumnVectors(b, k, n, bColumnsBuffer.AsSpan(0, b.Length));

        if (useParallel)
        {
            double[] aBuffer = ArrayPool<double>.Shared.Rent(a.Length);
            double[] cBuffer = ArrayPool<double>.Shared.Rent(c.Length);
            int workerCap = ResolveWorkerCap(m, settings);
            ParallelOptions options = new() { MaxDegreeOfParallelism = workerCap };

            a.CopyTo(aBuffer);
            c.CopyTo(cBuffer);

            try
            {
                Parallel.For(
                    0,
                    m,
                    options,
                    () => ArrayPool<double>.Shared.Rent(n),
                    (i, _, rowScratch) =>
                    {
                        int aRow = i * k;
                        int cRow = i * n;
                        ReadOnlySpan<double> row = aBuffer.AsSpan(aRow, k);
                        Span<double> scratch = rowScratch.AsSpan(0, n);

                        for (int j = 0; j < n; j++)
                        {
                            scratch[j] = PerformancePrimitives.Dot(row, bColumnsBuffer.AsSpan(j * k, k), settings);
                        }

                        for (int j = 0; j < n; j++)
                        {
                            int idx = cRow + j;
                            cBuffer[idx] = (alpha * scratch[j]) + (beta * cBuffer[idx]);
                        }

                        return rowScratch;
                    },
                    rowScratch => ArrayPool<double>.Shared.Return(rowScratch, clearArray: false));

                cBuffer.AsSpan(0, c.Length).CopyTo(c);
            }
            finally
            {
                ArrayPool<double>.Shared.Return(cBuffer, clearArray: false);
                ArrayPool<double>.Shared.Return(aBuffer, clearArray: false);
                ArrayPool<double>.Shared.Return(bColumnsBuffer, clearArray: false);
            }

            return;
        }

        try
        {
            for (int i = 0; i < m; i++)
            {
                int aRow = i * k;
                int cRow = i * n;
                ReadOnlySpan<double> row = a.Slice(aRow, k);

                for (int j = 0; j < n; j++)
                {
                    double sum = PerformancePrimitives.Dot(row, bColumnsBuffer.AsSpan(j * k, k), settings);
                    int idx = cRow + j;
                    c[idx] = (alpha * sum) + (beta * c[idx]);
                }
            }
        }
        finally
        {
            ArrayPool<double>.Shared.Return(bColumnsBuffer, clearArray: false);
        }
    }

    public static void Multiply(
        ReadOnlySpan<float> a,
        ReadOnlySpan<float> b,
        Span<float> c,
        int m,
        int n,
        int k,
        float alpha = 1f,
        float beta = 0f)
    {
        if (m <= 0 || n <= 0 || k <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(m), "m, n, and k must be positive.");
        }

        if (a.Length != m * k)
        {
            throw new ArgumentException("Matrix A size must be m*k.", nameof(a));
        }

        if (b.Length != k * n)
        {
            throw new ArgumentException("Matrix B size must be k*n.", nameof(b));
        }

        if (c.Length != m * n)
        {
            throw new ArgumentException("Matrix C size must be m*n.", nameof(c));
        }

        DataStructurePerformanceSettings settings = DataStructureCompatibility.GetPerformanceSettings();
        bool useParallel = ShouldUseParallel(m, n, k, settings);

        float[] bColumnsBuffer = ArrayPool<float>.Shared.Rent(b.Length);
        TransposeToColumnVectors(b, k, n, bColumnsBuffer.AsSpan(0, b.Length));

        if (useParallel)
        {
            float[] aBuffer = ArrayPool<float>.Shared.Rent(a.Length);
            float[] cBuffer = ArrayPool<float>.Shared.Rent(c.Length);
            int workerCap = ResolveWorkerCap(m, settings);
            ParallelOptions options = new() { MaxDegreeOfParallelism = workerCap };

            a.CopyTo(aBuffer);
            c.CopyTo(cBuffer);

            try
            {
                Parallel.For(
                    0,
                    m,
                    options,
                    () => ArrayPool<float>.Shared.Rent(n),
                    (i, _, rowScratch) =>
                    {
                        int aRow = i * k;
                        int cRow = i * n;
                        ReadOnlySpan<float> row = aBuffer.AsSpan(aRow, k);
                        Span<float> scratch = rowScratch.AsSpan(0, n);

                        for (int j = 0; j < n; j++)
                        {
                            scratch[j] = PerformancePrimitives.Dot(row, bColumnsBuffer.AsSpan(j * k, k), settings);
                        }

                        for (int j = 0; j < n; j++)
                        {
                            int idx = cRow + j;
                            cBuffer[idx] = (alpha * scratch[j]) + (beta * cBuffer[idx]);
                        }

                        return rowScratch;
                    },
                    rowScratch => ArrayPool<float>.Shared.Return(rowScratch, clearArray: false));

                cBuffer.AsSpan(0, c.Length).CopyTo(c);
            }
            finally
            {
                ArrayPool<float>.Shared.Return(cBuffer, clearArray: false);
                ArrayPool<float>.Shared.Return(aBuffer, clearArray: false);
                ArrayPool<float>.Shared.Return(bColumnsBuffer, clearArray: false);
            }

            return;
        }

        try
        {
            for (int i = 0; i < m; i++)
            {
                int aRow = i * k;
                int cRow = i * n;
                ReadOnlySpan<float> row = a.Slice(aRow, k);

                for (int j = 0; j < n; j++)
                {
                    float sum = PerformancePrimitives.Dot(row, bColumnsBuffer.AsSpan(j * k, k), settings);
                    int idx = cRow + j;
                    c[idx] = (alpha * sum) + (beta * c[idx]);
                }
            }
        }
        finally
        {
            ArrayPool<float>.Shared.Return(bColumnsBuffer, clearArray: false);
        }
    }

    public static void Multiply(
        ReadOnlySpan<Complex> a,
        ReadOnlySpan<Complex> b,
        Span<Complex> c,
        int m,
        int n,
        int k,
        Complex alpha,
        Complex beta)
    {
        if (m <= 0 || n <= 0 || k <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(m), "m, n, and k must be positive.");
        }

        if (a.Length != m * k)
        {
            throw new ArgumentException("Matrix A size must be m*k.", nameof(a));
        }

        if (b.Length != k * n)
        {
            throw new ArgumentException("Matrix B size must be k*n.", nameof(b));
        }

        if (c.Length != m * n)
        {
            throw new ArgumentException("Matrix C size must be m*n.", nameof(c));
        }

        DataStructurePerformanceSettings settings = DataStructureCompatibility.GetPerformanceSettings();
        bool useParallel = ShouldUseParallel(m, n, k, settings);

        Complex[] bColumnsBuffer = ArrayPool<Complex>.Shared.Rent(b.Length);
        TransposeToColumnVectors(b, k, n, bColumnsBuffer.AsSpan(0, b.Length));

        if (useParallel)
        {
            Complex[] aBuffer = ArrayPool<Complex>.Shared.Rent(a.Length);
            Complex[] cBuffer = ArrayPool<Complex>.Shared.Rent(c.Length);
            int workerCap = ResolveWorkerCap(m, settings);
            ParallelOptions options = new() { MaxDegreeOfParallelism = workerCap };

            a.CopyTo(aBuffer);
            c.CopyTo(cBuffer);

            try
            {
                Parallel.For(
                    0,
                    m,
                    options,
                    () => ArrayPool<Complex>.Shared.Rent(n),
                    (i, _, rowScratch) =>
                    {
                        int aRow = i * k;
                        int cRow = i * n;
                        ReadOnlySpan<Complex> row = aBuffer.AsSpan(aRow, k);
                        Span<Complex> scratch = rowScratch.AsSpan(0, n);

                        for (int j = 0; j < n; j++)
                        {
                            scratch[j] = DotComplex(row, bColumnsBuffer.AsSpan(j * k, k));
                        }

                        for (int j = 0; j < n; j++)
                        {
                            int idx = cRow + j;
                            cBuffer[idx] = (alpha * scratch[j]) + (beta * cBuffer[idx]);
                        }

                        return rowScratch;
                    },
                    rowScratch => ArrayPool<Complex>.Shared.Return(rowScratch, clearArray: false));

                cBuffer.AsSpan(0, c.Length).CopyTo(c);
            }
            finally
            {
                ArrayPool<Complex>.Shared.Return(cBuffer, clearArray: false);
                ArrayPool<Complex>.Shared.Return(aBuffer, clearArray: false);
                ArrayPool<Complex>.Shared.Return(bColumnsBuffer, clearArray: false);
            }

            return;
        }

        try
        {
            for (int i = 0; i < m; i++)
            {
                int aRow = i * k;
                int cRow = i * n;
                ReadOnlySpan<Complex> row = a.Slice(aRow, k);

                for (int j = 0; j < n; j++)
                {
                    Complex sum = DotComplex(row, bColumnsBuffer.AsSpan(j * k, k));
                    int idx = cRow + j;
                    c[idx] = (alpha * sum) + (beta * c[idx]);
                }
            }
        }
        finally
        {
            ArrayPool<Complex>.Shared.Return(bColumnsBuffer, clearArray: false);
        }
    }

    public static void Multiply(
        ReadOnlySpan<Complex> a,
        ReadOnlySpan<Complex> b,
        Span<Complex> c,
        int m,
        int n,
        int k)
    {
        Multiply(a, b, c, m, n, k, Complex.One, Complex.Zero);
    }

    private static bool ShouldUseParallel(int rows, int cols, int depth, in DataStructurePerformanceSettings settings)
    {
        if (!settings.EnableParallel)
        {
            return false;
        }

        long opCount = (long)rows * cols * depth;
        int effectiveCols = Math.Min(Math.Max(1, cols), 64);
        long strategyThreshold = (long)settings.ParallelLengthThreshold * Math.Max(1, depth) * effectiveCols;
        return rows >= ParallelRowThreshold && opCount >= Math.Max(ParallelOpThreshold, strategyThreshold);
    }

    private static int ResolveWorkerCap(int workItems, in DataStructurePerformanceSettings settings)
    {
        int configured = settings.MaxDegreeOfParallelism ?? Environment.ProcessorCount;
        configured = Math.Max(1, configured);
        return Math.Max(1, Math.Min(configured, workItems));
    }

    private static void TransposeToColumnVectors<T>(ReadOnlySpan<T> source, int rows, int cols, Span<T> destination)
    {
        for (int row = 0; row < rows; row++)
        {
            int sourceRowOffset = row * cols;
            for (int col = 0; col < cols; col++)
            {
                destination[(col * rows) + row] = source[sourceRowOffset + col];
            }
        }
    }

    private static Complex DotComplex(ReadOnlySpan<Complex> left, ReadOnlySpan<Complex> right)
    {
        Complex sum = Complex.Zero;
        for (int i = 0; i < left.Length; i++)
        {
            sum += left[i] * right[i];
        }

        return sum;
    }

    private static double[] GetThreadArrayDoubleColumnsScratch(int minLength)
    {
        double[]? buffer = t_doubleArrayColumnsScratch;
        if (buffer is null || buffer.Length < minLength)
        {
            if (buffer is not null)
            {
                ArrayPool<double>.Shared.Return(buffer, clearArray: false);
            }

            buffer = ArrayPool<double>.Shared.Rent(minLength);
            t_doubleArrayColumnsScratch = buffer;
        }

        return buffer;
    }

    private static double[] GetThreadArrayDoubleRowScratch(int minLength)
    {
        double[]? buffer = t_doubleArrayRowScratch;
        if (buffer is null || buffer.Length < minLength)
        {
            if (buffer is not null)
            {
                ArrayPool<double>.Shared.Return(buffer, clearArray: false);
            }

            buffer = ArrayPool<double>.Shared.Rent(minLength);
            t_doubleArrayRowScratch = buffer;
        }

        return buffer;
    }

}

