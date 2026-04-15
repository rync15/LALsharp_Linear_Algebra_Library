using LAL.TensorCore;

namespace LAL.Tests.TensorCore;

public class UFuncTranscendentalTests
{
    [Fact]
    public void ExpLogSinCos_Work()
    {
        double[] values = [0.0, 1.0, Math.PI / 2.0];
        double[] dst = new double[values.Length];

        UFuncTranscendental.Exp(values, dst);
        Assert.InRange(dst[1], Math.E - 1e-12, Math.E + 1e-12);

        UFuncTranscendental.Log(dst, dst);
        Assert.InRange(dst[1], 1.0 - 1e-10, 1.0 + 1e-10);

        UFuncTranscendental.Sin(values, dst);
        Assert.InRange(dst[2], 1.0 - 1e-12, 1.0 + 1e-12);

        UFuncTranscendental.Cos(values, dst);
        Assert.InRange(dst[0], 1.0 - 1e-12, 1.0 + 1e-12);
    }

    [Fact]
    public void ExtendedTranscendentalFunctions_Work()
    {
        double[] dst = new double[3];

        UFuncTranscendental.Tan([0.0, Math.PI / 4.0, 0.0], dst);
        Assert.InRange(dst[0], -1e-12, 1e-12);
        Assert.InRange(dst[1], 1.0 - 1e-12, 1.0 + 1e-12);

        UFuncTranscendental.Asin([-1.0, 0.0, 1.0], dst);
        Assert.InRange(dst[0], (-Math.PI / 2.0) - 1e-12, (-Math.PI / 2.0) + 1e-12);
        Assert.InRange(dst[2], (Math.PI / 2.0) - 1e-12, (Math.PI / 2.0) + 1e-12);

        UFuncTranscendental.Acos([-1.0, 0.0, 1.0], dst);
        Assert.InRange(dst[0], Math.PI - 1e-12, Math.PI + 1e-12);
        Assert.InRange(dst[2], -1e-12, 1e-12);

        UFuncTranscendental.Atan([0.0, 1.0, -1.0], dst);
        Assert.InRange(dst[0], -1e-12, 1e-12);
        Assert.InRange(dst[1], (Math.PI / 4.0) - 1e-12, (Math.PI / 4.0) + 1e-12);

        UFuncTranscendental.Ln([Math.E, 1.0, Math.E * Math.E], dst);
        Assert.InRange(dst[0], 1.0 - 1e-12, 1.0 + 1e-12);
        Assert.InRange(dst[1], -1e-12, 1e-12);
        Assert.InRange(dst[2], 2.0 - 1e-12, 2.0 + 1e-12);

        UFuncTranscendental.Sinh([0.0, 0.0, 0.0], dst);
        Assert.InRange(dst[0], -1e-12, 1e-12);

        UFuncTranscendental.Cosh([0.0, 0.0, 0.0], dst);
        Assert.InRange(dst[0], 1.0 - 1e-12, 1.0 + 1e-12);

        UFuncTranscendental.Tanh([0.0, 0.0, 0.0], dst);
        Assert.InRange(dst[0], -1e-12, 1e-12);
    }
}
