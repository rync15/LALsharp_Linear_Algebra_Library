using System.Buffers;
using System.Numerics;

namespace LAL.TensorCore;

internal static class Fft
{
    private const int StackallocComplexThreshold = 256;

    public static void Forward(ReadOnlySpan<Complex> input, Span<Complex> output)
    {
        Forward1D(input, output);
    }

    public static void Forward1D(ReadOnlySpan<Complex> input, Span<Complex> output)
    {
        Transform(input, output, inverse: false);
    }

    public static void Inverse(ReadOnlySpan<Complex> input, Span<Complex> output)
    {
        Inverse1D(input, output);
    }

    public static void Inverse1D(ReadOnlySpan<Complex> input, Span<Complex> output)
    {
        Transform(input, output, inverse: true);
    }

    public static void Forward2D(ReadOnlySpan<Complex> input, int rows, int cols, Span<Complex> output)
    {
        int[] shape = [rows, cols];
        ForwardND(input, shape, output);
    }

    public static void Inverse2D(ReadOnlySpan<Complex> input, int rows, int cols, Span<Complex> output)
    {
        int[] shape = [rows, cols];
        InverseND(input, shape, output);
    }

    public static void ForwardND(ReadOnlySpan<Complex> input, ReadOnlySpan<int> shape, Span<Complex> output)
    {
        TransformND(input, shape, output, inverse: false);
    }

    public static void InverseND(ReadOnlySpan<Complex> input, ReadOnlySpan<int> shape, Span<Complex> output)
    {
        TransformND(input, shape, output, inverse: true);
    }

    public static void Rfft(ReadOnlySpan<double> input, Span<Complex> output)
    {
        if (output.Length != input.Length)
        {
            throw new ArgumentException("Output length must match input length.", nameof(output));
        }

        if (input.Length <= StackallocComplexThreshold)
        {
            Span<Complex> complexInput = stackalloc Complex[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                complexInput[i] = new Complex(input[i], 0d);
            }

            Forward1D(complexInput, output);
            return;
        }

        Complex[] rented = ArrayPool<Complex>.Shared.Rent(input.Length);
        try
        {
            Span<Complex> complexInput = rented.AsSpan(0, input.Length);
            for (int i = 0; i < input.Length; i++)
            {
                complexInput[i] = new Complex(input[i], 0d);
            }

            Forward1D(complexInput, output);
        }
        finally
        {
            ArrayPool<Complex>.Shared.Return(rented);
        }
    }

    public static void Rfft(ReadOnlySpan<float> input, Span<Complex> output)
    {
        if (output.Length != input.Length)
        {
            throw new ArgumentException("Output length must match input length.", nameof(output));
        }

        if (input.Length <= StackallocComplexThreshold)
        {
            Span<Complex> complexInput = stackalloc Complex[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                complexInput[i] = new Complex(input[i], 0d);
            }

            Forward1D(complexInput, output);
            return;
        }

        Complex[] rented = ArrayPool<Complex>.Shared.Rent(input.Length);
        try
        {
            Span<Complex> complexInput = rented.AsSpan(0, input.Length);
            for (int i = 0; i < input.Length; i++)
            {
                complexInput[i] = new Complex(input[i], 0d);
            }

            Forward1D(complexInput, output);
        }
        finally
        {
            ArrayPool<Complex>.Shared.Return(rented);
        }
    }

