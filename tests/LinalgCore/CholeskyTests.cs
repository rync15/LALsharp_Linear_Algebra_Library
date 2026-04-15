using System.Numerics;
using LAL.LinalgCore;

namespace LAL.Tests.LinalgCore;

public class CholeskyTests
{
    [Fact]
    public void DecomposeLower_FactorsSpdMatrix()
    {
        double[] matrix =
        [
            4.0, 2.0,
            2.0, 3.0
        ];
        double[] lower = new double[4];

        bool ok = Cholesky.DecomposeLower(matrix, n: 2, lower);

        Assert.True(ok);
        Assert.InRange(lower[0], 2.0 - 1e-12, 2.0 + 1e-12);
        Assert.InRange(lower[2], 1.0 - 1e-12, 1.0 + 1e-12);
        Assert.InRange(lower[3], Math.Sqrt(2.0) - 1e-12, Math.Sqrt(2.0) + 1e-12);

        double a01 = lower[0] * lower[2];
        double a11 = (lower[2] * lower[2]) + (lower[3] * lower[3]);
        Assert.InRange(a01, 2.0 - 1e-10, 2.0 + 1e-10);
        Assert.InRange(a11, 3.0 - 1e-10, 3.0 + 1e-10);
    }

    [Fact]
    public void DecomposeLower_FactorsHermitianPositiveDefiniteComplexMatrix()
    {
        Complex[] matrix =
        [
            new Complex(4.0, 0.0), new Complex(1.0, 1.0),
            new Complex(1.0, -1.0), new Complex(3.0, 0.0)
        ];
        Complex[] lower = new Complex[4];

        bool ok = Cholesky.DecomposeLower(matrix, n: 2, lower);

        Assert.True(ok);

        Complex[] reconstructed = new Complex[4];
        for (int row = 0; row < 2; row++)
        {
            for (int col = 0; col < 2; col++)
            {
                Complex sum = Complex.Zero;
                int maxK = Math.Min(row, col);
                for (int k = 0; k <= maxK; k++)
                {
                    sum += lower[(row * 2) + k] * Complex.Conjugate(lower[(col * 2) + k]);
                }

                reconstructed[(row * 2) + col] = sum;
            }
        }

        for (int i = 0; i < matrix.Length; i++)
        {
            Assert.InRange(Complex.Abs(reconstructed[i] - matrix[i]), 0d, 1e-10);
        }
    }
}
