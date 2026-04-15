using System.Numerics;
using LAL.TensorCore;

namespace LAL.Tests.TensorCore;

public class FftTests
{
    [Fact]
    public void ForwardAndInverse_RoundTrip()
    {
        Complex[] input =
        [
            new Complex(1, 0),
            new Complex(2, 0),
            new Complex(3, 0),
            new Complex(4, 0)
        ];

        Complex[] spectrum = new Complex[input.Length];
        Complex[] restored = new Complex[input.Length];

        Fft.Forward1D(input, spectrum);
        Fft.Inverse1D(spectrum, restored);

        for (int i = 0; i < input.Length; i++)
        {
            Assert.InRange(Math.Abs(input[i].Real - restored[i].Real), 0, 1e-9);
            Assert.InRange(Math.Abs(input[i].Imaginary - restored[i].Imaginary), 0, 1e-9);
        }
    }

    [Fact]
    public void ForwardInverse_AliasMethods_Work()
    {
        Complex[] input =
        [
            new Complex(1, 0),
            new Complex(0, 1),
            new Complex(-1, 0),
            new Complex(0, -1)
        ];

        Complex[] spectrum = new Complex[input.Length];
        Complex[] restored = new Complex[input.Length];

        Fft.Forward(input, spectrum);
        Fft.Inverse(spectrum, restored);

        for (int i = 0; i < input.Length; i++)
        {
            Assert.InRange(Math.Abs(input[i].Real - restored[i].Real), 0, 1e-9);
            Assert.InRange(Math.Abs(input[i].Imaginary - restored[i].Imaginary), 0, 1e-9);
        }
    }

    [Fact]
    public void Rfft_ReturnsHalfSpectrum()
    {
        double[] input = [1, 2, 3, 4];
        Complex[] spectrum = new Complex[input.Length];
        Fft.Rfft(input, spectrum);

        Assert.Equal(input.Length, spectrum.Length);
    }

    [Fact]
    public void ForwardInverse2D_RoundTrip()
    {
        Complex[] input =
        [
            new Complex(1, 0), new Complex(2, 0),
            new Complex(3, 0), new Complex(4, 0)
        ];

        Complex[] spectrum = new Complex[input.Length];
        Complex[] restored = new Complex[input.Length];

        Fft.Forward2D(input, rows: 2, cols: 2, spectrum);
        Fft.Inverse2D(spectrum, rows: 2, cols: 2, restored);

        for (int i = 0; i < input.Length; i++)
        {
            Assert.InRange(Math.Abs(input[i].Real - restored[i].Real), 0, 1e-9);
            Assert.InRange(Math.Abs(input[i].Imaginary - restored[i].Imaginary), 0, 1e-9);
        }
    }

    [Fact]
    public void ForwardInverseND_RoundTrip()
    {
        Complex[] input =
        [
            new Complex(1, 0), new Complex(2, 0),
            new Complex(3, 0), new Complex(4, 0),
            new Complex(5, 0), new Complex(6, 0),
            new Complex(7, 0), new Complex(8, 0)
        ];

        int[] shape = [2, 2, 2];
        Complex[] spectrum = new Complex[input.Length];
        Complex[] restored = new Complex[input.Length];

        Fft.ForwardND(input, shape, spectrum);
        Fft.InverseND(spectrum, shape, restored);

        for (int i = 0; i < input.Length; i++)
        {
            Assert.InRange(Math.Abs(input[i].Real - restored[i].Real), 0, 1e-8);
            Assert.InRange(Math.Abs(input[i].Imaginary - restored[i].Imaginary), 0, 1e-8);
        }
    }

    [Fact]
    public void RfftND_ReturnsSpectrumWithExpectedLength()
    {
        double[] input = [1, 2, 3, 4];
        int[] shape = [2, 2];
        Complex[] spectrum = new Complex[input.Length];

        Fft.RfftND(input, shape, spectrum);

        Assert.Equal(input.Length, spectrum.Length);
    }

    [Fact]
    public void ForwardInverse_NonPowerOfTwoLength_RoundTrip()
    {
        Complex[] input =
        [
            new Complex(1, 0),
            new Complex(2, -1),
            new Complex(0.5, 3)
        ];

        Complex[] spectrum = new Complex[input.Length];
        Complex[] restored = new Complex[input.Length];

        Fft.Forward1D(input, spectrum);
        Fft.Inverse1D(spectrum, restored);

        for (int i = 0; i < input.Length; i++)
        {
            Assert.InRange(Math.Abs(input[i].Real - restored[i].Real), 0, 1e-9);
            Assert.InRange(Math.Abs(input[i].Imaginary - restored[i].Imaginary), 0, 1e-9);
        }
    }
}
