using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using LAL.TensorCore;

namespace LAL.Benches;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80, launchCount: 1, warmupCount: 2, iterationCount: 6)]
public class Convolution2DThresholdCalibrationBenchmarks
{
    [Params(48, 64, 128)]
    public int SignalSize { get; set; }

    [Params(5, 9)]
    public int KernelSize { get; set; }

    private double[] _signal = [];
    private double[] _kernel = [];
    private double[] _destination = [];

    [GlobalSetup]
    public void GlobalSetup()
    {
        _signal = new double[SignalSize * SignalSize];
        _kernel = new double[KernelSize * KernelSize];
        _destination = new double[(SignalSize + KernelSize - 1) * (SignalSize + KernelSize - 1)];

        for (int i = 0; i < _signal.Length; i++)
        {
            _signal[i] = ((i * 17) % 31 - 15) * 0.05;
        }

        for (int i = 0; i < _kernel.Length; i++)
        {
            _kernel[i] = ((i * 7) % 13 - 6) * 0.1;
        }
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        Convolution.ResetParallelSettings();
    }

    [Benchmark(Baseline = true)]
    public double Convolve2D_SequentialGate()
    {
        ApplySequentialProfile();
        Convolution.Convolve2D(_signal, SignalSize, SignalSize, _kernel, KernelSize, KernelSize, _destination);
        return _destination[_destination.Length / 2];
    }

    [Benchmark]
    public double Convolve2D_ForcedParallelGate()
    {
        ApplyForcedParallelProfile();
        Convolution.Convolve2D(_signal, SignalSize, SignalSize, _kernel, KernelSize, KernelSize, _destination);
        return _destination[_destination.Length / 2];
    }

    [Benchmark]
    public double Convolve2D_AutoAdaptiveGate()
    {
        Convolution.ResetParallelSettings();
        Convolution.Convolve2D(_signal, SignalSize, SignalSize, _kernel, KernelSize, KernelSize, _destination);
        return _destination[_destination.Length / 2];
    }

    private static void ApplySequentialProfile()
    {
        Convolution.SetParallelSettings(
            ConvolutionParallelSettings.Default with
            {
                EnableAdaptiveThresholding = false,
                Parallel2DOutputThreshold = int.MaxValue / 4,
                Parallel2DOpThreshold = long.MaxValue / 4,
                ParallelNdOutputThreshold = int.MaxValue / 4,
                ParallelNdOpThreshold = long.MaxValue / 4
            });
    }

    private static void ApplyForcedParallelProfile()
    {
        Convolution.SetParallelSettings(
            ConvolutionParallelSettings.Default with
            {
                EnableAdaptiveThresholding = false,
                Parallel2DOutputThreshold = 1,
                Parallel2DOpThreshold = 1,
                ParallelNdOutputThreshold = 1,
                ParallelNdOpThreshold = 1,
                MaxDegreeOfParallelism = Environment.ProcessorCount
            });
    }
}

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80, launchCount: 1, warmupCount: 2, iterationCount: 6)]
public class ConvolutionNdThresholdCalibrationBenchmarks
{
    [Params(10, 12, 16)]
    public int SignalEdge { get; set; }

    [Params(3, 5)]
    public int KernelEdge { get; set; }

    private int[] _signalShape = [];
    private int[] _kernelShape = [];
    private double[] _signal = [];
    private double[] _kernel = [];
    private double[] _destination = [];

    [GlobalSetup]
    public void GlobalSetup()
    {
        _signalShape = [SignalEdge, SignalEdge, SignalEdge];
        _kernelShape = [KernelEdge, KernelEdge, KernelEdge];

        _signal = new double[TensorShape.GetElementCount(_signalShape)];
        _kernel = new double[TensorShape.GetElementCount(_kernelShape)];

        int[] outputShape = Convolution.GetFullOutputShape(_signalShape, _kernelShape);
        _destination = new double[TensorShape.GetElementCount(outputShape)];

        for (int i = 0; i < _signal.Length; i++)
        {
            _signal[i] = ((i * 19) % 37 - 18) * 0.04;
        }

        for (int i = 0; i < _kernel.Length; i++)
        {
            _kernel[i] = ((i * 5) % 17 - 8) * 0.08;
        }
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        Convolution.ResetParallelSettings();
    }

    [Benchmark(Baseline = true)]
    public double ConvolveND_SequentialGate()
    {
        ApplySequentialProfile();
        Convolution.ConvolveNDFull(_signal, _signalShape, _kernel, _kernelShape, _destination);
        return _destination[_destination.Length / 2];
    }

    [Benchmark]
    public double ConvolveND_ForcedParallelGate()
    {
        ApplyForcedParallelProfile();
        Convolution.ConvolveNDFull(_signal, _signalShape, _kernel, _kernelShape, _destination);
        return _destination[_destination.Length / 2];
    }

    [Benchmark]
    public double ConvolveND_AutoAdaptiveGate()
    {
        Convolution.ResetParallelSettings();
        Convolution.ConvolveNDFull(_signal, _signalShape, _kernel, _kernelShape, _destination);
        return _destination[_destination.Length / 2];
    }

    private static void ApplySequentialProfile()
    {
        Convolution.SetParallelSettings(
            ConvolutionParallelSettings.Default with
            {
                EnableAdaptiveThresholding = false,
                Parallel2DOutputThreshold = int.MaxValue / 4,
                Parallel2DOpThreshold = long.MaxValue / 4,
                ParallelNdOutputThreshold = int.MaxValue / 4,
                ParallelNdOpThreshold = long.MaxValue / 4
            });
    }

    private static void ApplyForcedParallelProfile()
    {
        Convolution.SetParallelSettings(
            ConvolutionParallelSettings.Default with
            {
                EnableAdaptiveThresholding = false,
                Parallel2DOutputThreshold = 1,
                Parallel2DOpThreshold = 1,
                ParallelNdOutputThreshold = 1,
                ParallelNdOpThreshold = 1,
                MaxDegreeOfParallelism = Environment.ProcessorCount
            });
    }
}

