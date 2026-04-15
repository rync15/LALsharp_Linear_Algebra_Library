using LAL.Core;
using System.Numerics;
using LAL.LinalgCore;

namespace LAL.Tests.LinalgCore;

public class GemmTests
{
    [Fact]
    public void Multiply_ComputesMatrixMatrixProduct()
    {
        double[] a =
        [
            1, 2,
            3, 4
        ];
        double[] b =
        [
            5, 6,
            7, 8
        ];
        double[] c = new double[4];

        Gemm.Multiply(a, b, c, m: 2, n: 2, k: 2);

        Assert.Equal([19.0, 22.0, 43.0, 50.0], c);
    }

    [Fact]
    public void Multiply_Float_ComputesMatrixMatrixProduct()
    {
        float[] a =
        [
            1f, 2f,
            3f, 4f
        ];
        float[] b =
        [
            2f, 0f,
            1f, 2f
        ];
        float[] c = new float[4];

        Gemm.Multiply(a, b, c, m: 2, n: 2, k: 2);

        Assert.Equal([4f, 4f, 10f, 8f], c);
    }

    [Fact]
    public void Multiply_Complex_ComputesMatrixMatrixProduct()
    {
        Complex[] a =
        [
            new Complex(1, 0), new Complex(0, 1),
            new Complex(2, -1), new Complex(1, 0)
        ];
        Complex[] b =
        [
            new Complex(1, 0), new Complex(2, 0),
            new Complex(0, -1), new Complex(1, 1)
        ];
        Complex[] c = new Complex[4];

        Gemm.Multiply(a, b, c, m: 2, n: 2, k: 2);

        Assert.Equal(new Complex(2, 0), c[0]);
        Assert.Equal(new Complex(1, 1), c[1]);
        Assert.Equal(new Complex(2, -2), c[2]);
        Assert.Equal(new Complex(5, -1), c[3]);
    }

    [Fact]
    public void Multiply_StrategyToggle_ProducesConsistentResult()
    {
        DataStructurePerformanceSettings original = DataStructureCompatibility.GetPerformanceSettings();

        try
        {
            const int m = 16;
            const int n = 16;
            const int k = 16;

            double[] a = new double[m * k];
            double[] b = new double[k * n];
            for (int i = 0; i < a.Length; i++)
            {
                a[i] = ((i * 3) % 13) - 6;
            }

            for (int i = 0; i < b.Length; i++)
            {
                b[i] = ((i * 5) % 17) - 8;
            }

            double[] scalarResult = new double[m * n];
            double[] acceleratedResult = new double[m * n];

            DataStructureCompatibility.SetPerformanceSettings(new DataStructurePerformanceSettings(
                EnableUnsafe: false,
                EnableSimd: false,
                EnableIntrinsics: false,
                EnableParallel: false,
                SimdLengthThreshold: 1_024,
                IntrinsicsLengthThreshold: 2_048,
                ParallelLengthThreshold: int.MaxValue,
                MaxDegreeOfParallelism: 1));

            Gemm.Multiply(a, b, scalarResult, m, n, k);

            DataStructureCompatibility.SetPerformanceSettings(new DataStructurePerformanceSettings(
                EnableUnsafe: true,
                EnableSimd: true,
                EnableIntrinsics: true,
                EnableParallel: true,
                SimdLengthThreshold: 1,
                IntrinsicsLengthThreshold: 1,
                ParallelLengthThreshold: 1,
                MaxDegreeOfParallelism: 4));

            Gemm.Multiply(a, b, acceleratedResult, m, n, k);

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
