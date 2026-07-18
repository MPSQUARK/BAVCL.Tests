using BenchmarkDotNet.Attributes;

namespace BAVCL.Benchmarks;

/// <summary>
/// UpdateCache hot path (same-length upload) at multiple sizes.
/// </summary>
[MemoryDiagnoser]
[MinWarmupCount(1), MaxWarmupCount(2)]
[MinIterationCount(3), MaxIterationCount(5)]
public class CacheUpdateBenchmarks
{
    private Vector _vector1K = null!;
    private Vector _vector10K = null!;
    private Vector _vector100K = null!;

    [GlobalSetup]
    public void Setup()
    {
        GPU gpu = GPUManager.Default;
        _vector1K = new Vector(gpu, Data(1_000), cache: true);
        _vector10K = new Vector(gpu, Data(10_000), cache: true);
        _vector100K = new Vector(gpu, Data(100_000), cache: true);
    }

    static float[] Data(int n) =>
        Enumerable.Range(0, n).Select(i => (float)i).ToArray();

    [Benchmark] public void UpdateCache_1K() => _vector1K.UpdateCache();
    [Benchmark] public void UpdateCache_10K() => _vector10K.UpdateCache();
    [Benchmark] public void UpdateCache_100K() => _vector100K.UpdateCache();
}
