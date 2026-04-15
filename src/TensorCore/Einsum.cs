using System.Buffers;
using System.Numerics;
using LAL.LinalgCore;

namespace LAL.TensorCore;

internal static class Einsum
{
    public static double Evaluate(string signature, ReadOnlySpan<double> left, ReadOnlySpan<double> right)
    {
        Span<double> destination = stackalloc double[1];
        Compute(signature, left, right, destination);
        return destination[0];
    }

    public static double[,] EvaluateMatMul(string signature, double[,] left, double[,] right)
    {
        int m = left.GetLength(0);
        int k = left.GetLength(1);
        int rightRows = right.GetLength(0);
        int n = right.GetLength(1);

        if (rightRows != k)
        {
            throw new ArgumentException("Inner matrix dimensions must match.", nameof(right));
        }

        int leftCount = m * k;
        int rightCount = k * n;
        int destinationCount = m * n;

        double[] leftFlat = ArrayPool<double>.Shared.Rent(leftCount);
        double[] rightFlat = ArrayPool<double>.Shared.Rent(rightCount);
        double[] destinationFlat = ArrayPool<double>.Shared.Rent(destinationCount);

        try
        {
            for (int row = 0; row < m; row++)
            {
                int offset = row * k;
                for (int col = 0; col < k; col++)
                {
                    leftFlat[offset + col] = left[row, col];
                }
            }

            for (int row = 0; row < k; row++)
            {
                int offset = row * n;
                for (int col = 0; col < n; col++)
                {
                    rightFlat[offset + col] = right[row, col];
                }
            }

            Compute(
                signature,
                leftFlat.AsSpan(0, leftCount),
                rightFlat.AsSpan(0, rightCount),
                destinationFlat.AsSpan(0, destinationCount),
                m: m,
                n: n,
                k: k);

            double[,] result = new double[m, n];
            for (int row = 0; row < m; row++)
            {
                int offset = row * n;
                for (int col = 0; col < n; col++)
                {
                    result[row, col] = destinationFlat[offset + col];
                }
            }

            return result;
        }
        finally
        {
            ArrayPool<double>.Shared.Return(destinationFlat);
            ArrayPool<double>.Shared.Return(rightFlat);
            ArrayPool<double>.Shared.Return(leftFlat);
        }
    }

    public static float Evaluate(string signature, ReadOnlySpan<float> left, ReadOnlySpan<float> right)
    {
        Span<float> destination = stackalloc float[1];
        Compute(signature, left, right, destination);
        return destination[0];
    }

    public static Complex Evaluate(string signature, ReadOnlySpan<Complex> left, ReadOnlySpan<Complex> right)
    {
        Span<Complex> destination = stackalloc Complex[1];
        Compute(signature, left, right, destination);
        return destination[0];
    }

    public static float[,] EvaluateMatMul(string signature, float[,] left, float[,] right)
    {
        int m = left.GetLength(0);
        int k = left.GetLength(1);
        int rightRows = right.GetLength(0);
        int n = right.GetLength(1);

        if (rightRows != k)
        {
            throw new ArgumentException("Inner matrix dimensions must match.", nameof(right));
        }

        int leftCount = m * k;
        int rightCount = k * n;
        int destinationCount = m * n;

        float[] leftFlat = ArrayPool<float>.Shared.Rent(leftCount);
        float[] rightFlat = ArrayPool<float>.Shared.Rent(rightCount);
        float[] destinationFlat = ArrayPool<float>.Shared.Rent(destinationCount);

        try
        {
            for (int row = 0; row < m; row++)
            {
                int offset = row * k;
                for (int col = 0; col < k; col++)
                {
                    leftFlat[offset + col] = left[row, col];
                }
            }

            for (int row = 0; row < k; row++)
            {
                int offset = row * n;
                for (int col = 0; col < n; col++)
                {
                    rightFlat[offset + col] = right[row, col];
                }
            }

            Compute(
                signature,
                leftFlat.AsSpan(0, leftCount),
                rightFlat.AsSpan(0, rightCount),
                destinationFlat.AsSpan(0, destinationCount),
                m: m,
                n: n,
                k: k);

            float[,] result = new float[m, n];
            for (int row = 0; row < m; row++)
            {
                int offset = row * n;
                for (int col = 0; col < n; col++)
                {
                    result[row, col] = destinationFlat[offset + col];
                }
            }

            return result;
        }
        finally
        {
            ArrayPool<float>.Shared.Return(destinationFlat);
            ArrayPool<float>.Shared.Return(rightFlat);
            ArrayPool<float>.Shared.Return(leftFlat);
        }
    }

