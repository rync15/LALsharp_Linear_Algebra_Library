using System.Numerics;
using LAL.NumericalCore.Integration;

namespace LAL.Tests.NumericalCore.Integration;

public class GaussianQuadratureTests
{
    [Fact]
    public void Integrate_ApproximatesPolynomialIntegral()
    {
        double result = GaussianQuadrature.Integrate(static x => x * x, a: 0.0, b: 1.0, order: 2);

        Assert.InRange(result, (1.0 / 3.0) - 1e-12, (1.0 / 3.0) + 1e-12);
    }

    [Fact]
    public void Integrate_Float_ApproximatesPolynomialIntegral()
    {
        float result = GaussianQuadrature.Integrate(static x => x * x, a: 0f, b: 1f, order: 2);

        Assert.InRange(result, (1f / 3f) - 1e-5f, (1f / 3f) + 1e-5f);
    }

    [Fact]
    public void Integrate_Complex_ApproximatesPolynomialIntegral()
    {
        Complex result = GaussianQuadrature.Integrate(static x => new Complex(x * x, x), a: 0.0, b: 1.0, order: 3);

        Assert.InRange(result.Real, (1.0 / 3.0) - 1e-12, (1.0 / 3.0) + 1e-12);
        Assert.InRange(result.Imaginary, 0.5 - 1e-12, 0.5 + 1e-12);
    }
}
