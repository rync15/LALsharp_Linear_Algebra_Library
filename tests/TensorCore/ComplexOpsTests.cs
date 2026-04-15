using System.Numerics;
using LAL.TensorCore;

namespace LAL.Tests.TensorCore;

public class ComplexOpsTests
{
    [Fact]
    public void ReAndIm_ExtractComponents()
    {
        Complex[] values =
        [
            new Complex(1.5, -2.0),
            new Complex(-3.0, 4.5)
        ];

        double[] real = new double[2];
        double[] imaginary = new double[2];

        ComplexOps.Re(values, real);
        ComplexOps.Im(values, imaginary);

        Assert.Equal(new[] { 1.5, -3.0 }, real);
        Assert.Equal(new[] { -2.0, 4.5 }, imaginary);
    }

    [Fact]
    public void Conjugate_FlipsImaginarySign()
    {
        Complex[] values =
        [
            new Complex(1.0, 2.0),
            new Complex(-3.0, -4.0)
        ];

        Complex[] destination = new Complex[2];
        ComplexOps.Conjugate(values, destination);

        Assert.Equal(new Complex(1.0, -2.0), destination[0]);
        Assert.Equal(new Complex(-3.0, 4.0), destination[1]);
    }
}
