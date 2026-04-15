using System.Numerics;
using LAL.LinalgCore;

namespace LAL.Tests.LinalgCore;

public class SchurTests
{
    [Fact]
    public void RealSchur2x2_DrivesLowerLeftTowardZero()
    {
        double[] matrix =
        [
            2.0, 1.0,
            1.0, 2.0
        ];
        double[] schur = new double[4];

        Schur.RealSchur2x2(matrix, schur, iterations: 64);

        Assert.InRange(Math.Abs(schur[2]), 0, 1e-6);
        Assert.InRange(schur[0] + schur[3], 4.0 - 1e-8, 4.0 + 1e-8);
    }

    [Fact]
    public void ComplexSchur2x2_ReturnsUpperTriangularWithMatchingTraceAndDeterminant()
    {
        Complex[] matrix =
        [
            new Complex(1.0, 1.0), new Complex(2.0, 0.0),
            new Complex(3.0, 0.0), new Complex(4.0, -1.0)
        ];
        Complex[] schur = new Complex[4];

        Schur.ComplexSchur2x2(matrix, schur);

        Assert.InRange(Complex.Abs(schur[2]), 0d, 1e-12);

        Complex traceOriginal = matrix[0] + matrix[3];
        Complex traceSchur = schur[0] + schur[3];
        Assert.InRange(Complex.Abs(traceSchur - traceOriginal), 0d, 1e-10);

        Complex determinantOriginal = (matrix[0] * matrix[3]) - (matrix[1] * matrix[2]);
        Complex determinantSchur = schur[0] * schur[3];
        Assert.InRange(Complex.Abs(determinantSchur - determinantOriginal), 0d, 1e-10);
    }
}
