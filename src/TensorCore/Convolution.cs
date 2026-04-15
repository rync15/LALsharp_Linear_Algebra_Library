using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LAL.Core;
using LAL.LinalgCore;

namespace LAL.TensorCore;

public readonly record struct ConvolutionParallelSettings
{
    public ConvolutionParallelSettings()
    {
    }

    public int Parallel2DOutputThreshold { get; init; } = 4096;

    public long Parallel2DOpThreshold { get; init; } = 1_000_000;

    public int ParallelNdOutputThreshold { get; init; } = 4096;

    public long ParallelNdOpThreshold { get; init; } = 500_000;

    public bool EnableAdaptiveThresholding { get; init; } = true;

    public int Minimum2DOutputThreshold { get; init; } = 1024;

    public long Minimum2DOperationThreshold { get; init; } = 250_000;

    public int MinimumNdOutputThreshold { get; init; } = 1024;

    public long MinimumNdOperationThreshold { get; init; } = 250_000;

    public int Large2DOutputHint { get; init; } = 65_536;

    public int LargeNdOutputHint { get; init; } = 32_768;

    public int LargeKernelHint { get; init; } = 256;

    public int MaxDegreeOfParallelism { get; init; } = 0;

    public static ConvolutionParallelSettings Default => new();
}

internal static class Convolution
{
    private const int ParallelOutputThreshold = 4096;
    private const long ParallelOpThreshold = 1_000_000;
    private static readonly object ParallelSettingsLock = new();
    private static ConvolutionParallelSettings s_parallelSettings = ConvolutionParallelSettings.Default;

    public static ConvolutionParallelSettings GetParallelSettings()
    {
        lock (ParallelSettingsLock)
        {
            return s_parallelSettings;
        }
    }

    public static void SetParallelSettings(ConvolutionParallelSettings settings)
    {
        ValidateParallelSettings(settings);

        lock (ParallelSettingsLock)
        {
            s_parallelSettings = settings;
        }
    }

    public static void ResetParallelSettings()
    {
        lock (ParallelSettingsLock)
        {
            s_parallelSettings = ConvolutionParallelSettings.Default;
        }
    }

    public static void Convolve1DFull(ReadOnlySpan<double> signal, ReadOnlySpan<double> kernel, Span<double> destination)
    {
        Convolve1D(signal, kernel, destination);
    }

    public static void Convolve1D(ReadOnlySpan<double> signal, ReadOnlySpan<double> kernel, Span<double> destination)
    {
        Validate1D(signal.Length, kernel.Length, destination.Length);

        DataStructurePerformanceSettings performanceSettings = DataStructureCompatibility.GetPerformanceSettings();

        bool useParallel = ShouldUseParallel1D(signal.Length, kernel.Length, destination.Length, performanceSettings);

        if (useParallel)
        {
            Convolve1DParallel(signal, kernel, destination, performanceSettings);
            return;
        }

        destination.Clear();

        bool vectorized = performanceSettings.EnableSimd
            && Vector.IsHardwareAccelerated
            && kernel.Length >= Math.Max(performanceSettings.SimdLengthThreshold, Vector<double>.Count * 2);
        for (int i = 0; i < signal.Length; i++)
        {
            Span<double> target = destination.Slice(i, kernel.Length);
            if (vectorized)
            {
                Axpy.Compute(signal[i], kernel, target);
                continue;
            }

            double s = signal[i];
            for (int j = 0; j < kernel.Length; j++)
            {
                target[j] += s * kernel[j];
            }
        }
    }

    public static void Convolve1DFull(ReadOnlySpan<float> signal, ReadOnlySpan<float> kernel, Span<float> destination)
    {
        Convolve1D(signal, kernel, destination);
    }

    public static void Convolve1D(ReadOnlySpan<float> signal, ReadOnlySpan<float> kernel, Span<float> destination)
    {
        Validate1D(signal.Length, kernel.Length, destination.Length);

        DataStructurePerformanceSettings performanceSettings = DataStructureCompatibility.GetPerformanceSettings();

        bool useParallel = ShouldUseParallel1D(signal.Length, kernel.Length, destination.Length, performanceSettings);

        if (useParallel)
        {
            Convolve1DParallel(signal, kernel, destination, performanceSettings);
            return;
        }

        destination.Clear();

        bool vectorized = performanceSettings.EnableSimd
            && Vector.IsHardwareAccelerated
            && kernel.Length >= Math.Max(performanceSettings.SimdLengthThreshold, Vector<float>.Count * 2);
        for (int i = 0; i < signal.Length; i++)
        {
            Span<float> target = destination.Slice(i, kernel.Length);
            if (vectorized)
            {
                Axpy.Compute(signal[i], kernel, target);
                continue;
            }

            float s = signal[i];
            for (int j = 0; j < kernel.Length; j++)
            {
                target[j] += s * kernel[j];
            }
        }
    }

