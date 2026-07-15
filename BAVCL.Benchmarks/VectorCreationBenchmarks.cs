using BenchmarkDotNet.Attributes;

namespace BAVCL.Benchmarks;

[MemoryDiagnoser]
[MinWarmupCount(1), MaxWarmupCount(2)]
[MinIterationCount(3), MaxIterationCount(5)]
public class VectorCreationBenchmarks
{
    private GPU _gpu = null!;
    private float[] _data1K = null!;
    private float[] _data10K = null!;
    private float[] _data100K = null!;

    [GlobalSetup]
    public void Setup()
    {
        _gpu = GPUManager.Default;
        _data1K = Enumerable.Range(0, 1_000).Select(i => (float)i).ToArray();
        _data10K = Enumerable.Range(0, 10_000).Select(i => (float)i).ToArray();
        _data100K = Enumerable.Range(0, 100_000).Select(i => (float)i).ToArray();
    }

    [Benchmark] public Vector CreateCached_1K() => new(_gpu, _data1K, cache: true);
    [Benchmark] public Vector CreateCached_10K() => new(_gpu, _data10K, cache: true);
    [Benchmark] public Vector CreateCached_100K() => new(_gpu, _data100K, cache: true);
    [Benchmark] public Vector CreateNonCached_1K() => new(_gpu, _data1K, cache: false);
    [Benchmark] public Vector CreateNonCached_10K() => new(_gpu, _data10K, cache: false);
    [Benchmark] public Vector Zeros_1K() => Vector.Zeros(_gpu, 1_000);
    [Benchmark] public Vector Zeros_10K() => Vector.Zeros(_gpu, 10_000);
    [Benchmark] public Vector Zeros_100K() => Vector.Zeros(_gpu, 100_000);
}
