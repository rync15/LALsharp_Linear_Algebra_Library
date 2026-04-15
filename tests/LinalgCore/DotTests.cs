using System.Numerics;
using LAL.LinalgCore;

namespace LAL.Tests.LinalgCore;

public class DotTests
{
    [Fact]
    public void Dotu_Double_ReturnsExpectedValue()
    {
        double[] x = [1.0, 2.0, 3.0];
        double[] y = [4.0, 5.0, 6.0];
        double result = Dot.Dotu(x, y);

        Assert.Equal(32.0, result);
    }

    [Fact]
    public void Dotu_Float_ReturnsExpectedValue()
    {
        float[] x = [1f, 2f, 3f];
        float[] y = [4f, 5f, 6f];
        float result = Dot.Dotu(x, y);

        Assert.Equal(32f, result);
    }

    [Fact]
    public void Dotu_Complex_DoesNotConjugateLeftOperand()
    {
        Complex[] x = [new Complex(1, 1), new Complex(2, -1)];
        Complex[] y = [new Complex(3, 0), new Complex(1, 2)];

        Complex result = Dot.Dotu(x, y);

        Assert.Equal(new Complex(7, 6), result);
    }

    [Fact]
    public void Dotc_Complex_UsesConjugationOnLeftOperand()
    {
        Complex[] x = [new Complex(1, 1), new Complex(2, -1)];
        Complex[] y = [new Complex(3, 0), new Complex(1, 2)];

        Complex result = Dot.Dotc(x, y);

        Assert.Equal(new Complex(3, 2), result);
    }
}