    public static Complex[,] EvaluateMatMul(string signature, Complex[,] left, Complex[,] right)
    {
        int m = left.GetLength(0);
        int k = left.GetLength(1);
        int rightRows = right.GetLength(0);
        int n = right.GetLength(1);

        if (rightRows != k)
        {
            throw new ArgumentException("Inner matrix dimensions must match.", nameof(right));
        }

        int leftCount = m * k;
        int rightCount = k * n;
        int destinationCount = m * n;

        Complex[] leftFlat = ArrayPool<Complex>.Shared.Rent(leftCount);
        Complex[] rightFlat = ArrayPool<Complex>.Shared.Rent(rightCount);
        Complex[] destinationFlat = ArrayPool<Complex>.Shared.Rent(destinationCount);

        try
        {
            for (int row = 0; row < m; row++)
            {
                int offset = row * k;
                for (int col = 0; col < k; col++)
                {
                    leftFlat[offset + col] = left[row, col];
                }
            }

            for (int row = 0; row < k; row++)
            {
                int offset = row * n;
                for (int col = 0; col < n; col++)
                {
                    rightFlat[offset + col] = right[row, col];
                }
            }

            Compute(
                signature,
                leftFlat.AsSpan(0, leftCount),
                rightFlat.AsSpan(0, rightCount),
                destinationFlat.AsSpan(0, destinationCount),
                m: m,
                n: n,
                k: k);

            Complex[,] result = new Complex[m, n];
            for (int row = 0; row < m; row++)
            {
                int offset = row * n;
                for (int col = 0; col < n; col++)
                {
                    result[row, col] = destinationFlat[offset + col];
                }
            }

            return result;
        }
        finally
        {
            ArrayPool<Complex>.Shared.Return(destinationFlat);
            ArrayPool<Complex>.Shared.Return(rightFlat);
            ArrayPool<Complex>.Shared.Return(leftFlat);
        }
    }

    public static void Compute(
        string signature,
        ReadOnlySpan<double> left,
        ReadOnlySpan<double> right,
        Span<double> destination,
        int m = 0,
        int n = 0,
        int k = 0)
    {
        if (string.IsNullOrWhiteSpace(signature))
        {
            throw new ArgumentException("Signature must not be empty.", nameof(signature));
        }

        if (signature == "i,i->")
        {
            if (left.Length != right.Length)
            {
                throw new ArgumentException("Dot signature requires equal input lengths.");
            }

            if (destination.Length != 1)
            {
                throw new ArgumentException("Dot signature destination length must be 1.");
            }

            destination[0] = Dot.Dotu(left, right);
            return;
        }

        if (signature == "ij,jk->ik")
        {
            if (m <= 0 || n <= 0 || k <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(m), "m,n,k must be positive for matrix signature.");
            }

            if (left.Length != m * k || right.Length != k * n || destination.Length != m * n)
            {
                throw new ArgumentException("Input lengths do not match matrix dimensions.");
            }

            Gemm.Multiply(left, right, destination, m, n, k);

            return;
        }