    public static void RfftND(ReadOnlySpan<double> input, ReadOnlySpan<int> shape, Span<Complex> output)
    {
        int expectedLength = GetElementCount(shape);
        if (input.Length != expectedLength || output.Length != expectedLength)
        {
            throw new ArgumentException("Input/output lengths must match product of shape dimensions.");
        }

        if (expectedLength <= StackallocComplexThreshold)
        {
            Span<Complex> complexInput = stackalloc Complex[expectedLength];
            for (int i = 0; i < expectedLength; i++)
            {
                complexInput[i] = new Complex(input[i], 0d);
            }

            ForwardND(complexInput, shape, output);
            return;
        }

        Complex[] rented = ArrayPool<Complex>.Shared.Rent(expectedLength);
        try
        {
            Span<Complex> complexInput = rented.AsSpan(0, expectedLength);
            for (int i = 0; i < expectedLength; i++)
            {
                complexInput[i] = new Complex(input[i], 0d);
            }

            ForwardND(complexInput, shape, output);
        }
        finally
        {
            ArrayPool<Complex>.Shared.Return(rented);
        }
    }

    public static void RfftND(ReadOnlySpan<float> input, ReadOnlySpan<int> shape, Span<Complex> output)
    {
        int expectedLength = GetElementCount(shape);
        if (input.Length != expectedLength || output.Length != expectedLength)
        {
            throw new ArgumentException("Input/output lengths must match product of shape dimensions.");
        }

        if (expectedLength <= StackallocComplexThreshold)
        {
            Span<Complex> complexInput = stackalloc Complex[expectedLength];
            for (int i = 0; i < expectedLength; i++)
            {
                complexInput[i] = new Complex(input[i], 0d);
            }

            ForwardND(complexInput, shape, output);
            return;
        }

        Complex[] rented = ArrayPool<Complex>.Shared.Rent(expectedLength);
        try
        {
            Span<Complex> complexInput = rented.AsSpan(0, expectedLength);
            for (int i = 0; i < expectedLength; i++)
            {
                complexInput[i] = new Complex(input[i], 0d);
            }

            ForwardND(complexInput, shape, output);
        }
        finally
        {
            ArrayPool<Complex>.Shared.Return(rented);
        }
    }

    private static void TransformND(ReadOnlySpan<Complex> input, ReadOnlySpan<int> shape, Span<Complex> output, bool inverse)
    {
        ValidateShapeAndLengths(shape, input.Length, output.Length);

        if (shape.Length == 1)
        {
            Transform(input, output, inverse);
            return;
        }

        input.CopyTo(output);

        int totalLength = input.Length;
        Complex[] rentedScratch = ArrayPool<Complex>.Shared.Rent(totalLength);

        try
        {
            Span<Complex> scratch = rentedScratch.AsSpan(0, totalLength);
            bool sourceIsOutput = true;

            for (int axis = 0; axis < shape.Length; axis++)
            {
                int axisLength = shape[axis];
                int inner = 1;
                for (int i = axis + 1; i < shape.Length; i++)
                {
                    inner *= shape[i];
                }

                int outer = totalLength / (axisLength * inner);

                ReadOnlySpan<Complex> source = sourceIsOutput ? output : scratch;
                Span<Complex> destination = sourceIsOutput ? scratch : output;

                Complex[]? rentedLine = null;
                Complex[]? rentedLineOut = null;

                try
                {
                    rentedLine = ArrayPool<Complex>.Shared.Rent(axisLength);
                    rentedLineOut = ArrayPool<Complex>.Shared.Rent(axisLength);

                    Span<Complex> line = rentedLine.AsSpan(0, axisLength);
                    Span<Complex> lineOut = rentedLineOut.AsSpan(0, axisLength);

                    for (int o = 0; o < outer; o++)
                    {
                        int outerOffset = o * axisLength * inner;
                        for (int innerIndex = 0; innerIndex < inner; innerIndex++)
                        {
                            for (int a = 0; a < axisLength; a++)
                            {
                                line[a] = source[outerOffset + (a * inner) + innerIndex];
                            }

                            Transform(line, lineOut, inverse);

                            for (int a = 0; a < axisLength; a++)
                            {
                                destination[outerOffset + (a * inner) + innerIndex] = lineOut[a];
                            }
                        }
                    }
                }
                finally
                {
                    if (rentedLine is not null)
                    {
                        ArrayPool<Complex>.Shared.Return(rentedLine);
                    }

                    if (rentedLineOut is not null)
                    {
                        ArrayPool<Complex>.Shared.Return(rentedLineOut);
                    }
                }

                sourceIsOutput = !sourceIsOutput;
            }

            if (!sourceIsOutput)
            {
                scratch.CopyTo(output);
            }
        }
        finally
        {
            ArrayPool<Complex>.Shared.Return(rentedScratch);
        }
    }

