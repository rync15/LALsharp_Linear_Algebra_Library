using System.Numerics;
using LAL.TensorCore;

namespace LAL.Tests.TensorCore;

public class TypeCoverageTests
{
    [Fact]
    public void Arithmetic_SupportsFloatAndComplex()
    {
        float[] fa = [1f, 2f];
        float[] fb = [3f, 4f];
        float[] fdst = new float[2];

        UFuncArithmetic.Add(fa, fb, fdst);
        Assert.Equal(new[] { 4f, 6f }, fdst);

        UFuncArithmetic.Subtract(fb, fa, fdst);
        Assert.Equal(new[] { 2f, 2f }, fdst);

        UFuncArithmetic.Multiply(fa, fb, fdst);
        Assert.Equal(new[] { 3f, 8f }, fdst);

        UFuncArithmetic.Divide(fb, fa, fdst);
        Assert.Equal(new[] { 3f, 2f }, fdst);

        UFuncArithmetic.Power(fa, 2f, fdst);
        Assert.Equal(new[] { 1f, 4f }, fdst);

        Complex[] ca = [new Complex(1, 1), new Complex(2, -1)];
        Complex[] cb = [new Complex(1, -1), new Complex(0, 2)];
        Complex[] cdst = new Complex[2];

        UFuncArithmetic.Add(ca, cb, cdst);
        Assert.Equal(new Complex(2, 0), cdst[0]);

        UFuncArithmetic.Multiply(ca, cb, cdst);
        Assert.Equal(new Complex(2, 0), cdst[0]);

        UFuncArithmetic.Power(ca, 2d, cdst);
        Assert.InRange(cdst[0].Real, -1e-12, 1e-12);
        Assert.InRange(cdst[0].Imaginary, 2d - 1e-12, 2d + 1e-12);
    }

    [Fact]
    public void Transcendental_SupportsFloatAndComplex()
    {
        float[] fv = [0f, 1f];
        float[] fdst = new float[2];

        UFuncTranscendental.Exp(fv, fdst);
        Assert.InRange(fdst[1], MathF.E - 1e-5f, MathF.E + 1e-5f);

        UFuncTranscendental.Tanh(fv, fdst);
        Assert.InRange(fdst[0], -1e-6f, 1e-6f);

        Complex[] cv = [Complex.One, new Complex(0, 1)];
        Complex[] cdst = new Complex[2];

        UFuncTranscendental.Ln(cv, cdst);
        Assert.InRange(cdst[0].Real, -1e-12, 1e-12);
        Assert.InRange(cdst[0].Imaginary, -1e-12, 1e-12);

        UFuncTranscendental.Sin(cv, cdst);
        Assert.False(double.IsNaN(cdst[1].Real));
        Assert.False(double.IsNaN(cdst[1].Imaginary));
    }

    [Fact]
    public void StructuralOps_SupportsFloatAndComplex()
    {
        float[] left = [1f, 2f];
        float[] right = [3f, 4f, 5f];
        float[] concat = new float[left.Length + right.Length];
        ConcatStack.Concatenate(left, right, concat);
        Assert.Equal(new[] { 1f, 2f, 3f, 4f, 5f }, concat);

        Complex[] top = [new Complex(1, 0), new Complex(2, 0)];
        Complex[] bottom = [new Complex(3, 0), new Complex(4, 0)];
        Complex[] stacked = new Complex[4];
        ConcatStack.Stack(top, bottom, cols: 2, stacked);
        Assert.Equal(new Complex(3, 0), stacked[2]);

        float[] input = [1f, 2f, 3f, 4f];
        float[] transposed = new float[4];
        ShapeOps.Transpose2D(input, rows: 2, cols: 2, transposed);
        Assert.Equal(new[] { 1f, 3f, 2f, 4f }, transposed);

        Complex[] cinput = [new Complex(1, 1), new Complex(2, 2)];
        Complex[] cflat = new Complex[2];
        ShapeOps.Flatten(cinput, cflat);
        Assert.Equal(cinput, cflat);

        float[] padded = Padding.PeriodicPad1D(input, left: 1, right: 1);
        Assert.Equal(6, padded.Length);

        Complex[] cpadded = Padding.ZeroPad1D(cinput, left: 1, right: 1);
        Assert.Equal(4, cpadded.Length);
    }