    public static void Convolve1DFull(ReadOnlySpan<Complex> signal, ReadOnlySpan<Complex> kernel, Span<Complex> destination)
    {
        Convolve1D(signal, kernel, destination);
    }

    public static void Convolve1D(ReadOnlySpan<Complex> signal, ReadOnlySpan<Complex> kernel, Span<Complex> destination)
    {
        Validate1D(signal.Length, kernel.Length, destination.Length);

        DataStructurePerformanceSettings performanceSettings = DataStructureCompatibility.GetPerformanceSettings();

        bool useParallel = ShouldUseParallel1D(signal.Length, kernel.Length, destination.Length, performanceSettings);

        if (useParallel)
        {
            Convolve1DParallel(signal, kernel, destination, performanceSettings);
            return;
        }

        Convolve1DCore(signal, kernel, destination);
    }

    private static void Convolve1DCore<T>(ReadOnlySpan<T> signal, ReadOnlySpan<T> kernel, Span<T> destination)
        where T : struct, IAdditionOperators<T, T, T>, IMultiplyOperators<T, T, T>
    {
        Validate1D(signal.Length, kernel.Length, destination.Length);

        destination.Clear();

        for (int i = 0; i < signal.Length; i++)
        {
            for (int j = 0; j < kernel.Length; j++)
            {
                destination[i + j] = destination[i + j] + (signal[i] * kernel[j]);
            }
        }
    }

    private static void Validate1D(int signalLength, int kernelLength, int destinationLength)
    {
        if (signalLength == 0 || kernelLength == 0)
        {
            throw new ArgumentException("Signal and kernel must not be empty.");
        }

        int expectedLength = signalLength + kernelLength - 1;
        if (destinationLength != expectedLength)
        {
            throw new ArgumentException("Destination length must be signal.Length + kernel.Length - 1.");
        }
    }

    private static bool ShouldUseParallel1D(int signalLength, int kernelLength, int outputLength, in DataStructurePerformanceSettings settings)
    {
        if (!settings.EnableParallel)
        {
            return false;
        }

        long opCount = (long)signalLength * kernelLength;
        long strategyThreshold = Math.Max(1, settings.ParallelLengthThreshold);
        return outputLength >= ParallelOutputThreshold && opCount >= Math.Max(ParallelOpThreshold, strategyThreshold);
    }

    private static void Convolve1DParallel(ReadOnlySpan<double> signal, ReadOnlySpan<double> kernel, Span<double> destination, in DataStructurePerformanceSettings performanceSettings)
    {
        Validate1D(signal.Length, kernel.Length, destination.Length);

        double[]? signalBuffer = null;
        double[]? kernelBuffer = null;
        double[]? destinationBuffer = null;

        try
        {
            signalBuffer = RentAndCopy(signal);
            kernelBuffer = RentAndCopy(kernel);
            destinationBuffer = ArrayPool<double>.Shared.Rent(destination.Length);
            int signalLength = signal.Length;
            int kernelLength = kernel.Length;

            ParallelOptions options = CreateGlobalParallelOptions(destination.Length, performanceSettings);

            Parallel.For(0, destination.Length, options, outIndex =>
            {
                int iStart = Math.Max(0, outIndex - (kernelLength - 1));
                int iEnd = Math.Min(signalLength - 1, outIndex);

                double sum = 0d;
                for (int i = iStart; i <= iEnd; i++)
                {
                    sum += signalBuffer[i] * kernelBuffer[outIndex - i];
                }

                destinationBuffer[outIndex] = sum;
            });

            destinationBuffer.AsSpan(0, destination.Length).CopyTo(destination);
        }
        finally
        {
            ReturnPooled(destinationBuffer);
            ReturnPooled(kernelBuffer);
            ReturnPooled(signalBuffer);
        }
    }

    private static void Convolve1DParallel(ReadOnlySpan<float> signal, ReadOnlySpan<float> kernel, Span<float> destination, in DataStructurePerformanceSettings performanceSettings)
    {
        Validate1D(signal.Length, kernel.Length, destination.Length);

        float[]? signalBuffer = null;
        float[]? kernelBuffer = null;
        float[]? destinationBuffer = null;

        try
        {
            signalBuffer = RentAndCopy(signal);
            kernelBuffer = RentAndCopy(kernel);
            destinationBuffer = ArrayPool<float>.Shared.Rent(destination.Length);
            int signalLength = signal.Length;
            int kernelLength = kernel.Length;

            ParallelOptions options = CreateGlobalParallelOptions(destination.Length, performanceSettings);

            Parallel.For(0, destination.Length, options, outIndex =>
            {
                int iStart = Math.Max(0, outIndex - (kernelLength - 1));
                int iEnd = Math.Min(signalLength - 1, outIndex);

                float sum = 0f;
                for (int i = iStart; i <= iEnd; i++)
                {
                    sum += signalBuffer[i] * kernelBuffer[outIndex - i];
                }

                destinationBuffer[outIndex] = sum;
            });

            destinationBuffer.AsSpan(0, destination.Length).CopyTo(destination);
        }
        finally
        {
            ReturnPooled(destinationBuffer);
            ReturnPooled(kernelBuffer);
            ReturnPooled(signalBuffer);
        }
    }

