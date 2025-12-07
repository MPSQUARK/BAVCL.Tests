namespace BAVCL.Tests.Benchmarks;

/// <summary>
/// Benchmarks for vector creation operations
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
public class VectorCreationBenchmarks
{
    private GPU _gpu = null!;
    private float[] _data1K = null!;
    private float[] _data10K = null!;
    private float[] _data100K = null!;
    private float[] _data1M = null!;

    [GlobalSetup]
    public void Setup()
    {
        _gpu = GPUManager.Default;
        _gpu.Should().NotBeNull();

        _data1K = Enumerable.Range(0, 1_000).Select(i => (float)i).ToArray();
        _data10K = Enumerable.Range(0, 10_000).Select(i => (float)i).ToArray();
        _data100K = Enumerable.Range(0, 100_000).Select(i => (float)i).ToArray();
        _data1M = Enumerable.Range(0, 1_000_000).Select(i => (float)i).ToArray();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        // GPU and Vector resources will be cleaned up automatically
    }

    [Benchmark]
    public Vector CreateCachedVector_1K() => new(_gpu, _data1K, cache: true);

    [Benchmark]
    public Vector CreateCachedVector_10K() => new(_gpu, _data10K, cache: true);

    [Benchmark]
    public Vector CreateCachedVector_100K() => new(_gpu, _data100K, cache: true);

    [Benchmark]
    public Vector CreateCachedVector_1M() => new(_gpu, _data1M, cache: true);

    [Benchmark]
    public Vector CreateNonCachedVector_1K() => new(_gpu, _data1K, cache: false);

    [Benchmark]
    public Vector CreateNonCachedVector_10K() => new(_gpu, _data10K, cache: false);
    [Benchmark]
    public Vector CreateNonCachedVector_100K() => new(_gpu, _data100K, cache: false);

    [Benchmark]
    public Vector CreateNonCachedVector_1M() => new(_gpu, _data1M, cache: false);

    [Benchmark]
    public Vector CreateZerosVector_1K() => new(_gpu, 1_000);

    [Benchmark]
    public Vector CreateZerosVector_10K() => new(_gpu, 10_000);

    [Benchmark]
    public Vector CreateZerosVector_100K() => new(_gpu, 100_000);

    [Benchmark]
    public Vector CreateZerosVector_1M() => new(_gpu, 1_000_000);
}