    [Fact]
    public void ReductionsMaskSortCumulative_SupportsFloatAndComplex()
    {
        float[] fv = [1f, float.NaN, 3f];
        Assert.Equal(4f, Reductions.SumNanSafe(fv), 5);
        Assert.Equal(2f, Reductions.MeanNanSafe(fv), 5);

        Complex[] cv = [new Complex(1, 1), new Complex(2, -1), new Complex(double.NaN, 0)];
        Complex sumSafe = Reductions.SumNanSafe(cv);
        Assert.InRange(sumSafe.Real, 3 - 1e-12, 3 + 1e-12);

        float[] csumDst = new float[2];
        Cumulative.CumSum([1f, 2f], csumDst);
        Assert.Equal(new[] { 1f, 3f }, csumDst);

        Complex[] cprodDst = new Complex[2];
        Cumulative.CumProd([new Complex(1, 1), new Complex(1, -1)], cprodDst);
        Assert.Equal(new Complex(2, 0), cprodDst[1]);

        int[] idx = new int[3];
        float[] sortable = [3f, 1f, 2f];
        SortSearch.Argsort(sortable, idx);
        Assert.Equal(new[] { 1, 2, 0 }, idx);

        int argMaxComplex = SortSearch.ArgMax([new Complex(1, 0), new Complex(0, 3)]);
        Assert.Equal(1, argMaxComplex);

        bool[] mask = new bool[3];
        MaskOps.GreaterThan([1f, 3f, 2f], 2f, mask);
        Assert.Equal(new[] { false, true, false }, mask);

        Complex[] whereDst = new Complex[2];
        MaskOps.Where([new Complex(1, 0), new Complex(2, 0)], [true, false], whereDst);
        Assert.Equal(Complex.Zero, whereDst[1]);
    }

    [Fact]
    public void EinsumConvolutionFft_SupportsFloatAndComplex()
    {
        float[] a = [1f, 2f, 3f, 4f];
        float[] b = [5f, 6f, 7f, 8f];
        float[] dst = new float[4];
        Einsum.TensorContractionGemm(a, b, dst, m: 2, n: 2, k: 2);
        Assert.Equal(new[] { 19f, 22f, 43f, 50f }, dst);

        Complex[] left = [new Complex(1, 1), new Complex(2, 0)];
        Complex[] right = [new Complex(1, -1), new Complex(0, 2)];
        Complex dot = Einsum.Evaluate("i,i->", left, right);
        Assert.False(double.IsNaN(dot.Real));

        float[] signal = [1f, 2f];
        float[] kernel = [1f, 1f];
        float[] conv = new float[3];
        Convolution.Convolve1D(signal, kernel, conv);
        Assert.Equal(new[] { 1f, 3f, 2f }, conv);

        Complex[] csignal = [new Complex(1, 0), new Complex(2, 0)];
        Complex[] ckernel = [new Complex(1, 0), new Complex(1, 0)];
        Complex[] cconv = new Complex[3];
        Convolution.Convolve1D(csignal, ckernel, cconv);
        Assert.Equal(new Complex(3, 0), cconv[1]);

        Complex[] fIn = [new Complex(1, 0), new Complex(2, 0), new Complex(3, 0), new Complex(4, 0)];
        Complex[] spectrum = new Complex[4];
        Complex[] restored = new Complex[4];
        Fft.Forward1D(fIn, spectrum);
        Fft.Inverse1D(spectrum, restored);
        Assert.InRange(Math.Abs(restored[0].Real - 1d), 0d, 1e-9);

        Complex[] rfftOut = new Complex[4];
        float[] realInput = [1f, 2f, 3f, 4f];
        Fft.Rfft(realInput, rfftOut);
        Assert.Equal(4, rfftOut.Length);
    }
}
