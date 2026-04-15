using LAL.Core;
using LAL.TensorCore;

namespace LAL.Tests.TensorCore;

public class ConvolutionTests
{
    [Fact]
    public void ParallelSettings_CanSetGetAndReset()
    {
        ConvolutionParallelSettings original = Convolution.GetParallelSettings();

        try
        {
            ConvolutionParallelSettings custom = ConvolutionParallelSettings.Default with
            {
                Parallel2DOutputThreshold = 1024,
                Parallel2DOpThreshold = 250_000,
                ParallelNdOutputThreshold = 1536,
                ParallelNdOpThreshold = 200_000,
                EnableAdaptiveThresholding = false,
                MaxDegreeOfParallelism = 2
            };

            Convolution.SetParallelSettings(custom);
            Assert.Equal(custom, Convolution.GetParallelSettings());

            Convolution.ResetParallelSettings();
            Assert.Equal(ConvolutionParallelSettings.Default, Convolution.GetParallelSettings());
        }
        finally
        {
            Convolution.SetParallelSettings(original);
        }
    }

    [Fact]
    public void ParallelSettings_NegativeMaxDegree_Throws()
    {
        ConvolutionParallelSettings invalid = ConvolutionParallelSettings.Default with
        {
            MaxDegreeOfParallelism = -1
        };

        Assert.Throws<ArgumentOutOfRangeException>(() => Convolution.SetParallelSettings(invalid));
    }

    [Fact]
    public void Convolve1DFull_Works()
    {
        double[] x = [1, 2];
        double[] h = [1, 1];
        double[] y = new double[x.Length + h.Length - 1];

        Convolution.Convolve1D(x, h, y);

        Assert.Equal(new double[] { 1, 3, 2 }, y);

        double[] yAlias = new double[x.Length + h.Length - 1];
        Convolution.Convolve1DFull(x, h, yAlias);
        Assert.Equal(new double[] { 1, 3, 2 }, yAlias);
    }

    [Fact]
    public void Convolve2D_Works()
    {
        double[] signal =
        [
            1, 2,
            3, 4
        ];

        double[] kernel =
        [
            1, 1,
            1, 1
        ];

        double[] destination = new double[9];

        Convolution.Convolve2D(signal, signalRows: 2, signalCols: 2, kernel, kernelRows: 2, kernelCols: 2, destination);

        Assert.Equal(
            new double[]
            {
                1, 3, 2,
                4, 10, 6,
                3, 7, 4
            },
            destination);
    }

    [Fact]
    public void ConvolveNDFull_WorksFor2DShape()
    {
        double[] signal =
        [
            1, 2,
            3, 4
        ];

        double[] kernel =
        [
            1, 1,
            1, 1
        ];

        int[] signalShape = [2, 2];
        int[] kernelShape = [2, 2];
        int[] outputShape = Convolution.GetFullOutputShape(signalShape, kernelShape);
        double[] destination = new double[TensorShape.GetElementCount(outputShape)];

        Convolution.ConvolveNDFull(signal, signalShape, kernel, kernelShape, destination);

        Assert.Equal(
            new double[]
            {
                1, 3, 2,
                4, 10, 6,
                3, 7, 4
            },
            destination);
    }

    [Fact]
    public void Convolve2D_ParallelPath_MatchesReference()
    {
        const int signalRows = 64;
        const int signalCols = 64;
        const int kernelRows = 8;
        const int kernelCols = 8;

        double[] signal = new double[signalRows * signalCols];
        for (int r = 0; r < signalRows; r++)
        {
            int rowOffset = r * signalCols;
            for (int c = 0; c < signalCols; c++)
            {
                signal[rowOffset + c] = ((r * 3 + c * 5) % 17) - 8;
            }
        }

        double[] kernel = new double[kernelRows * kernelCols];
        for (int r = 0; r < kernelRows; r++)
        {
            int rowOffset = r * kernelCols;
            for (int c = 0; c < kernelCols; c++)
            {
                kernel[rowOffset + c] = ((((r + 1) * (c + 2)) % 7) - 3) * 0.125;
            }
        }

        double[] expected = ReferenceConvolve2D(signal, signalRows, signalCols, kernel, kernelRows, kernelCols);
        double[] actual = new double[(signalRows + kernelRows - 1) * (signalCols + kernelCols - 1)];

        Convolution.Convolve2D(signal, signalRows, signalCols, kernel, kernelRows, kernelCols, actual);

        AssertClose(expected, actual, 1e-9);
    }

