using System.Numerics;

namespace LAL.LinalgCore.Sparse;

internal static class Spmv
{
    public static void CscMultiply(
        ReadOnlySpan<double> values,
        ReadOnlySpan<int> rowIndices,
        ReadOnlySpan<int> colPointers,
        ReadOnlySpan<double> x,
        Span<double> y)
    {
        if (values.Length != rowIndices.Length)
        {
            throw new ArgumentException("Values and row indices must have the same length.");
        }

        if (colPointers.Length != x.Length + 1)
        {
            throw new ArgumentException("Column pointers length must be colCount + 1.", nameof(colPointers));
        }

        if (colPointers[0] != 0 || colPointers[colPointers.Length - 1] != values.Length)
        {
            throw new ArgumentException("Column pointer bounds are inconsistent with nnz.", nameof(colPointers));
        }

        y.Clear();

        for (int col = 0; col < x.Length; col++)
        {
            int start = colPointers[col];
            int end = colPointers[col + 1];
            if (start < 0 || end < start || end > values.Length)
            {
                throw new ArgumentException("Column pointer entries must be monotonic and within nnz bounds.", nameof(colPointers));
            }

            double xValue = x[col];
            for (int idx = start; idx < end; idx++)
            {
                int row = rowIndices[idx];
                if (row < 0 || row >= y.Length)
                {
                    throw new ArgumentException("Row index out of range for output vector.", nameof(rowIndices));
                }

                y[row] += values[idx] * xValue;
            }
        }
    }

    public static void CscMultiply(
        ReadOnlySpan<Complex> values,
        ReadOnlySpan<int> rowIndices,
        ReadOnlySpan<int> colPointers,
        ReadOnlySpan<Complex> x,
        Span<Complex> y)
    {
        if (values.Length != rowIndices.Length)
        {
            throw new ArgumentException("Values and row indices must have the same length.");
        }

        if (colPointers.Length != x.Length + 1)
        {
            throw new ArgumentException("Column pointers length must be colCount + 1.", nameof(colPointers));
        }

        if (colPointers[0] != 0 || colPointers[colPointers.Length - 1] != values.Length)
        {
            throw new ArgumentException("Column pointer bounds are inconsistent with nnz.", nameof(colPointers));
        }

        y.Clear();

        for (int col = 0; col < x.Length; col++)
        {
            int start = colPointers[col];
            int end = colPointers[col + 1];
            if (start < 0 || end < start || end > values.Length)
            {
                throw new ArgumentException("Column pointer entries must be monotonic and within nnz bounds.", nameof(colPointers));
            }

            Complex xValue = x[col];
            for (int idx = start; idx < end; idx++)
            {
                int row = rowIndices[idx];
                if (row < 0 || row >= y.Length)
                {
                    throw new ArgumentException("Row index out of range for output vector.", nameof(rowIndices));
                }

                y[row] += values[idx] * xValue;
            }
        }
    }

    public static void CsrMultiply(
        ReadOnlySpan<double> values,
        ReadOnlySpan<int> colIndices,
        ReadOnlySpan<int> rowPointers,
        ReadOnlySpan<double> x,
        Span<double> y)
    {
        if (values.Length != colIndices.Length)
        {
            throw new ArgumentException("Values and column indices must have the same length.");
        }

        if (rowPointers.Length != y.Length + 1)
        {
            throw new ArgumentException("Row pointers length must be rowCount + 1.", nameof(rowPointers));
        }

        if (rowPointers[0] != 0 || rowPointers[rowPointers.Length - 1] != values.Length)
        {
            throw new ArgumentException("Row pointer bounds are inconsistent with nnz.", nameof(rowPointers));
        }

        for (int row = 0; row < y.Length; row++)
        {
            int start = rowPointers[row];
            int end = rowPointers[row + 1];
            if (start < 0 || end < start || end > values.Length)
            {
                throw new ArgumentException("Row pointer entries must be monotonic and within nnz bounds.", nameof(rowPointers));
            }

            double sum = 0d;
            for (int idx = start; idx < end; idx++)
            {
                int col = colIndices[idx];
                if (col < 0 || col >= x.Length)
                {
                    throw new ArgumentException("Column index out of range for input vector.", nameof(colIndices));
                }

                sum += values[idx] * x[col];
            }

            y[row] = sum;
        }
    }

    public static void CsrMultiply(
        ReadOnlySpan<Complex> values,
        ReadOnlySpan<int> colIndices,
        ReadOnlySpan<int> rowPointers,
        ReadOnlySpan<Complex> x,
        Span<Complex> y)
    {
        if (values.Length != colIndices.Length)
        {
            throw new ArgumentException("Values and column indices must have the same length.");
        }

        if (rowPointers.Length != y.Length + 1)
        {
            throw new ArgumentException("Row pointers length must be rowCount + 1.", nameof(rowPointers));
        }

        if (rowPointers[0] != 0 || rowPointers[rowPointers.Length - 1] != values.Length)
        {
            throw new ArgumentException("Row pointer bounds are inconsistent with nnz.", nameof(rowPointers));
        }

        for (int row = 0; row < y.Length; row++)
        {
            int start = rowPointers[row];
            int end = rowPointers[row + 1];
            if (start < 0 || end < start || end > values.Length)
            {
                throw new ArgumentException("Row pointer entries must be monotonic and within nnz bounds.", nameof(rowPointers));
            }

            Complex sum = Complex.Zero;
            for (int idx = start; idx < end; idx++)
            {
                int col = colIndices[idx];
                if (col < 0 || col >= x.Length)
                {
                    throw new ArgumentException("Column index out of range for input vector.", nameof(colIndices));
                }

                sum += values[idx] * x[col];
            }

            y[row] = sum;
        }
    }
}

