using BAVCL.Core;
using BenchmarkDotNet.Attributes;

namespace BAVCL.Benchmarks;

/// <summary>
/// Measures read-heavy stats (Sum / Var / Min) on GPU-resident vectors after span migration.
/// </summary>
[MemoryDiagnoser]
[MinWarmupCount(1), MaxWarmupCount(2)]
[MinIterationCount(3), MaxIterationCount(5)]
public class StatsReadBenchmarks
{
    private Vector _vector10K = null!;
    private Vector _vector100K = null!;

    [GlobalSetup]
    public void Setup()
    {
        _vector10K = CreateGpuResident(10_000);
        _vector100K = CreateGpuResident(100_000);
    }

    static Vector CreateGpuResident(int length)
    {
        float[] data = Enumerable.Range(0, length).Select(i => (float)i).ToArray();
        var vector = new Vector(GPUManager.Default, data, cache: true);
        vector.Residence = Residence.Gpu;
        return vector;
    }

    [Benchmark] public float Sum_10K() => _vector10K.Sum();
    [Benchmark] public float Var_10K() => _vector10K.Var();
    [Benchmark] public float Min_10K() => _vector10K.Min();

    [Benchmark] public float Sum_100K() => _vector100K.Sum();
    [Benchmark] public float Var_100K() => _vector100K.Var();
    [Benchmark] public float Min_100K() => _vector100K.Min();
}
