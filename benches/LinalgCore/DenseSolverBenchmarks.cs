using BenchmarkDotNet.Attributes;

namespace LAL.Benches;

[MemoryDiagnoser]
public class DenseSolverBenchmarks
{
    [Benchmark]
    public int Placeholder() => 1;
}