    [Fact]
    public void ConvolveNDFull_ParallelPath_MatchesReferenceFor2DShape()
    {
        const int signalRows = 64;
        const int signalCols = 64;
        const int kernelRows = 8;
        const int kernelCols = 8;

        double[] signal = new double[signalRows * signalCols];
        for (int r = 0; r < signalRows; r++)
        {
            int rowOffset = r * signalCols;
            for (int c = 0; c < signalCols; c++)
            {
                signal[rowOffset + c] = ((r * 7 + c * 2) % 19) - 9;
            }
        }

        double[] kernel = new double[kernelRows * kernelCols];
        for (int r = 0; r < kernelRows; r++)
        {
            int rowOffset = r * kernelCols;
            for (int c = 0; c < kernelCols; c++)
            {
                kernel[rowOffset + c] = ((((r + 3) * (c + 1)) % 11) - 5) * 0.1;
            }
        }

        int[] signalShape = [signalRows, signalCols];
        int[] kernelShape = [kernelRows, kernelCols];
        double[] expected = ReferenceConvolve2D(signal, signalRows, signalCols, kernel, kernelRows, kernelCols);
        double[] actual = new double[(signalRows + kernelRows - 1) * (signalCols + kernelCols - 1)];

        Convolution.ConvolveNDFull(signal, signalShape, kernel, kernelShape, actual);

        AssertClose(expected, actual, 1e-9);
    }

    [Fact]
    public void Convolve1D_StrategyToggle_ProducesConsistentResult()
    {
        DataStructurePerformanceSettings originalPerformance = DataStructureCompatibility.GetPerformanceSettings();
        ConvolutionParallelSettings originalConvolution = Convolution.GetParallelSettings();

        try
        {
            Convolution.SetParallelSettings(ConvolutionParallelSettings.Default with
            {
                Parallel2DOutputThreshold = 1,
                Parallel2DOpThreshold = 1,
                ParallelNdOutputThreshold = 1,
                ParallelNdOpThreshold = 1,
                EnableAdaptiveThresholding = false,
                MaxDegreeOfParallelism = 4
            });

            double[] signal = new double[128];
            double[] kernel = new double[64];
            for (int i = 0; i < signal.Length; i++)
            {
                signal[i] = ((i * 3) % 17) - 8;
            }

            for (int i = 0; i < kernel.Length; i++)
            {
                kernel[i] = (((i * 5) % 11) - 5) * 0.25;
            }

            double[] scalarResult = new double[signal.Length + kernel.Length - 1];
            double[] acceleratedResult = new double[signal.Length + kernel.Length - 1];

            DataStructureCompatibility.SetPerformanceSettings(new DataStructurePerformanceSettings(
                EnableUnsafe: false,
                EnableSimd: false,
                EnableIntrinsics: false,
                EnableParallel: false,
                SimdLengthThreshold: 1_024,
                IntrinsicsLengthThreshold: 2_048,
                ParallelLengthThreshold: int.MaxValue,
                MaxDegreeOfParallelism: 1));

            Convolution.Convolve1D(signal, kernel, scalarResult);

            DataStructureCompatibility.SetPerformanceSettings(new DataStructurePerformanceSettings(
                EnableUnsafe: true,
                EnableSimd: true,
                EnableIntrinsics: true,
                EnableParallel: true,
                SimdLengthThreshold: 1,
                IntrinsicsLengthThreshold: 1,
                ParallelLengthThreshold: 1,
                MaxDegreeOfParallelism: 4));

            Convolution.Convolve1D(signal, kernel, acceleratedResult);

            AssertClose(scalarResult, acceleratedResult, 1e-9);
        }
        finally
        {
            DataStructureCompatibility.SetPerformanceSettings(originalPerformance);
            Convolution.SetParallelSettings(originalConvolution);
        }
    }

    private static double[] ReferenceConvolve2D(
        ReadOnlySpan<double> signal,
        int signalRows,
        int signalCols,
        ReadOnlySpan<double> kernel,
        int kernelRows,
        int kernelCols)
    {
        int outRows = signalRows + kernelRows - 1;
        int outCols = signalCols + kernelCols - 1;
        double[] destination = new double[outRows * outCols];

        for (int sr = 0; sr < signalRows; sr++)
        {
            int signalRowOffset = sr * signalCols;
            for (int sc = 0; sc < signalCols; sc++)
            {
                double signalValue = signal[signalRowOffset + sc];
                for (int kr = 0; kr < kernelRows; kr++)
                {
                    int dstRow = sr + kr;
                    int dstOffset = dstRow * outCols + sc;
                    int kernelOffset = kr * kernelCols;

                    for (int kc = 0; kc < kernelCols; kc++)
                    {
                        destination[dstOffset + kc] += signalValue * kernel[kernelOffset + kc];
                    }
                }
            }
        }

        return destination;
    }

    private static void AssertClose(ReadOnlySpan<double> expected, ReadOnlySpan<double> actual, double tolerance)
    {
        Assert.Equal(expected.Length, actual.Length);
        for (int i = 0; i < expected.Length; i++)
        {
            Assert.InRange(Math.Abs(expected[i] - actual[i]), 0d, tolerance);
        }
    }
}