    private static void Convolve1DParallel(ReadOnlySpan<Complex> signal, ReadOnlySpan<Complex> kernel, Span<Complex> destination, in DataStructurePerformanceSettings performanceSettings)
    {
        Validate1D(signal.Length, kernel.Length, destination.Length);

        Complex[]? signalBuffer = null;
        Complex[]? kernelBuffer = null;
        Complex[]? destinationBuffer = null;

        try
        {
            signalBuffer = RentAndCopy(signal);
            kernelBuffer = RentAndCopy(kernel);
            destinationBuffer = ArrayPool<Complex>.Shared.Rent(destination.Length);
            int signalLength = signal.Length;
            int kernelLength = kernel.Length;

            ParallelOptions options = CreateGlobalParallelOptions(destination.Length, performanceSettings);

            Parallel.For(0, destination.Length, options, outIndex =>
            {
                int iStart = Math.Max(0, outIndex - (kernelLength - 1));
                int iEnd = Math.Min(signalLength - 1, outIndex);

                Complex sum = Complex.Zero;
                for (int i = iStart; i <= iEnd; i++)
                {
                    sum += signalBuffer[i] * kernelBuffer[outIndex - i];
                }

                destinationBuffer[outIndex] = sum;
            });

            destinationBuffer.AsSpan(0, destination.Length).CopyTo(destination);
        }
        finally
        {
            ReturnPooled(destinationBuffer);
            ReturnPooled(kernelBuffer);
            ReturnPooled(signalBuffer);
        }
    }

    public static void Convolve2DFull(
        ReadOnlySpan<double> signal,
        int signalRows,
        int signalCols,
        ReadOnlySpan<double> kernel,
        int kernelRows,
        int kernelCols,
        Span<double> destination)
    {
        Convolve2D(signal, signalRows, signalCols, kernel, kernelRows, kernelCols, destination);
    }

    public static void Convolve2D(
        ReadOnlySpan<double> signal,
        int signalRows,
        int signalCols,
        ReadOnlySpan<double> kernel,
        int kernelRows,
        int kernelCols,
        Span<double> destination)
    {
        Validate2D(signal.Length, signalRows, signalCols, kernel.Length, kernelRows, kernelCols, destination.Length);
        ConvolutionParallelSettings settings = GetParallelSettings();
        DataStructurePerformanceSettings performanceSettings = DataStructureCompatibility.GetPerformanceSettings();

        if (ShouldUseParallel2D(signal.Length, kernel.Length, destination.Length, settings, performanceSettings))
        {
            Convolve2DParallel(signal, signalRows, signalCols, kernel, kernelRows, kernelCols, settings, performanceSettings, destination);
            return;
        }

        Convolve2DCore(signal, signalRows, signalCols, kernel, kernelRows, kernelCols, destination);
    }

    public static void Convolve2DFull(
        ReadOnlySpan<float> signal,
        int signalRows,
        int signalCols,
        ReadOnlySpan<float> kernel,
        int kernelRows,
        int kernelCols,
        Span<float> destination)
    {
        Convolve2D(signal, signalRows, signalCols, kernel, kernelRows, kernelCols, destination);
    }

    public static void Convolve2D(
        ReadOnlySpan<float> signal,
        int signalRows,
        int signalCols,
        ReadOnlySpan<float> kernel,
        int kernelRows,
        int kernelCols,
        Span<float> destination)
    {
        Validate2D(signal.Length, signalRows, signalCols, kernel.Length, kernelRows, kernelCols, destination.Length);
        ConvolutionParallelSettings settings = GetParallelSettings();
        DataStructurePerformanceSettings performanceSettings = DataStructureCompatibility.GetPerformanceSettings();

        if (ShouldUseParallel2D(signal.Length, kernel.Length, destination.Length, settings, performanceSettings))
        {
            Convolve2DParallel(signal, signalRows, signalCols, kernel, kernelRows, kernelCols, settings, performanceSettings, destination);
            return;
        }

        Convolve2DCore(signal, signalRows, signalCols, kernel, kernelRows, kernelCols, destination);
    }

