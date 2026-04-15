using System.Numerics;
using LAL.NumericalCore.Integration;

namespace LAL.Tests.NumericalCore.Integration;

public class BasicQuadratureTests
{
    [Fact]
    public void Trapezoidal_ApproximatesIntegralOfXSquared()
    {
        double result = BasicQuadrature.Trapezoidal(static x => x * x, 0.0, 1.0, 1000);

        Assert.InRange(result, (1.0 / 3.0) - 1e-4, (1.0 / 3.0) + 1e-4);
    }

    [Fact]
    public void Simpson_ApproximatesIntegralOfXSquared()
    {
        double result = BasicQuadrature.Simpson(static x => x * x, 0.0, 1.0, 100);

        Assert.InRange(result, (1.0 / 3.0) - 1e-8, (1.0 / 3.0) + 1e-8);
    }

    [Fact]
    public void Trapezoidal_Float_ApproximatesIntegralOfXSquared()
    {
        float result = BasicQuadrature.Trapezoidal(static x => x * x, 0f, 1f, 1000);

        Assert.InRange(result, (1f / 3f) - 1e-3f, (1f / 3f) + 1e-3f);
    }

    [Fact]
    public void Simpson_Complex_ApproximatesIntegral()
    {
        Complex result = BasicQuadrature.Simpson(static x => new Complex(x, x), 0.0, 1.0, 100);

        Assert.InRange(result.Real, 0.5 - 1e-8, 0.5 + 1e-8);
        Assert.InRange(result.Imaginary, 0.5 - 1e-8, 0.5 + 1e-8);
    }
}
