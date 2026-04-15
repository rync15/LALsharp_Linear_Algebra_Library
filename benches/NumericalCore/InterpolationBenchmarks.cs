using BenchmarkDotNet.Attributes;
using LAL.NumericalCore.Interpolation;
using System.Numerics;

namespace LAL.Benches;

[MemoryDiagnoser]
public class InterpolationBenchmarks
{
    private double[] _centers = [];
    private double[] _values = [];
    private double[] _weights = [];
    private float[] _centersF = [];
    private float[] _valuesF = [];
    private float[] _weightsF = [];
    private Complex[] _complexValues = [];
    private Complex[] _complexWeights = [];

    [Params(64, 128)]
    public int Size { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _centers = new double[Size];
        _values = new double[Size];
        _weights = new double[Size];
        _centersF = new float[Size];
        _valuesF = new float[Size];
        _weightsF = new float[Size];
        _complexValues = new Complex[Size];
        _complexWeights = new Complex[Size];

        for (int i = 0; i < Size; i++)
        {
            double x = i / 16.0;
            _centers[i] = x;
            _values[i] = Math.Sin(x);
            _centersF[i] = (float)x;
            _valuesF[i] = MathF.Sin((float)x);
            _complexValues[i] = new Complex(Math.Cos(x), Math.Sin(x));
        }
    }

    [Benchmark]
    public bool RbfWeightsSequential() => Rbf.ComputeGaussianWeights(_centers, _values, epsilon: 0.85, _weights, allowParallel: false);

    [Benchmark]
    public bool RbfWeightsParallel() => Rbf.ComputeGaussianWeights(_centers, _values, epsilon: 0.85, _weights, allowParallel: true);

    [Benchmark]
    public bool RbfWeightsFloatSequential() => Rbf.ComputeGaussianWeights(_centersF, _valuesF, epsilon: 0.85f, _weightsF, allowParallel: false);

    [Benchmark]
    public bool RbfWeightsFloatParallel() => Rbf.ComputeGaussianWeights(_centersF, _valuesF, epsilon: 0.85f, _weightsF, allowParallel: true);

    [Benchmark]
    public bool RbfWeightsComplexParallel() => Rbf.ComputeGaussianWeights(_centers, _complexValues, epsilon: 0.85, _complexWeights, allowParallel: true);
}