    public static void Convolve2DFull(
        ReadOnlySpan<Complex> signal,
        int signalRows,
        int signalCols,
        ReadOnlySpan<Complex> kernel,
        int kernelRows,
        int kernelCols,
        Span<Complex> destination)
    {
        Convolve2D(signal, signalRows, signalCols, kernel, kernelRows, kernelCols, destination);
    }

    public static void Convolve2D(
        ReadOnlySpan<Complex> signal,
        int signalRows,
        int signalCols,
        ReadOnlySpan<Complex> kernel,
        int kernelRows,
        int kernelCols,
        Span<Complex> destination)
    {
        Validate2D(signal.Length, signalRows, signalCols, kernel.Length, kernelRows, kernelCols, destination.Length);
        ConvolutionParallelSettings settings = GetParallelSettings();
        DataStructurePerformanceSettings performanceSettings = DataStructureCompatibility.GetPerformanceSettings();

        if (ShouldUseParallel2D(signal.Length, kernel.Length, destination.Length, settings, performanceSettings))
        {
            Convolve2DParallel(signal, signalRows, signalCols, kernel, kernelRows, kernelCols, settings, performanceSettings, destination);
            return;
        }

        Convolve2DCore(signal, signalRows, signalCols, kernel, kernelRows, kernelCols, destination);
    }

    private static void Convolve2DCore<T>(
        ReadOnlySpan<T> signal,
        int signalRows,
        int signalCols,
        ReadOnlySpan<T> kernel,
        int kernelRows,
        int kernelCols,
        Span<T> destination)
        where T : struct, IAdditionOperators<T, T, T>, IMultiplyOperators<T, T, T>
    {
        Validate2D(signal.Length, signalRows, signalCols, kernel.Length, kernelRows, kernelCols, destination.Length);

        int outRows = signalRows + kernelRows - 1;
        int outCols = signalCols + kernelCols - 1;

        destination.Clear();

        for (int sr = 0; sr < signalRows; sr++)
        {
            int signalRowOffset = sr * signalCols;
            for (int sc = 0; sc < signalCols; sc++)
            {
                T signalValue = signal[signalRowOffset + sc];
                for (int kr = 0; kr < kernelRows; kr++)
                {
                    int dstRow = sr + kr;
                    int dstOffset = dstRow * outCols + sc;
                    int kernelOffset = kr * kernelCols;

                    for (int kc = 0; kc < kernelCols; kc++)
                    {
                        destination[dstOffset + kc] = destination[dstOffset + kc] + (signalValue * kernel[kernelOffset + kc]);
                    }
                }
            }
        }
    }

    private static void Validate2D(
        int signalLength,
        int signalRows,
        int signalCols,
        int kernelLength,
        int kernelRows,
        int kernelCols,
        int destinationLength)
    {
        if (signalRows <= 0 || signalCols <= 0 || kernelRows <= 0 || kernelCols <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(signalRows), "All matrix dimensions must be positive.");
        }

        if (signalLength != signalRows * signalCols)
        {
            throw new ArgumentException("Signal length does not match signalRows*signalCols.");
        }

        if (kernelLength != kernelRows * kernelCols)
        {
            throw new ArgumentException("Kernel length does not match kernelRows*kernelCols.");
        }

