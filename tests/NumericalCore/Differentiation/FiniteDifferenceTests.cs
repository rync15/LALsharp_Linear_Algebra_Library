using System.Numerics;
using LAL.NumericalCore.Differentiation;

namespace LAL.Tests.NumericalCore.Differentiation;

public class FiniteDifferenceTests
{
    [Fact]
    public void ForwardBackwardCentral_ApproximateDerivative()
    {
        static double F(double x) => x * x;

        double forward = FiniteDifference.Forward(F, x: 2.0);
        double backward = FiniteDifference.Backward(F, x: 2.0);
        double central = FiniteDifference.Central(F, x: 2.0);

        Assert.InRange(forward, 4.0 - 1e-3, 4.0 + 1e-3);
        Assert.InRange(backward, 4.0 - 1e-3, 4.0 + 1e-3);
        Assert.InRange(central, 4.0 - 1e-6, 4.0 + 1e-6);
    }

    [Fact]
    public void Float_ApproximatesDerivative()
    {
        static float F(float x) => x * x;

        float central = FiniteDifference.Central(F, x: 2f);

        Assert.InRange(central, 4f - 1e-2f, 4f + 1e-2f);
    }

    [Fact]
    public void Complex_ApproximatesDerivative()
    {
        static Complex F(double x) => new Complex(x * x, x);

        Complex central = FiniteDifference.Central(F, x: 2.0);

        Assert.InRange(central.Real, 4.0 - 1e-5, 4.0 + 1e-5);
        Assert.InRange(central.Imaginary, 1.0 - 1e-6, 1.0 + 1e-6);
    }
}
