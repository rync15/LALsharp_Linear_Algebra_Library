using LAL.Core;
using System.Numerics;
using LAL.LinalgCore;

namespace LAL.Tests.LinalgCore;

public class GemvTests
{
    [Fact]
    public void Multiply_ComputesMatrixVectorProduct()
    {
        double[] a =
        [
            1, 2, 3,
            4, 5, 6
        ];
        double[] x = [1, 1, 1];
        double[] y = new double[2];

        Gemv.Multiply(a, rows: 2, cols: 3, x, y);

        Assert.Equal([6.0, 15.0], y);
    }

    [Fact]
    public void Multiply_Float_ComputesMatrixVectorProduct()
    {
        float[] a =
        [
            1f, 2f,
            3f, 4f
        ];
        float[] x = [2f, 1f];
        float[] y = new float[2];

        Gemv.Multiply(a, rows: 2, cols: 2, x, y);

        Assert.Equal([4f, 10f], y);
    }

    [Fact]
    public void Multiply_Complex_ComputesMatrixVectorProduct()
    {
        Complex[] a =
        [
            new Complex(1, 0), new Complex(0, 1),
            new Complex(2, -1), new Complex(1, 0)
        ];
        Complex[] x = [new Complex(1, 1), new Complex(2, 0)];
        Complex[] y = new Complex[2];

        Gemv.Multiply(a, rows: 2, cols: 2, x, y);

        Assert.Equal(new Complex(1, 3), y[0]);
        Assert.Equal(new Complex(5, 1), y[1]);
    }

    [Fact]
    public void Multiply_StrategyToggle_ProducesConsistentResult()
    {
        DataStructurePerformanceSettings original = DataStructureCompatibility.GetPerformanceSettings();

        try
        {
            const int rows = 64;
            const int cols = 64;

            double[] a = new double[rows * cols];
            double[] x = new double[cols];
            for (int i = 0; i < a.Length; i++)
            {
                a[i] = ((i * 7) % 19) - 9;
            }

            for (int i = 0; i < x.Length; i++)
            {
                x[i] = ((i * 11) % 23) - 11;
            }

            double[] scalarResult = new double[rows];
            double[] acceleratedResult = new double[rows];

            DataStructureCompatibility.SetPerformanceSettings(new DataStructurePerformanceSettings(
                EnableUnsafe: false,
                EnableSimd: false,
                EnableIntrinsics: false,
                EnableParallel: false,
                SimdLengthThreshold: 1_024,
                IntrinsicsLengthThreshold: 2_048,
                ParallelLengthThreshold: int.MaxValue,
                MaxDegreeOfParallelism: 1));

            Gemv.Multiply(a, rows, cols, x, scalarResult);

            DataStructureCompatibility.SetPerformanceSettings(new DataStructurePerformanceSettings(
                EnableUnsafe: true,
                EnableSimd: true,
                EnableIntrinsics: true,
                EnableParallel: true,
                SimdLengthThreshold: 1,
                IntrinsicsLengthThreshold: 1,
                ParallelLengthThreshold: 1,
                MaxDegreeOfParallelism: 4));

            Gemv.Multiply(a, rows, cols, x, acceleratedResult);

            AssertClose(scalarResult, acceleratedResult, 1e-10);
        }
        finally
        {
            DataStructureCompatibility.SetPerformanceSettings(original);
        }
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
