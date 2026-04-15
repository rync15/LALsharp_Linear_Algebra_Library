using System.Buffers;
using System.Numerics;

namespace LAL.LinalgCore;

internal static class DistanceMetrics
{
    private const int StackallocThreshold = 128;

    public static void PairwiseEuclidean(ReadOnlySpan<double> points, int pointCount, int dimension, Span<double> distances)
    {
        ValidatePairwiseInputs(points, pointCount, dimension, distances);

        for (int i = 0; i < pointCount; i++)
        {
            distances[(i * pointCount) + i] = 0d;
            ReadOnlySpan<double> pi = points.Slice(i * dimension, dimension);

            for (int j = i + 1; j < pointCount; j++)
            {
                ReadOnlySpan<double> pj = points.Slice(j * dimension, dimension);
                double sum = 0d;
                for (int k = 0; k < dimension; k++)
                {
                    double d = pi[k] - pj[k];
                    sum += d * d;
                }

                double dist = Math.Sqrt(sum);
                distances[(i * pointCount) + j] = dist;
                distances[(j * pointCount) + i] = dist;
            }
        }
    }

    public static void PairwiseCosine(ReadOnlySpan<double> points, int pointCount, int dimension, Span<double> distances)
    {
        ValidatePairwiseInputs(points, pointCount, dimension, distances);

        double[] norms = new double[pointCount];
        for (int i = 0; i < pointCount; i++)
        {
            ReadOnlySpan<double> p = points.Slice(i * dimension, dimension);
            norms[i] = Norms.L2(p);
            distances[(i * pointCount) + i] = 0d;
        }

        for (int i = 0; i < pointCount; i++)
        {
            ReadOnlySpan<double> pi = points.Slice(i * dimension, dimension);
            for (int j = i + 1; j < pointCount; j++)
            {
                ReadOnlySpan<double> pj = points.Slice(j * dimension, dimension);
                double denom = norms[i] * norms[j];
                double cosine = denom <= double.Epsilon ? 0d : global::LAL.LinalgCore.Dot.Dotu(pi, pj) / denom;
                cosine = Math.Clamp(cosine, -1d, 1d);

                double distance = 1d - cosine;
                distances[(i * pointCount) + j] = distance;
                distances[(j * pointCount) + i] = distance;
            }
        }
    }

    public static void PairwiseMahalanobis(
        ReadOnlySpan<double> points,
        int pointCount,
        int dimension,
        ReadOnlySpan<double> inverseCovariance,
        Span<double> distances)
    {
        ValidatePairwiseInputs(points, pointCount, dimension, distances);

        if (inverseCovariance.Length != dimension * dimension)
        {
            throw new ArgumentException("Inverse covariance length must be dimension*dimension.", nameof(inverseCovariance));
        }

        double[]? rented = null;
        Span<double> delta = dimension <= StackallocThreshold
            ? stackalloc double[dimension]
            : (rented = ArrayPool<double>.Shared.Rent(dimension)).AsSpan(0, dimension);

        try
        {
            for (int i = 0; i < pointCount; i++)
            {
                distances[(i * pointCount) + i] = 0d;
                ReadOnlySpan<double> pi = points.Slice(i * dimension, dimension);

                for (int j = i + 1; j < pointCount; j++)
                {
                    ReadOnlySpan<double> pj = points.Slice(j * dimension, dimension);

                    for (int k = 0; k < dimension; k++)
                    {
                        delta[k] = pi[k] - pj[k];
                    }

                    double q = QuadraticForm(delta, inverseCovariance, dimension);
                    q = Math.Max(0d, q);
                    double distance = Math.Sqrt(q);

                    distances[(i * pointCount) + j] = distance;
                    distances[(j * pointCount) + i] = distance;
                }
            }
        }
        finally
        {
            if (rented is not null)
            {
                ArrayPool<double>.Shared.Return(rented, clearArray: false);
            }
        }
    }

    public static void PairwiseEuclidean(ReadOnlySpan<Complex> points, int pointCount, int dimension, Span<double> distances)
    {
        ValidatePairwiseInputs(points, pointCount, dimension, distances);

        for (int i = 0; i < pointCount; i++)
        {
            distances[(i * pointCount) + i] = 0d;
            ReadOnlySpan<Complex> pi = points.Slice(i * dimension, dimension);

            for (int j = i + 1; j < pointCount; j++)
            {
                ReadOnlySpan<Complex> pj = points.Slice(j * dimension, dimension);
                double sum = 0d;
                for (int k = 0; k < dimension; k++)
                {
                    double magnitude = Complex.Abs(pi[k] - pj[k]);
                    sum += magnitude * magnitude;
                }

                double dist = Math.Sqrt(sum);
                distances[(i * pointCount) + j] = dist;
                distances[(j * pointCount) + i] = dist;
            }
        }
    }

