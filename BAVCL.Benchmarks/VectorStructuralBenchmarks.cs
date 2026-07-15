using BenchmarkDotNet.Attributes;

namespace BAVCL.Benchmarks;

[MemoryDiagnoser]
[MinWarmupCount(1), MaxWarmupCount(2)]
[MinIterationCount(3), MaxIterationCount(5)]
public class VectorStructuralBenchmarks
{
    private GPU _gpu = null!;
    private Vector _matrix = null!;
    private Vector _vector = null!;

    [GlobalSetup]
    public void Setup()
    {
        _gpu = GPUManager.Default;
        _matrix = new Vector(_gpu, Enumerable.Range(0, 90).Select(i => (float)i).ToArray(), columns: 9, cache: true);
        _vector = new Vector(_gpu, Enumerable.Range(0, 10_000).Select(i => (float)i).ToArray(), cache: true);
    }

    [Benchmark] public Vector Transpose_Matrix() => Vector.Transpose(_matrix);
    [Benchmark] public float Dot_10K() => Vector.Dot(_vector, _vector);
    [Benchmark] public Vector Concat_1K() => Vector.Concat(_vector, _vector);
}
