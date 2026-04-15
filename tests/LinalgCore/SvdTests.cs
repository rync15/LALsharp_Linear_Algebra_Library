using System.Numerics;
using LAL.LinalgCore;

namespace LAL.Tests.LinalgCore;

public class SvdTests
{
    [Fact]
    public void SingularValues_ReturnsDiagonalSpectrum()
    {
        double[] matrix =
        [
            3.0, 0.0,
            0.0, 2.0
        ];
        double[] singularValues = new double[2];

        Svd.SingularValues(matrix, rows: 2, cols: 2, singularValues);

        Assert.InRange(singularValues[0], 3.0 - 1e-8, 3.0 + 1e-8);
        Assert.InRange(singularValues[1], 2.0 - 1e-8, 2.0 + 1e-8);
    }

    [Fact]
    public void SingularValues_Complex_DiagonalSpectrumMatchesMagnitudes()
    {
        Complex[] matrix =
        [
            new Complex(3.0, 4.0), Complex.Zero,
            Complex.Zero, new Complex(0.0, 2.0)
        ];
        double[] singularValues = new double[2];

        Svd.SingularValues(matrix, rows: 2, cols: 2, singularValues);

        Assert.InRange(singularValues[0], 5.0 - 1e-8, 5.0 + 1e-8);
        Assert.InRange(singularValues[1], 2.0 - 1e-8, 2.0 + 1e-8);
    }
}
