using System.Numerics;

namespace LAL.LinalgCore;

internal static class Transpose
{
    public static void Matrix(ReadOnlySpan<double> input, int rows, int cols, Span<double> output)
    {
        ValidateShape(input.Length, output.Length, rows, cols);

        for (int r = 0; r < rows; r++)
        {
            int rowOffset = r * cols;
            for (int c = 0; c < cols; c++)
            {
                output[c * rows + r] = input[rowOffset + c];
            }
        }
    }

    public static void Matrix(ReadOnlySpan<float> input, int rows, int cols, Span<float> output)
    {
        ValidateShape(input.Length, output.Length, rows, cols);

        for (int r = 0; r < rows; r++)
        {
            int rowOffset = r * cols;
            for (int c = 0; c < cols; c++)
            {
                output[c * rows + r] = input[rowOffset + c];
            }
        }
    }

    public static void Matrix(ReadOnlySpan<Complex> input, int rows, int cols, Span<Complex> output)
    {
        ValidateShape(input.Length, output.Length, rows, cols);

        for (int r = 0; r < rows; r++)
        {
            int rowOffset = r * cols;
            for (int c = 0; c < cols; c++)
            {
                output[c * rows + r] = input[rowOffset + c];
            }
        }
    }

    public static void ConjugateTranspose(ReadOnlySpan<Complex> input, int rows, int cols, Span<Complex> output)
    {
        ValidateShape(input.Length, output.Length, rows, cols);

        for (int r = 0; r < rows; r++)
        {
            int rowOffset = r * cols;
            for (int c = 0; c < cols; c++)
            {
                output[c * rows + r] = Complex.Conjugate(input[rowOffset + c]);
            }
        }
    }

    private static void ValidateShape(int inputLength, int outputLength, int rows, int cols)
    {
        if (rows <= 0 || cols <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(rows), "Rows and cols must be positive.");
        }

        if (inputLength != rows * cols)
        {
            throw new ArgumentException("Input matrix size does not match rows*cols.", nameof(inputLength));
        }

        if (outputLength != rows * cols)
        {
            throw new ArgumentException("Output matrix size does not match rows*cols.", nameof(outputLength));
        }
    }
}

