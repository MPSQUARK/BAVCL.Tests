using BenchmarkDotNet.Attributes;

namespace BAVCL.Benchmarks;

[MemoryDiagnoser]
[MinWarmupCount(1), MaxWarmupCount(2)]
[MinIterationCount(3), MaxIterationCount(5)]
public class VectorArithmeticBenchmarks
{
    private GPU _gpu = null!;
    private Vector _a1K = null!, _b1K = null!;
    private Vector _a10K = null!, _b10K = null!;
    private Vector _a100K = null!, _b100K = null!;

    [GlobalSetup]
    public void Setup()
    {
        _gpu = GPUManager.Default;
        _a1K = new Vector(_gpu, Data(1_000), cache: true);
        _b1K = new Vector(_gpu, Data(1_000, 0.5f), cache: true);
        _a10K = new Vector(_gpu, Data(10_000), cache: true);
        _b10K = new Vector(_gpu, Data(10_000, 0.5f), cache: true);
        _a100K = new Vector(_gpu, Data(100_000), cache: true);
        _b100K = new Vector(_gpu, Data(100_000, 0.5f), cache: true);
    }

    private static float[] Data(int n, float scale = 1f) =>
        Enumerable.Range(0, n).Select(i => (float)i * scale).ToArray();

    [Benchmark] public Vector Add_1K() => _a1K + _b1K;
    [Benchmark] public Vector Add_10K() => _a10K + _b10K;
    [Benchmark] public Vector Add_100K() => _a100K + _b100K;
    [Benchmark] public Vector Sub_1K() => _a1K - _b1K;
    [Benchmark] public Vector Mul_1K() => _a1K * _b1K;
    [Benchmark] public Vector Div_1K() => _a1K / _b1K;
}