    public static void PairwiseCosine(ReadOnlySpan<Complex> points, int pointCount, int dimension, Span<double> distances)
    {
        ValidatePairwiseInputs(points, pointCount, dimension, distances);

        double[] norms = new double[pointCount];
        for (int i = 0; i < pointCount; i++)
        {
            ReadOnlySpan<Complex> p = points.Slice(i * dimension, dimension);
            norms[i] = Norms.L2(p);
            distances[(i * pointCount) + i] = 0d;
        }

        for (int i = 0; i < pointCount; i++)
        {
            ReadOnlySpan<Complex> pi = points.Slice(i * dimension, dimension);
            for (int j = i + 1; j < pointCount; j++)
            {
                ReadOnlySpan<Complex> pj = points.Slice(j * dimension, dimension);
                double denom = norms[i] * norms[j];
                double cosine = denom <= double.Epsilon ? 0d : Dot.Dotc(pi, pj).Magnitude / denom;
                cosine = Math.Clamp(cosine, 0d, 1d);

                double distance = 1d - cosine;
                distances[(i * pointCount) + j] = distance;
                distances[(j * pointCount) + i] = distance;
            }
        }
    }

    public static void PairwiseMahalanobis(
        ReadOnlySpan<Complex> points,
        int pointCount,
        int dimension,
        ReadOnlySpan<Complex> inverseCovariance,
        Span<double> distances)
    {
        ValidatePairwiseInputs(points, pointCount, dimension, distances);

        if (inverseCovariance.Length != dimension * dimension)
        {
            throw new ArgumentException("Inverse covariance length must be dimension*dimension.", nameof(inverseCovariance));
        }

        Complex[]? rented = null;
        Span<Complex> delta = dimension <= StackallocThreshold
            ? stackalloc Complex[dimension]
            : (rented = ArrayPool<Complex>.Shared.Rent(dimension)).AsSpan(0, dimension);

        try
        {
            for (int i = 0; i < pointCount; i++)
            {
                distances[(i * pointCount) + i] = 0d;
                ReadOnlySpan<Complex> pi = points.Slice(i * dimension, dimension);

                for (int j = i + 1; j < pointCount; j++)
                {
                    ReadOnlySpan<Complex> pj = points.Slice(j * dimension, dimension);

                    for (int k = 0; k < dimension; k++)
                    {
                        delta[k] = pi[k] - pj[k];
                    }

                    Complex q = QuadraticForm(delta, inverseCovariance, dimension);
                    double distance = Math.Sqrt(Math.Max(0d, q.Real));

                    distances[(i * pointCount) + j] = distance;
                    distances[(j * pointCount) + i] = distance;
                }
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

    private static double QuadraticForm(ReadOnlySpan<double> v, ReadOnlySpan<double> m, int n)
    {
        double sum = 0d;
        for (int r = 0; r < n; r++)
        {
            double rowSum = 0d;
            int offset = r * n;
            for (int c = 0; c < n; c++)
            {
                rowSum += m[offset + c] * v[c];
            }

            sum += v[r] * rowSum;
        }

        return sum;
    }

    private static Complex QuadraticForm(ReadOnlySpan<Complex> v, ReadOnlySpan<Complex> m, int n)
    {
        Complex sum = Complex.Zero;
        for (int r = 0; r < n; r++)
        {
            Complex rowSum = Complex.Zero;
            int offset = r * n;
            for (int c = 0; c < n; c++)
            {
                rowSum += m[offset + c] * v[c];
            }

            sum += Complex.Conjugate(v[r]) * rowSum;
        }

        return sum;
    }

    private static void ValidatePairwiseInputs(ReadOnlySpan<double> points, int pointCount, int dimension, Span<double> distances)
    {
        if (pointCount <= 0 || dimension <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pointCount), "Point count and dimension must be positive.");
        }

        if (points.Length != pointCount * dimension)
        {
            throw new ArgumentException("Points length must be pointCount * dimension.", nameof(points));
        }

        if (distances.Length != pointCount * pointCount)
        {
            throw new ArgumentException("Distances length must be pointCount * pointCount.", nameof(distances));
        }
    }

    private static void ValidatePairwiseInputs(ReadOnlySpan<Complex> points, int pointCount, int dimension, Span<double> distances)
    {
        if (pointCount <= 0 || dimension <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pointCount), "Point count and dimension must be positive.");
        }

        if (points.Length != pointCount * dimension)
        {
            throw new ArgumentException("Points length must be pointCount * dimension.", nameof(points));
        }

        if (distances.Length != pointCount * pointCount)
        {
            throw new ArgumentException("Distances length must be pointCount * pointCount.", nameof(distances));
        }
    }
}
