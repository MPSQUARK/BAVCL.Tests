using BAVCL.Core;
using BenchmarkDotNet.Attributes;

namespace BAVCL.Benchmarks;

/// <summary>
/// Compares zero-copy span reads vs heap copy on GPU-resident vectors.
/// Run on Release; absolute numbers are machine-dependent — use for relative comparison.
/// </summary>
[MemoryDiagnoser]
[MinWarmupCount(1), MaxWarmupCount(2)]
[MinIterationCount(3), MaxIterationCount(5)]
public class SpanReadBenchmarks
{
    private GPU _gpu = null!;
    private Vector _vector1K = null!;
    private Vector _vector100K = null!;
    private Vector _vector1M = null!;

    [GlobalSetup]
    public void Setup()
    {
        _gpu = GPUManager.Default;
        _vector1K = CreateGpuResident(1_000);
        _vector100K = CreateGpuResident(100_000);
        _vector1M = CreateGpuResident(1_000_000);
    }

    static Vector CreateGpuResident(int length)
    {
        float[] data = Enumerable.Range(0, length).Select(i => (float)i).ToArray();
        var vector = new Vector(GPUManager.Default, data, cache: true);
        vector.Residence = Residence.Gpu;
        return vector;
    }

    [Benchmark] public float RetrieveReadOnlySpan_1K() => Sum(_vector1K.RetrieveReadOnlySpan());
    [Benchmark] public float GetCpuReadOnlySpan_1K() => Sum(_vector1K.GetCpuReadOnlySpan());
    [Benchmark] public float ToArray_1K() => Sum(_vector1K.ToArray());

    [Benchmark] public float RetrieveReadOnlySpan_100K() => Sum(_vector100K.RetrieveReadOnlySpan());
    [Benchmark] public float GetCpuReadOnlySpan_100K() => Sum(_vector100K.GetCpuReadOnlySpan());
    [Benchmark] public float ToArray_100K() => Sum(_vector100K.ToArray());

    [Benchmark] public float RetrieveReadOnlySpan_1M() => Sum(_vector1M.RetrieveReadOnlySpan());
    [Benchmark] public float GetCpuReadOnlySpan_1M() => Sum(_vector1M.GetCpuReadOnlySpan());
    [Benchmark] public float ToArray_1M() => Sum(_vector1M.ToArray());

    static float Sum(ReadOnlySpan<float> data)
    {
        float total = 0f;
        for (int i = 0; i < data.Length; i++)
            total += data[i];
        return total;
    }

    static float Sum(float[] data)
    {
        float total = 0f;
        for (int i = 0; i < data.Length; i++)
            total += data[i];
        return total;
    }
}