        int outRows = signalRows + kernelRows - 1;
        int outCols = signalCols + kernelCols - 1;
        if (destinationLength != outRows * outCols)
        {
            throw new ArgumentException("Destination length does not match 2D full-convolution output size.");
        }
    }

    private static void Convolve2DParallel<T>(
        ReadOnlySpan<T> signal,
        int signalRows,
        int signalCols,
        ReadOnlySpan<T> kernel,
        int kernelRows,
        int kernelCols,
        ConvolutionParallelSettings settings,
        DataStructurePerformanceSettings performanceSettings,
        Span<T> destination)
        where T : struct, IAdditionOperators<T, T, T>, IMultiplyOperators<T, T, T>
    {
        Validate2D(signal.Length, signalRows, signalCols, kernel.Length, kernelRows, kernelCols, destination.Length);

        T[]? signalBuffer = null;
        T[]? kernelBuffer = null;
        T[]? destinationBuffer = null;

        try
        {
            signalBuffer = RentAndCopy(signal);
            kernelBuffer = RentAndCopy(kernel);
            destinationBuffer = ArrayPool<T>.Shared.Rent(destination.Length);

            int outCols = signalCols + kernelCols - 1;
            ParallelOptions options = CreateParallelOptions(destination.Length, settings, performanceSettings);

            Parallel.For(0, destination.Length, options, outFlat =>
            {
                int outRow = outFlat / outCols;
                int outCol = outFlat - (outRow * outCols);

                int srStart = Math.Max(0, outRow - (kernelRows - 1));
                int srEnd = Math.Min(signalRows - 1, outRow);
                int scStart = Math.Max(0, outCol - (kernelCols - 1));
                int scEnd = Math.Min(signalCols - 1, outCol);

                T sum = default;
                for (int sr = srStart; sr <= srEnd; sr++)
                {
                    int signalRowOffset = sr * signalCols;
                    int kernelRow = outRow - sr;
                    int kernelRowOffset = kernelRow * kernelCols;

                    for (int sc = scStart; sc <= scEnd; sc++)
                    {
                        int kernelCol = outCol - sc;
                        sum = sum + (signalBuffer[signalRowOffset + sc] * kernelBuffer[kernelRowOffset + kernelCol]);
                    }
                }

                destinationBuffer[outFlat] = sum;
            });

            destinationBuffer.AsSpan(0, destination.Length).CopyTo(destination);
        }
        finally
        {
            ReturnPooled(destinationBuffer);
            ReturnPooled(kernelBuffer);
            ReturnPooled(signalBuffer);
        }
    }

    public static int[] GetFullOutputShape(ReadOnlySpan<int> signalShape, ReadOnlySpan<int> kernelShape)
    {
        if (signalShape.Length == 0 || kernelShape.Length == 0)
        {
            throw new ArgumentException("Signal and kernel shapes must not be empty.");
        }

        if (signalShape.Length != kernelShape.Length)
        {
            throw new ArgumentException("Signal and kernel shapes must have the same rank.");
        }

        int[] outputShape = new int[signalShape.Length];
        for (int i = 0; i < signalShape.Length; i++)
        {
            if (signalShape[i] <= 0 || kernelShape[i] <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(signalShape), "All dimensions must be positive.");
            }

            outputShape[i] = signalShape[i] + kernelShape[i] - 1;
        }

        return outputShape;
    }

    public static void ConvolveNDFull(
        ReadOnlySpan<double> signal,
        ReadOnlySpan<int> signalShape,
        ReadOnlySpan<double> kernel,
        ReadOnlySpan<int> kernelShape,
        Span<double> destination)
    {
        int[] outputShape = GetFullOutputShape(signalShape, kernelShape);
        int outputCount = TensorShape.GetElementCount(outputShape);
        ConvolutionParallelSettings settings = GetParallelSettings();
        DataStructurePerformanceSettings performanceSettings = DataStructureCompatibility.GetPerformanceSettings();

        if (ShouldUseParallelND(outputCount, kernel.Length, signalShape.Length, settings, performanceSettings))
        {
            ConvolveNDFullParallelCore(signal, signalShape, kernel, kernelShape, settings, performanceSettings, destination);
            return;
        }

        ConvolveNDFullCore(signal, signalShape, kernel, kernelShape, destination);
    }

    public static void ConvolveNDFull(
        ReadOnlySpan<float> signal,
        ReadOnlySpan<int> signalShape,
        ReadOnlySpan<float> kernel,
        ReadOnlySpan<int> kernelShape,
        Span<float> destination)
    {
        int[] outputShape = GetFullOutputShape(signalShape, kernelShape);
        int outputCount = TensorShape.GetElementCount(outputShape);
        ConvolutionParallelSettings settings = GetParallelSettings();
        DataStructurePerformanceSettings performanceSettings = DataStructureCompatibility.GetPerformanceSettings();

        if (ShouldUseParallelND(outputCount, kernel.Length, signalShape.Length, settings, performanceSettings))
        {
            ConvolveNDFullParallelCore(signal, signalShape, kernel, kernelShape, settings, performanceSettings, destination);
            return;
        }

        ConvolveNDFullCore(signal, signalShape, kernel, kernelShape, destination);
    }

    public static void ConvolveNDFull(
        ReadOnlySpan<Complex> signal,
        ReadOnlySpan<int> signalShape,
        ReadOnlySpan<Complex> kernel,
        ReadOnlySpan<int> kernelShape,
        Span<Complex> destination)
    {
        int[] outputShape = GetFullOutputShape(signalShape, kernelShape);
        int outputCount = TensorShape.GetElementCount(outputShape);
        ConvolutionParallelSettings settings = GetParallelSettings();
        DataStructurePerformanceSettings performanceSettings = DataStructureCompatibility.GetPerformanceSettings();

        if (ShouldUseParallelND(outputCount, kernel.Length, signalShape.Length, settings, performanceSettings))
        {
            ConvolveNDFullParallelCore(signal, signalShape, kernel, kernelShape, settings, performanceSettings, destination);
            return;
        }

        ConvolveNDFullCore(signal, signalShape, kernel, kernelShape, destination);
    }

    private static bool ShouldUseParallel2D(
        int signalCount,
        int kernelCount,
        int outputCount,
        in ConvolutionParallelSettings settings,
        in DataStructurePerformanceSettings performanceSettings)
    {
        if (!performanceSettings.EnableParallel)
        {
            return false;
        }

        int outputThreshold = settings.Parallel2DOutputThreshold;
        long opThreshold = settings.Parallel2DOpThreshold;

        if (settings.EnableAdaptiveThresholding)
        {
            int parallelBudget = ResolveParallelismBudget(settings);
            bool largeWorkload = outputCount >= settings.Large2DOutputHint || kernelCount >= settings.LargeKernelHint;

            if (largeWorkload)
            {
                int downScale = Math.Max(1, parallelBudget / 4);
                outputThreshold = Math.Max(settings.Minimum2DOutputThreshold, outputThreshold / downScale);
                opThreshold = Math.Max(settings.Minimum2DOperationThreshold, opThreshold / downScale);
            }
            else
            {
                int upScale = Math.Max(1, parallelBudget / 2);
                outputThreshold = SaturatingMultiply(outputThreshold, upScale);
                opThreshold = SaturatingMultiply(opThreshold, upScale);
            }
        }

        long globalThreshold = Math.Max(1, performanceSettings.ParallelLengthThreshold);
        return outputCount >= outputThreshold
            && ((long)signalCount * kernelCount) >= Math.Max(opThreshold, globalThreshold);
    }

    private static bool ShouldUseParallelND(
        int outputCount,
        int kernelCount,
        int rank,
        in ConvolutionParallelSettings settings,
        in DataStructurePerformanceSettings performanceSettings)
    {
        if (!performanceSettings.EnableParallel)
        {
            return false;
        }

        int outputThreshold = settings.ParallelNdOutputThreshold;
        long opThreshold = settings.ParallelNdOpThreshold;

        if (settings.EnableAdaptiveThresholding)
        {
            int parallelBudget = ResolveParallelismBudget(settings);
            bool largeWorkload = outputCount >= settings.LargeNdOutputHint
                || kernelCount >= settings.LargeKernelHint
                || rank >= 4;

            if (largeWorkload)
            {
                int downScale = Math.Max(1, parallelBudget / 4);
                outputThreshold = Math.Max(settings.MinimumNdOutputThreshold, outputThreshold / downScale);
                opThreshold = Math.Max(settings.MinimumNdOperationThreshold, opThreshold / downScale);
            }
            else
            {
                int upScale = Math.Max(1, parallelBudget / 2);
                outputThreshold = SaturatingMultiply(outputThreshold, upScale);
                opThreshold = SaturatingMultiply(opThreshold, upScale);
            }
        }

        long estimatedOps = (long)outputCount * kernelCount * Math.Max(1, rank);
        long globalThreshold = (long)Math.Max(1, performanceSettings.ParallelLengthThreshold) * Math.Max(1, rank);
        return outputCount >= outputThreshold && estimatedOps >= Math.Max(opThreshold, globalThreshold);
    }

    private static void ConvolveNDFullCore<T>(
        ReadOnlySpan<T> signal,
        ReadOnlySpan<int> signalShape,
        ReadOnlySpan<T> kernel,
        ReadOnlySpan<int> kernelShape,
        Span<T> destination)
        where T : struct, IAdditionOperators<T, T, T>, IMultiplyOperators<T, T, T>
    {
        int[] outputShape = GetFullOutputShape(signalShape, kernelShape);

        int signalCount = TensorShape.GetElementCount(signalShape);
        int kernelCount = TensorShape.GetElementCount(kernelShape);
        int outputCount = TensorShape.GetElementCount(outputShape);

        if (signal.Length != signalCount)
        {
            throw new ArgumentException("Signal length does not match signalShape.", nameof(signal));
        }

        if (kernel.Length != kernelCount)
        {
            throw new ArgumentException("Kernel length does not match kernelShape.", nameof(kernel));
        }

        if (destination.Length != outputCount)
        {
            throw new ArgumentException("Destination length does not match full N-D convolution output size.", nameof(destination));
        }

        int rank = signalShape.Length;
        int[] signalStrides = TensorShape.ComputeRowMajorStrides(signalShape);
        int[] kernelStrides = TensorShape.ComputeRowMajorStrides(kernelShape);
        int[] outputStrides = TensorShape.ComputeRowMajorStrides(outputShape);

        int[] rentedSignalIndices = ArrayPool<int>.Shared.Rent(rank);
        int[] rentedKernelIndices = ArrayPool<int>.Shared.Rent(rank);
        int[] rentedOutputIndices = ArrayPool<int>.Shared.Rent(rank);

        try
        {
            Span<int> signalIndices = rentedSignalIndices.AsSpan(0, rank);
            Span<int> kernelIndices = rentedKernelIndices.AsSpan(0, rank);
            Span<int> outputIndices = rentedOutputIndices.AsSpan(0, rank);

            destination.Clear();

            for (int s = 0; s < signalCount; s++)
            {
                UnflattenIndex(s, signalShape, signalStrides, signalIndices);
                T signalValue = signal[s];

                for (int k = 0; k < kernelCount; k++)
                {
                    UnflattenIndex(k, kernelShape, kernelStrides, kernelIndices);

                    for (int axis = 0; axis < rank; axis++)
                    {
                        outputIndices[axis] = signalIndices[axis] + kernelIndices[axis];
                    }

                    int outputFlat = FlattenIndex(outputIndices, outputStrides);
                    destination[outputFlat] = destination[outputFlat] + (signalValue * kernel[k]);
                }
            }
        }
        finally
        {
            ReturnPooled(rentedOutputIndices);
            ReturnPooled(rentedKernelIndices);
            ReturnPooled(rentedSignalIndices);
        }
    }

    private sealed class NdParallelWorkspace
    {
        public NdParallelWorkspace(int rank)
        {
            OutputIndices = new int[rank];
            KernelIndices = new int[rank];
        }

        public int[] OutputIndices { get; }

        public int[] KernelIndices { get; }
    }

    private static void ConvolveNDFullParallelCore<T>(
        ReadOnlySpan<T> signal,
        ReadOnlySpan<int> signalShape,
        ReadOnlySpan<T> kernel,
        ReadOnlySpan<int> kernelShape,
        ConvolutionParallelSettings settings,
        DataStructurePerformanceSettings performanceSettings,
        Span<T> destination)
        where T : struct, IAdditionOperators<T, T, T>, IMultiplyOperators<T, T, T>
    {
        int[] outputShape = GetFullOutputShape(signalShape, kernelShape);

        int signalCount = TensorShape.GetElementCount(signalShape);
        int kernelCount = TensorShape.GetElementCount(kernelShape);
        int outputCount = TensorShape.GetElementCount(outputShape);

        if (signal.Length != signalCount)
        {
            throw new ArgumentException("Signal length does not match signalShape.", nameof(signal));
        }

        if (kernel.Length != kernelCount)
        {
            throw new ArgumentException("Kernel length does not match kernelShape.", nameof(kernel));
        }

        if (destination.Length != outputCount)
        {
            throw new ArgumentException("Destination length does not match full N-D convolution output size.", nameof(destination));
        }

        int rank = signalShape.Length;

        T[]? signalBuffer = null;
        T[]? kernelBuffer = null;
        T[]? destinationBuffer = null;

        try
        {
            signalBuffer = RentAndCopy(signal);
            kernelBuffer = RentAndCopy(kernel);

            int[] signalShapeArray = signalShape.ToArray();
            int[] kernelShapeArray = kernelShape.ToArray();

            int[] signalStrides = TensorShape.ComputeRowMajorStrides(signalShapeArray);
            int[] kernelStrides = TensorShape.ComputeRowMajorStrides(kernelShapeArray);
            int[] outputStrides = TensorShape.ComputeRowMajorStrides(outputShape);

            destinationBuffer = ArrayPool<T>.Shared.Rent(outputCount);
            ParallelOptions options = CreateParallelOptions(outputCount, settings, performanceSettings);

            Parallel.For(
                0,
                outputCount,
                options,
                () => new NdParallelWorkspace(rank),
                (outFlat, _, workspace) =>
                {
                    UnflattenIndex(outFlat, outputShape, outputStrides, workspace.OutputIndices);

                    T sum = default;
                    for (int k = 0; k < kernelCount; k++)
                    {
                        UnflattenIndex(k, kernelShapeArray, kernelStrides, workspace.KernelIndices);

                        int signalFlat = 0;
                        bool inRange = true;
                        for (int axis = 0; axis < rank; axis++)
                        {
                            int signalAxis = workspace.OutputIndices[axis] - workspace.KernelIndices[axis];
                            if ((uint)signalAxis >= (uint)signalShapeArray[axis])
                            {
                                inRange = false;
                                break;
                            }

                            signalFlat += signalAxis * signalStrides[axis];
                        }

                        if (inRange)
                        {
                            sum = sum + (signalBuffer[signalFlat] * kernelBuffer[k]);
                        }
                    }

                    destinationBuffer[outFlat] = sum;
                    return workspace;
                },
                _ => { });

            destinationBuffer.AsSpan(0, outputCount).CopyTo(destination);
        }
        finally
        {
            ReturnPooled(destinationBuffer);
            ReturnPooled(kernelBuffer);
            ReturnPooled(signalBuffer);
        }
    }

    private static void UnflattenIndex(int flatIndex, ReadOnlySpan<int> shape, ReadOnlySpan<int> strides, Span<int> indices)
    {
        for (int i = 0; i < shape.Length; i++)
        {
            indices[i] = (flatIndex / strides[i]) % shape[i];
        }
    }

    private static T[] RentAndCopy<T>(ReadOnlySpan<T> source)
    {
        T[] rented = ArrayPool<T>.Shared.Rent(source.Length);
        source.CopyTo(rented);
        return rented;
    }

    private static void ReturnPooled<T>(T[]? rented)
    {
        if (rented is null)
        {
            return;
        }

        ArrayPool<T>.Shared.Return(rented, RuntimeHelpers.IsReferenceOrContainsReferences<T>());
    }

    private static ParallelOptions CreateParallelOptions(
        int workLength,
        in ConvolutionParallelSettings settings,
        in DataStructurePerformanceSettings performanceSettings)
    {
        int budget = ResolveParallelismBudget(settings);
        int globalBudget = ResolveGlobalParallelismBudget(performanceSettings);
        budget = Math.Max(1, Math.Min(budget, globalBudget));
        int maxDegree = Math.Max(1, Math.Min(budget, workLength));

        return new ParallelOptions
        {
            MaxDegreeOfParallelism = maxDegree
        };
    }

    private static ParallelOptions CreateGlobalParallelOptions(int workLength, in DataStructurePerformanceSettings performanceSettings)
    {
        int budget = ResolveGlobalParallelismBudget(performanceSettings);
        int maxDegree = Math.Max(1, Math.Min(budget, workLength));

        return new ParallelOptions
        {
            MaxDegreeOfParallelism = maxDegree
        };
    }

    private static int ResolveParallelismBudget(in ConvolutionParallelSettings settings)
    {
        if (settings.MaxDegreeOfParallelism > 0)
        {
            return settings.MaxDegreeOfParallelism;
        }

        return Math.Max(1, Environment.ProcessorCount);
    }

    private static int ResolveGlobalParallelismBudget(in DataStructurePerformanceSettings performanceSettings)
    {
        if (!performanceSettings.EnableParallel)
        {
            return 1;
        }

        if (performanceSettings.MaxDegreeOfParallelism.HasValue)
        {
            return Math.Max(1, performanceSettings.MaxDegreeOfParallelism.Value);
        }

        return Math.Max(1, Environment.ProcessorCount);
    }

    private static int SaturatingMultiply(int value, int scale)
    {
        long scaled = (long)value * scale;
        return scaled >= int.MaxValue ? int.MaxValue : (int)scaled;
    }

    private static long SaturatingMultiply(long value, int scale)
    {
        if (value > long.MaxValue / scale)
        {
            return long.MaxValue;
        }

        return value * scale;
    }

    private static void ValidateParallelSettings(in ConvolutionParallelSettings settings)
    {
        if (settings.Parallel2DOutputThreshold <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(settings), "Parallel2DOutputThreshold must be positive.");
        }

        if (settings.Parallel2DOpThreshold <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(settings), "Parallel2DOpThreshold must be positive.");
        }

        if (settings.ParallelNdOutputThreshold <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(settings), "ParallelNdOutputThreshold must be positive.");
        }

        if (settings.ParallelNdOpThreshold <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(settings), "ParallelNdOpThreshold must be positive.");
        }

        if (settings.Minimum2DOutputThreshold <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(settings), "Minimum2DOutputThreshold must be positive.");
        }

        if (settings.Minimum2DOperationThreshold <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(settings), "Minimum2DOperationThreshold must be positive.");
        }

        if (settings.MinimumNdOutputThreshold <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(settings), "MinimumNdOutputThreshold must be positive.");
        }

        if (settings.MinimumNdOperationThreshold <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(settings), "MinimumNdOperationThreshold must be positive.");
        }

        if (settings.Large2DOutputHint <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(settings), "Large2DOutputHint must be positive.");
        }

        if (settings.LargeNdOutputHint <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(settings), "LargeNdOutputHint must be positive.");
        }

        if (settings.LargeKernelHint <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(settings), "LargeKernelHint must be positive.");
        }

        if (settings.MaxDegreeOfParallelism < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(settings), "MaxDegreeOfParallelism must be >= 0 (0 means auto).");
        }
    }

    private static int FlattenIndex(ReadOnlySpan<int> indices, ReadOnlySpan<int> strides)
    {
        int flat = 0;
        for (int i = 0; i < indices.Length; i++)
        {
            flat += indices[i] * strides[i];
        }

        return flat;
    }
}

