using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using LAL.LinalgCore;
using LAL.NumericalCore.RootFinding;
using System.Numerics;
using System.Threading.Tasks;

namespace LAL.BenchmarkDotNetSuite;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80, launchCount: 1, warmupCount: 1, iterationCount: 3)]
public class LinalgHotPathBenchmarks
{
    [Params(100_000, 1_000_000)]
    public int N { get; set; }

    [Params(1, 4)]
    public int ThreadCount { get; set; }

    private double[] _xDouble = Array.Empty<double>();
    private double[][] _yDoubleByThread = Array.Empty<double[]>();
    private float[] _xFloat = Array.Empty<float>();
    private float[][] _yFloatByThread = Array.Empty<float[]>();
    private Complex[] _xComplex = Array.Empty<Complex>();
    private Complex[] _yComplex = Array.Empty<Complex>();

    [GlobalSetup]
    public void Setup()
    {
        var rng = new Random(42);

        _xDouble = new double[N];
        _yDoubleByThread = new double[Math.Max(1, ThreadCount)][];

        _xFloat = new float[N];
        _yFloatByThread = new float[Math.Max(1, ThreadCount)][];

        _xComplex = new Complex[N];
        _yComplex = new Complex[N];

        for (int i = 0; i < N; i++)
        {
            _xDouble[i] = rng.NextDouble();
            _xFloat[i] = (float)rng.NextDouble();
            _xComplex[i] = new Complex(rng.NextDouble(), rng.NextDouble());
            _yComplex[i] = new Complex(rng.NextDouble(), rng.NextDouble());
        }

        for (int t = 0; t < _yDoubleByThread.Length; t++)
        {
            _yDoubleByThread[t] = new double[N];
            _yFloatByThread[t] = new float[N];
            for (int i = 0; i < N; i++)
            {
                _yDoubleByThread[t][i] = rng.NextDouble();
                _yFloatByThread[t][i] = (float)rng.NextDouble();
            }
        }
    }

    [Benchmark]
    public void AxpyDouble()
    {
        RunByThreads(thread => Axpy.Compute(0.5, _xDouble, _yDoubleByThread[thread]));
    }

    [Benchmark]
    public void AxpyFloat()
    {
        RunByThreads(thread => Axpy.Compute(0.5f, _xFloat, _yFloatByThread[thread]));
    }

    [Benchmark]
    public double DotuDouble()
    {
        if (ThreadCount <= 1)
        {
            return Dot.Dotu(_xDouble, _yDoubleByThread[0]);
        }

        double[] results = new double[ThreadCount];
        Parallel.For(0, ThreadCount, thread =>
        {
            results[thread] = Dot.Dotu(_xDouble, _yDoubleByThread[thread]);
        });

        double sum = 0.0;
        for (int i = 0; i < results.Length; i++)
        {
            sum += results[i];
        }

        return sum;
    }

    [Benchmark]
    public float DotuFloat()
    {
        if (ThreadCount <= 1)
        {
            return Dot.Dotu(_xFloat, _yFloatByThread[0]);
        }

        float[] results = new float[ThreadCount];
        Parallel.For(0, ThreadCount, thread =>
        {
            results[thread] = Dot.Dotu(_xFloat, _yFloatByThread[thread]);
        });

        float sum = 0.0f;
        for (int i = 0; i < results.Length; i++)
        {
            sum += results[i];
        }

        return sum;
    }

    [Benchmark]
    public Complex DotcComplex()
    {
        return Dot.Dotc(_xComplex, _yComplex);
    }

    [Benchmark]
    public SecantResult SecantSolve()
    {
        return Secant.Solve(static x => (x * x) - 2.0, 1.0, 2.0, tolerance: 1e-12, maxIterations: 100);
    }

    private void RunByThreads(Action<int> body)
    {
        if (ThreadCount <= 1)
        {
            body(0);
            return;
        }

        Parallel.For(0, ThreadCount, body);
    }
}
