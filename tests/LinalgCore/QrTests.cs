using System.Numerics;
using LAL.LinalgCore;

namespace LAL.Tests.LinalgCore;

public class QrTests
{
    [Fact]
    public void DecomposeThin_ReconstructsMatrix()
    {
        double[] a =
        [
            1.0, 1.0,
            1.0, 0.0
        ];

        double[] q = new double[4];
        double[] r = new double[4];

        bool ok = Qr.DecomposeThin(a, rows: 2, cols: 2, q, r);

        Assert.True(ok);

        double dotColumns = (q[0] * q[1]) + (q[2] * q[3]);
        Assert.InRange(Math.Abs(dotColumns), 0, 1e-10);

        double[] reconstructed = new double[4];
        for (int row = 0; row < 2; row++)
        {
            for (int col = 0; col < 2; col++)
            {
                double sum = 0d;
                for (int k = 0; k < 2; k++)
                {
                    sum += q[(row * 2) + k] * r[(k * 2) + col];
                }

                reconstructed[(row * 2) + col] = sum;
            }
        }

        for (int i = 0; i < a.Length; i++)
        {
            Assert.InRange(Math.Abs(reconstructed[i] - a[i]), 0, 1e-10);
        }
    }

    [Fact]
    public void DecomposeThin_Complex_ReconstructsMatrix()
    {
        Complex[] a =
        [
            new Complex(1.0, 0.0), new Complex(0.0, 1.0),
            new Complex(1.0, 0.0), Complex.Zero
        ];

        Complex[] q = new Complex[4];
        Complex[] r = new Complex[4];

        bool ok = Qr.DecomposeThin(a, rows: 2, cols: 2, q, r);

        Assert.True(ok);

        Complex[] reconstructed = new Complex[4];
        for (int row = 0; row < 2; row++)
        {
            for (int col = 0; col < 2; col++)
            {
                Complex sum = Complex.Zero;
                for (int k = 0; k < 2; k++)
                {
                    sum += q[(row * 2) + k] * r[(k * 2) + col];
                }

                reconstructed[(row * 2) + col] = sum;
            }
        }

        for (int i = 0; i < a.Length; i++)
        {
            Assert.InRange(Complex.Abs(reconstructed[i] - a[i]), 0d, 1e-10);
        }

        Complex[] qCol0 = [q[0], q[2]];
        Complex[] qCol1 = [q[1], q[3]];
        Assert.InRange(Complex.Abs(Dot.Dotc(qCol0, qCol1)), 0d, 1e-10);
    }
}
