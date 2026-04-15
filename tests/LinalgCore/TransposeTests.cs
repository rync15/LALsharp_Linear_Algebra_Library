using System.Numerics;
using LAL.LinalgCore;

namespace LAL.Tests.LinalgCore;

public class TransposeTests
{
    [Fact]
    public void Matrix_TransposesRowMajorMatrix()
    {
        double[] input =
        [
            1, 2, 3,
            4, 5, 6
        ];

        double[] output = new double[input.Length];

        Transpose.Matrix(input, rows: 2, cols: 3, output);

        double[] expected =
        [
            1, 4,
            2, 5,
            3, 6
        ];

        Assert.Equal(expected, output);
    }

    [Fact]
    public void Matrix_Float_TransposesRowMajorMatrix()
    {
        float[] input =
        [
            1f, 2f, 3f,
            4f, 5f, 6f
        ];

        float[] output = new float[input.Length];

        Transpose.Matrix(input, rows: 2, cols: 3, output);

        float[] expected =
        [
            1f, 4f,
            2f, 5f,
            3f, 6f
        ];

        Assert.Equal(expected, output);
    }

    [Fact]
    public void Matrix_Complex_TransposesWithoutConjugation()
    {
        Complex[] input =
        [
            new Complex(1, 2), new Complex(3, -4),
            new Complex(-5, 1), new Complex(0, -2)
        ];

        Complex[] output = new Complex[input.Length];

        Transpose.Matrix(input, rows: 2, cols: 2, output);

        Complex[] expected =
        [
            new Complex(1, 2),
            new Complex(-5, 1),
            new Complex(3, -4),
            new Complex(0, -2)
        ];

        Assert.Equal(expected, output);
    }

    [Fact]
    public void ConjugateTranspose_ConjugatesAndTransposes()
    {
        Complex[] input =
        [
            new Complex(1, 2), new Complex(3, -4),
            new Complex(-5, 1), new Complex(0, -2)
        ];

        Complex[] output = new Complex[input.Length];

        Transpose.ConjugateTranspose(input, rows: 2, cols: 2, output);

        Complex[] expected =
        [
            Complex.Conjugate(new Complex(1, 2)),
            Complex.Conjugate(new Complex(-5, 1)),
            Complex.Conjugate(new Complex(3, -4)),
            Complex.Conjugate(new Complex(0, -2))
        ];

        Assert.Equal(expected, output);
    }
}
