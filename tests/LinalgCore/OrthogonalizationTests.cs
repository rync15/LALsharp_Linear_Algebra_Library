using System.Numerics;
using LAL.LinalgCore;

namespace LAL.Tests.LinalgCore;

public class OrthogonalizationTests
{
    [Fact]
    public void OrthogonalProjection_ProjectsOntoBasis()
    {
        double[] vector = [3.0, 4.0];
        double[] basis = [1.0, 0.0];
        double[] projection = new double[2];

        Orthogonalization.OrthogonalProjection(vector, basis, projection);

        Assert.InRange(projection[0], 3.0 - 1e-12, 3.0 + 1e-12);
        Assert.InRange(projection[1], -1e-12, 1e-12);
    }

    [Fact]
    public void GramSchmidtOrthonormalize_ReturnsOrthonormalBasis()
    {
        double[] vectors =
        [
            1.0, 0.0,
            1.0, 1.0
        ];

        double[] output = new double[4];

        int rank = Orthogonalization.GramSchmidtOrthonormalize(vectors, vectorCount: 2, dimension: 2, output);

        Assert.Equal(2, rank);
        Assert.InRange(output[0], 1.0 - 1e-12, 1.0 + 1e-12);
        Assert.InRange(output[1], -1e-12, 1e-12);
        Assert.InRange(output[2], -1e-12, 1e-12);
        Assert.InRange(output[3], 1.0 - 1e-12, 1.0 + 1e-12);
    }

    [Fact]
    public void OrthogonalProjection_Complex_ProjectsOntoBasis()
    {
        Complex[] vector = [new Complex(3.0, 4.0), new Complex(2.0, -1.0)];
        Complex[] basis = [Complex.One, Complex.Zero];
        Complex[] projection = new Complex[2];

        Orthogonalization.OrthogonalProjection(vector, basis, projection);

        Assert.InRange(Complex.Abs(projection[0] - new Complex(3.0, 4.0)), 0d, 1e-12);
        Assert.InRange(Complex.Abs(projection[1]), 0d, 1e-12);
    }

    [Fact]
    public void GramSchmidtOrthonormalize_Complex_ReturnsOrthonormalBasis()
    {
        Complex[] vectors =
        [
            Complex.One, Complex.Zero,
            Complex.Zero, Complex.ImaginaryOne
        ];

        Complex[] output = new Complex[4];

        int rank = Orthogonalization.GramSchmidtOrthonormalize(vectors, vectorCount: 2, dimension: 2, output);

        Assert.Equal(2, rank);

        Complex[] q0 = [output[0], output[1]];
        Complex[] q1 = [output[2], output[3]];

        Assert.InRange(Norms.L2(q0), 1.0 - 1e-10, 1.0 + 1e-10);
        Assert.InRange(Norms.L2(q1), 1.0 - 1e-10, 1.0 + 1e-10);
        Assert.InRange(Complex.Abs(Dot.Dotc(q0, q1)), 0d, 1e-10);
    }
}
