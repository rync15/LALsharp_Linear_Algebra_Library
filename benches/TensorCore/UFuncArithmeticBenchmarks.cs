using BenchmarkDotNet.Attributes;

namespace LAL.Benches;

[MemoryDiagnoser]
public class UFuncArithmeticBenchmarks
{
    [Benchmark]
    public int Placeholder() => 1;
}

