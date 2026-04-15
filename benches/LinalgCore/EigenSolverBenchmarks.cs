using BenchmarkDotNet.Attributes;

namespace LAL.Benches;

[MemoryDiagnoser]
public class EigenSolverBenchmarks
{
    [Benchmark]
    public int Placeholder() => 1;
}

