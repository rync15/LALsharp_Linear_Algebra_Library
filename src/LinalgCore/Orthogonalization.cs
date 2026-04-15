using System.Buffers;
using System.Numerics;

namespace LAL.LinalgCore;

internal static class Orthogonalization
{
    private const int StackallocThreshold = 128;

    public static double ProjectionCoefficient(ReadOnlySpan<double> vector, ReadOnlySpan<double> basis)
    {
        if (vector.Length != basis.Length)
        {
            throw new ArgumentException("Vector and basis lengths must match.");
        }

        double denom = global::LAL.LinalgCore.Dot.Dotu(basis, basis);
        if (denom <= double.Epsilon)
        {
            throw new ArgumentException("Basis vector must be non-zero.", nameof(basis));
        }

        return global::LAL.LinalgCore.Dot.Dotu(vector, basis) / denom;
    }

    public static Complex ProjectionCoefficient(ReadOnlySpan<Complex> vector, ReadOnlySpan<Complex> basis)
    {
        if (vector.Length != basis.Length)
        {
            throw new ArgumentException("Vector and basis lengths must match.");
        }

        Complex denom = global::LAL.LinalgCore.Dot.Dotc(basis, basis);
        if (denom.Magnitude <= double.Epsilon)
        {
            throw new ArgumentException("Basis vector must be non-zero.", nameof(basis));
        }

        return global::LAL.LinalgCore.Dot.Dotc(basis, vector) / denom;
    }

    public static void OrthogonalProjection(ReadOnlySpan<double> vector, ReadOnlySpan<double> basis, Span<double> destination)
    {
        if (vector.Length != basis.Length || destination.Length != vector.Length)
        {
            throw new ArgumentException("Vector, basis, and destination lengths must match.");
        }

        double coeff = ProjectionCoefficient(vector, basis);
        for (int i = 0; i < destination.Length; i++)
        {
            destination[i] = coeff * basis[i];
        }
    }

    public static void OrthogonalProjection(ReadOnlySpan<Complex> vector, ReadOnlySpan<Complex> basis, Span<Complex> destination)
    {
        if (vector.Length != basis.Length || destination.Length != vector.Length)
        {
            throw new ArgumentException("Vector, basis, and destination lengths must match.");
        }

        Complex coeff = ProjectionCoefficient(vector, basis);
        for (int i = 0; i < destination.Length; i++)
        {
            destination[i] = coeff * basis[i];
        }
    }

    public static int GramSchmidtOrthonormalize(
        ReadOnlySpan<double> vectors,
        int vectorCount,
        int dimension,
        Span<double> orthonormalVectors,
        double tolerance = 1e-10)
    {
        if (vectorCount <= 0 || dimension <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(vectorCount), "Vector count and dimension must be positive.");
        }

        if (vectors.Length != vectorCount * dimension)
        {
            throw new ArgumentException("Input vector storage length mismatch.", nameof(vectors));
        }

        if (orthonormalVectors.Length < vectorCount * dimension)
        {
            throw new ArgumentException("Output storage must fit vectorCount * dimension.", nameof(orthonormalVectors));
        }

        if (tolerance <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tolerance), "Tolerance must be positive.");
        }

        double[]? rented = null;
        Span<double> candidate = dimension <= StackallocThreshold
            ? stackalloc double[dimension]
            : (rented = ArrayPool<double>.Shared.Rent(dimension)).AsSpan(0, dimension);

        int rank = 0;

        try
        {
            for (int i = 0; i < vectorCount; i++)
            {
                ReadOnlySpan<double> current = vectors.Slice(i * dimension, dimension);
                current.CopyTo(candidate);

                for (int j = 0; j < rank; j++)
                {
                    ReadOnlySpan<double> q = orthonormalVectors.Slice(j * dimension, dimension);
                    double coeff = global::LAL.LinalgCore.Dot.Dotu(candidate, q);
                    for (int k = 0; k < dimension; k++)
                    {
                        candidate[k] -= coeff * q[k];
                    }
                }

                double norm = Norms.L2(candidate);
                if (norm <= tolerance)
                {
                    continue;
                }

                Span<double> destination = orthonormalVectors.Slice(rank * dimension, dimension);
                for (int k = 0; k < dimension; k++)
                {
                    destination[k] = candidate[k] / norm;
                }

                rank++;
            }
        }
        finally
        {
            if (rented is not null)
            {
                ArrayPool<double>.Shared.Return(rented, clearArray: false);
            }
        }

        return rank;
    }

    public static int GramSchmidtOrthonormalize(
        ReadOnlySpan<Complex> vectors,
        int vectorCount,
        int dimension,
        Span<Complex> orthonormalVectors,
        double tolerance = 1e-10)
    {
        if (vectorCount <= 0 || dimension <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(vectorCount), "Vector count and dimension must be positive.");
        }

        if (vectors.Length != vectorCount * dimension)
        {
            throw new ArgumentException("Input vector storage length mismatch.", nameof(vectors));
        }

        if (orthonormalVectors.Length < vectorCount * dimension)
        {
            throw new ArgumentException("Output storage must fit vectorCount * dimension.", nameof(orthonormalVectors));
        }

        if (tolerance <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tolerance), "Tolerance must be positive.");
        }

        Complex[]? rented = null;
        Span<Complex> candidate = dimension <= StackallocThreshold
            ? stackalloc Complex[dimension]
            : (rented = ArrayPool<Complex>.Shared.Rent(dimension)).AsSpan(0, dimension);

        int rank = 0;

        try
        {
            for (int i = 0; i < vectorCount; i++)
            {
                ReadOnlySpan<Complex> current = vectors.Slice(i * dimension, dimension);
                current.CopyTo(candidate);

                for (int j = 0; j < rank; j++)
                {
                    ReadOnlySpan<Complex> q = orthonormalVectors.Slice(j * dimension, dimension);
                    Complex coeff = global::LAL.LinalgCore.Dot.Dotc(q, candidate);
                    for (int k = 0; k < dimension; k++)
                    {
                        candidate[k] -= coeff * q[k];
                    }
                }

                double norm = Norms.L2(candidate);
                if (norm <= tolerance)
                {
                    continue;
                }

                Span<Complex> destination = orthonormalVectors.Slice(rank * dimension, dimension);
                for (int k = 0; k < dimension; k++)
                {
                    destination[k] = candidate[k] / norm;
                }

                rank++;
            }
        }
        finally
        {
            if (rented is not null)
            {
                ArrayPool<Complex>.Shared.Return(rented, clearArray: false);
            }
        }

        return rank;
    }
}