    private static void ValidateShapeAndLengths(ReadOnlySpan<int> shape, int inputLength, int outputLength)
    {
        if (shape.Length == 0)
        {
            throw new ArgumentException("Shape must contain at least one dimension.", nameof(shape));
        }

        int count = 1;
        for (int i = 0; i < shape.Length; i++)
        {
            if (shape[i] <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(shape), "All dimensions must be positive.");
            }

            checked
            {
                count *= shape[i];
            }
        }

        if (inputLength != count || outputLength != count)
        {
            throw new ArgumentException("Input/output lengths must match product of shape dimensions.");
        }
    }

    private static int GetElementCount(ReadOnlySpan<int> shape)
    {
        if (shape.Length == 0)
        {
            throw new ArgumentException("Shape must contain at least one dimension.", nameof(shape));
        }

        int count = 1;
        for (int i = 0; i < shape.Length; i++)
        {
            if (shape[i] <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(shape), "All dimensions must be positive.");
            }

            checked
            {
                count *= shape[i];
            }
        }

        return count;
    }

    private static void Transform(ReadOnlySpan<Complex> input, Span<Complex> output, bool inverse)
    {
        if (output.Length != input.Length)
        {
            throw new ArgumentException("Input and output lengths must match.");
        }

        int n = input.Length;
        if (n == 0)
        {
            return;
        }

        if (IsPowerOfTwo(n))
        {
            TransformRadix2(input, output, inverse);
            return;
        }

        double sign = inverse ? 1d : -1d;
        for (int k = 0; k < n; k++)
        {
            Complex sum = Complex.Zero;
            for (int t = 0; t < n; t++)
            {
                double angle = sign * 2d * Math.PI * t * k / n;
                Complex twiddle = new(Math.Cos(angle), Math.Sin(angle));
                sum += input[t] * twiddle;
            }

            output[k] = inverse ? sum / n : sum;
        }
    }

    private static bool IsPowerOfTwo(int n)
    {
        return n > 0 && (n & (n - 1)) == 0;
    }

    private static void TransformRadix2(ReadOnlySpan<Complex> input, Span<Complex> output, bool inverse)
    {
        int n = input.Length;
        input.CopyTo(output);

        int bits = 0;
        for (int t = n; t > 1; t >>= 1)
        {
            bits++;
        }

        for (int i = 0; i < n; i++)
        {
            int j = ReverseBits(i, bits);
            if (j > i)
            {
                (output[i], output[j]) = (output[j], output[i]);
            }
        }

        for (int len = 2; len <= n; len <<= 1)
        {
            int half = len >> 1;
            double angle = (inverse ? 2d : -2d) * Math.PI / len;
            Complex wLen = new(Math.Cos(angle), Math.Sin(angle));

            for (int start = 0; start < n; start += len)
            {
                Complex w = Complex.One;
                for (int j = 0; j < half; j++)
                {
                    int even = start + j;
                    int odd = even + half;

                    Complex u = output[even];
                    Complex v = output[odd] * w;

                    output[even] = u + v;
                    output[odd] = u - v;

                    w *= wLen;
                }
            }

            if (len == n)
            {
                break;
            }
        }

        if (inverse)
        {
            for (int i = 0; i < n; i++)
            {
                output[i] /= n;
            }
        }
    }

    private static int ReverseBits(int value, int bitCount)
    {
        int reversed = 0;
        for (int i = 0; i < bitCount; i++)
        {
            reversed = (reversed << 1) | (value & 1);
            value >>= 1;
        }

        return reversed;
    }
}

