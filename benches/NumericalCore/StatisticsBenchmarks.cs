using BenchmarkDotNet.Attributes;
using LAL.NumericalCore.Statistics;
using System.Numerics;

namespace LAL.Benches;

[MemoryDiagnoser]
public class StatisticsBenchmarks
{
    private double[] _x = [];
    private double[] _y = [];
    private Complex[] _xc = [];
    private Complex[] _yc = [];

    [Params(4_096, 65_536)]
    public int Size { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _x = new double[Size];
        _y = new double[Size];
        _xc = new Complex[Size];
        _yc = new Complex[Size];

        for (int i = 0; i < Size; i++)
        {
            double t = i * 0.001;
            _x[i] = t;
            _y[i] = (2.0 * t) + ((i % 5) * 0.0001);

            _xc[i] = new Complex(t, 0.5 * t);
            _yc[i] = new Complex((2.0 * t) + ((i % 3) * 0.0001), (-0.25 * t) + ((i % 7) * 0.0001));
        }
    }

    [Benchmark]
    public double CovarianceDoubleSequential() => Covariance.Compute(_x, _y, sample: true, allowParallel: false);

    [Benchmark]
    public double CovarianceDoubleParallel() => Covariance.Compute(_x, _y, sample: true, allowParallel: true);

    [Benchmark]
    public Complex CovarianceComplexParallel() => Covariance.Compute(_xc, _yc, sample: true, allowParallel: true);

    [Benchmark]
    public double CorrelationDoubleParallel() => Covariance.Correlation(_x, _y, allowParallel: true);
}

