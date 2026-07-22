using BenchmarkDotNet.Attributes;

namespace BAVCL.Benchmarks;

[MemoryDiagnoser]
[MinWarmupCount(1), MaxWarmupCount(2)]
[MinIterationCount(3), MaxIterationCount(5)]
public class VectorFactoryBenchmarks
{
    private GPU _gpu = null!;
    private Vector _left500 = null!;
    private Vector _right500 = null!;
    private Vector _left5K = null!;
    private Vector _right5K = null!;
    private Vector _left50K = null!;
    private Vector _right50K = null!;
    private Vector _appendTarget500 = null!;
    private Vector _appendTarget5K = null!;

    [GlobalSetup]
    public void Setup()
    {
        _gpu = GPUManager.Default;
        _left500 = new Vector(_gpu, Data(500), cache: true);
        _right500 = new Vector(_gpu, Data(500, 500), cache: true);
        _left5K = new Vector(_gpu, Data(5_000), cache: true);
        _right5K = new Vector(_gpu, Data(5_000, 5_000), cache: true);
        _left50K = new Vector(_gpu, Data(50_000), cache: true);
        _right50K = new Vector(_gpu, Data(50_000, 50_000), cache: true);
        _appendTarget500 = new Vector(_gpu, Data(500), cache: true);
        _appendTarget5K = new Vector(_gpu, Data(5_000), cache: true);
    }

    static float[] Data(int length, int offset = 0) =>
        Enumerable.Range(offset, length).Select(i => (float)i).ToArray();

    // --- Arange ---

    [Benchmark] public Vector Arange_1K() => Vector.Arange(_gpu, 0f, 1_000f, 1f);
    [Benchmark] public Vector Arange_10K() => Vector.Arange(_gpu, 0f, 10_000f, 1f);
    [Benchmark] public Vector Arange_100K() => Vector.Arange(_gpu, 0f, 100_000f, 1f);

    // --- Linspace ---

    [Benchmark] public Vector Linspace_1K() => Vector.Linspace(_gpu, 0f, 1f, 1_000);
    [Benchmark] public Vector Linspace_10K() => Vector.Linspace(_gpu, 0f, 1f, 10_000);
    [Benchmark] public Vector Linspace_100K() => Vector.Linspace(_gpu, 0f, 1f, 100_000);

    // --- Fill / Ones ---

    [Benchmark] public Vector Fill_1K() => Vector.Fill(_gpu, 3.14f, 1_000);
    [Benchmark] public Vector Fill_10K() => Vector.Fill(_gpu, 3.14f, 10_000);
    [Benchmark] public Vector Fill_100K() => Vector.Fill(_gpu, 3.14f, 100_000);

    [Benchmark] public Vector Ones_1K() => Vector.Ones(_gpu, 1_000);
    [Benchmark] public Vector Ones_10K() => Vector.Ones(_gpu, 10_000);
    [Benchmark] public Vector Ones_100K() => Vector.Ones(_gpu, 100_000);

    // --- Append (static allocates new vector) ---

    [Benchmark] public Vector Append_1K() => Vector.Append(_left500, _right500);
    [Benchmark] public Vector Append_10K() => Vector.Append(_left5K, _right5K);
    [Benchmark] public Vector Append_100K() => Vector.Append(_left50K, _right50K);

    // --- Append in-place ---

    [Benchmark] public Vector Append_IP_1K() => _appendTarget500.Append_IP(_right500);

    [Benchmark] public Vector Append_IP_10K() => _appendTarget5K.Append_IP(_right5K);

    [IterationSetup(Target = nameof(Append_IP_1K))]
    public void ResetAppendTarget500() => _appendTarget500 = new Vector(_gpu, Data(500), cache: true);

    [IterationSetup(Target = nameof(Append_IP_10K))]
    public void ResetAppendTarget5K() => _appendTarget5K = new Vector(_gpu, Data(5_000), cache: true);

    // --- Merge (unique union of elements) ---

    [Benchmark] public Vector Merge_1K() => Vector.Merge(_left500, _right500);
    [Benchmark] public Vector Merge_10K() => Vector.Merge(_left5K, _right5K);
}