        throw new NotSupportedException($"Unsupported einsum signature: {signature}");
    }

    public static void Compute(
        string signature,
        ReadOnlySpan<float> left,
        ReadOnlySpan<float> right,
        Span<float> destination,
        int m = 0,
        int n = 0,
        int k = 0)
    {
        if (string.IsNullOrWhiteSpace(signature))
        {
            throw new ArgumentException("Signature must not be empty.", nameof(signature));
        }

        if (signature == "i,i->")
        {
            if (left.Length != right.Length)
            {
                throw new ArgumentException("Dot signature requires equal input lengths.");
            }

            if (destination.Length != 1)
            {
                throw new ArgumentException("Dot signature destination length must be 1.");
            }

            destination[0] = Dot.Dotu(left, right);
            return;
        }

        if (signature == "ij,jk->ik")
        {
            if (m <= 0 || n <= 0 || k <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(m), "m,n,k must be positive for matrix signature.");
            }

            if (left.Length != m * k || right.Length != k * n || destination.Length != m * n)
            {
                throw new ArgumentException("Input lengths do not match matrix dimensions.");
            }

            Gemm.Multiply(left, right, destination, m, n, k);

            return;
        }

        throw new NotSupportedException($"Unsupported einsum signature: {signature}");
    }

    public static void Compute(
        string signature,
        ReadOnlySpan<Complex> left,
        ReadOnlySpan<Complex> right,
        Span<Complex> destination,
        int m = 0,
        int n = 0,
        int k = 0)
    {
        if (string.IsNullOrWhiteSpace(signature))
        {
            throw new ArgumentException("Signature must not be empty.", nameof(signature));
        }

        if (signature == "i,i->")
        {
            if (left.Length != right.Length)
            {
                throw new ArgumentException("Dot signature requires equal input lengths.");
            }

            if (destination.Length != 1)
            {
                throw new ArgumentException("Dot signature destination length must be 1.");
            }

            destination[0] = Dot.Dotu(left, right);
            return;
        }

        if (signature == "ij,jk->ik")
        {
            if (m <= 0 || n <= 0 || k <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(m), "m,n,k must be positive for matrix signature.");
            }

            if (left.Length != m * k || right.Length != k * n || destination.Length != m * n)
            {
                throw new ArgumentException("Input lengths do not match matrix dimensions.");
            }

            Gemm.Multiply(left, right, destination, m, n, k);

            return;
        }

        throw new NotSupportedException($"Unsupported einsum signature: {signature}");
    }

    public static void TensorContractionGemm(
        ReadOnlySpan<double> left,
        ReadOnlySpan<double> right,
        Span<double> destination,
        int m,
        int n,
        int k)
    {
        Compute("ij,jk->ik", left, right, destination, m, n, k);
    }

    public static void TensorContractionGemm(
        ReadOnlySpan<float> left,
        ReadOnlySpan<float> right,
        Span<float> destination,
        int m,
        int n,
        int k)
    {
        Compute("ij,jk->ik", left, right, destination, m, n, k);
    }

    public static void TensorContractionGemm(
        ReadOnlySpan<Complex> left,
        ReadOnlySpan<Complex> right,
        Span<Complex> destination,
        int m,
        int n,
        int k)
    {
        Compute("ij,jk->ik", left, right, destination, m, n, k);
    }

    public static void Kronecker(
        ReadOnlySpan<double> left,
        int leftRows,
        int leftCols,
        ReadOnlySpan<double> right,
        int rightRows,
        int rightCols,
        Span<double> destination)
    {
        if (leftRows <= 0 || leftCols <= 0 || rightRows <= 0 || rightCols <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(leftRows), "Matrix dimensions must be positive.");
        }

        if (left.Length != leftRows * leftCols)
        {
            throw new ArgumentException("Left length does not match leftRows*leftCols.", nameof(left));
        }

        if (right.Length != rightRows * rightCols)
        {
            throw new ArgumentException("Right length does not match rightRows*rightCols.", nameof(right));
        }

        int dstRows = leftRows * rightRows;
        int dstCols = leftCols * rightCols;
        if (destination.Length != dstRows * dstCols)
        {
            throw new ArgumentException("Destination length does not match Kronecker output shape.", nameof(destination));
        }

        for (int lr = 0; lr < leftRows; lr++)
        {
            int leftRowOffset = lr * leftCols;
            for (int lc = 0; lc < leftCols; lc++)
            {
                double scale = left[leftRowOffset + lc];

                for (int rr = 0; rr < rightRows; rr++)
                {
                    int dstRow = (lr * rightRows) + rr;
                    int dstOffset = dstRow * dstCols + (lc * rightCols);
                    int rightOffset = rr * rightCols;

                    for (int rc = 0; rc < rightCols; rc++)
                    {
                        destination[dstOffset + rc] = scale * right[rightOffset + rc];
                    }
                }
            }
        }
    }

    public static void Kronecker(
        ReadOnlySpan<float> left,
        int leftRows,
        int leftCols,
        ReadOnlySpan<float> right,
        int rightRows,
        int rightCols,
        Span<float> destination)
    {
        if (leftRows <= 0 || leftCols <= 0 || rightRows <= 0 || rightCols <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(leftRows), "Matrix dimensions must be positive.");
        }

        if (left.Length != leftRows * leftCols)
        {
            throw new ArgumentException("Left length does not match leftRows*leftCols.", nameof(left));
        }

        if (right.Length != rightRows * rightCols)
        {
            throw new ArgumentException("Right length does not match rightRows*rightCols.", nameof(right));
        }

        int dstRows = leftRows * rightRows;
        int dstCols = leftCols * rightCols;
        if (destination.Length != dstRows * dstCols)
        {
            throw new ArgumentException("Destination length does not match Kronecker output shape.", nameof(destination));
        }

        for (int lr = 0; lr < leftRows; lr++)
        {
            int leftRowOffset = lr * leftCols;
            for (int lc = 0; lc < leftCols; lc++)
            {
                float scale = left[leftRowOffset + lc];

                for (int rr = 0; rr < rightRows; rr++)
                {
                    int dstRow = (lr * rightRows) + rr;
                    int dstOffset = dstRow * dstCols + (lc * rightCols);
                    int rightOffset = rr * rightCols;

                    for (int rc = 0; rc < rightCols; rc++)
                    {
                        destination[dstOffset + rc] = scale * right[rightOffset + rc];
                    }
                }
            }
        }
    }

    public static void Kronecker(
        ReadOnlySpan<Complex> left,
        int leftRows,
        int leftCols,
        ReadOnlySpan<Complex> right,
        int rightRows,
        int rightCols,
        Span<Complex> destination)
    {
        if (leftRows <= 0 || leftCols <= 0 || rightRows <= 0 || rightCols <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(leftRows), "Matrix dimensions must be positive.");
        }

        if (left.Length != leftRows * leftCols)
        {
            throw new ArgumentException("Left length does not match leftRows*leftCols.", nameof(left));
        }

        if (right.Length != rightRows * rightCols)
        {
            throw new ArgumentException("Right length does not match rightRows*rightCols.", nameof(right));
        }

        int dstRows = leftRows * rightRows;
        int dstCols = leftCols * rightCols;
        if (destination.Length != dstRows * dstCols)
        {
            throw new ArgumentException("Destination length does not match Kronecker output shape.", nameof(destination));
        }

        for (int lr = 0; lr < leftRows; lr++)
        {
            int leftRowOffset = lr * leftCols;
            for (int lc = 0; lc < leftCols; lc++)
            {
                Complex scale = left[leftRowOffset + lc];

                for (int rr = 0; rr < rightRows; rr++)
                {
                    int dstRow = (lr * rightRows) + rr;
                    int dstOffset = dstRow * dstCols + (lc * rightCols);
                    int rightOffset = rr * rightCols;

                    for (int rc = 0; rc < rightCols; rc++)
                    {
                        destination[dstOffset + rc] = scale * right[rightOffset + rc];
                    }
                }
            }
        }
    }
}

