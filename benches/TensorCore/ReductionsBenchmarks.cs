using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using LAL.TensorCore;

namespace LAL.Benches;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80, launchCount: 1, warmupCount: 1, iterationCount: 5)]
public class ReductionsBenchmarks
{
    [Params(64, 256, 1024)]
    public int Length { get; set; }

    private double[] _valuesDouble = [];
    private float[] _valuesFloat = [];

    [GlobalSetup]
    public void GlobalSetup()
    {
        _valuesDouble = new double[Length];
        _valuesFloat = new float[Length];

        for (int i = 0; i < Length; i++)
        {
            double value = Math.Sin(i * 0.137) * 7.5 + (i % 11);
            _valuesDouble[i] = value;
            _valuesFloat[i] = (float)value;
        }
    }

    [Benchmark]
    public double QuantileDouble()
    {
        return Reductions.Quantile(_valuesDouble, 0.95d);
    }

    [Benchmark]
    public float QuantileFloat()
    {
        return Reductions.Quantile(_valuesFloat, 0.95f);
    }

    [Benchmark]
    public double MeanDouble()
    {
        return Reductions.Mean(_valuesDouble);
    }
}

