using LAL.LinalgCore;

namespace LAL.Tests.LinalgCore;

public class InvariantPropertyTests
{
    [Fact]
    public void Dotu_IsLinear_ForRandomVectors()
    {
        Random rng = new(20260410);

        for (int trial = 0; trial < 32; trial++)
        {
            double[] x = NextVector(rng, 32);
            double[] y = NextVector(rng, 32);
            double[] z = NextVector(rng, 32);

            double a = (rng.NextDouble() * 4.0) - 2.0;
            double b = (rng.NextDouble() * 4.0) - 2.0;

            double[] combo = new double[x.Length];
            for (int i = 0; i < combo.Length; i++)
            {
                combo[i] = (a * x[i]) + (b * y[i]);
            }

            double left = Dot.Dotu(combo, z);
            double right = (a * Dot.Dotu(x, z)) + (b * Dot.Dotu(y, z));

            Assert.InRange(Math.Abs(left - right), 0.0, 1e-8);
        }
    }

    [Fact]
    public void NormOrdering_Holds_ForRandomVectors()
    {
        Random rng = new(20260410);

        for (int trial = 0; trial < 64; trial++)
        {
            double[] values = NextVector(rng, 48);

            double lInf = Norms.Infinity(values);
            double l2 = Norms.L2(values);
            double l1 = Norms.L1(values);

            Assert.True(lInf <= l2 + 1e-12);
            Assert.True(l2 <= l1 + 1e-12);
        }
    }

    [Fact]
    public void Axpy_MatchesReference_ForRandomInputs()
    {
        Random rng = new(20260410);

        for (int trial = 0; trial < 32; trial++)
        {
            double alpha = (rng.NextDouble() * 6.0) - 3.0;
            double[] x = NextVector(rng, 40);
            double[] y = NextVector(rng, 40);
            double[] expected = (double[])y.Clone();

            for (int i = 0; i < expected.Length; i++)
            {
                expected[i] = (alpha * x[i]) + expected[i];
            }

            Axpy.Compute(alpha, x, y);

            for (int i = 0; i < y.Length; i++)
            {
                Assert.InRange(Math.Abs(y[i] - expected[i]), 0.0, 1e-12);
            }
        }
    }

    private static double[] NextVector(Random rng, int length)
    {
        double[] values = new double[length];
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = (rng.NextDouble() * 2.0) - 1.0;
        }

        return values;
    }
}