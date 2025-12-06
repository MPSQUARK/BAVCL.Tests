using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace BAVCL.Tests.Benchmarks;

/// <summary>
/// Benchmarks for vector arithmetic operations (add, subtract, multiply)
/// Tests GPU performance vs theoretical CPU performance
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
public class VectorOperationsBenchmarks
{
    private GPU _gpu = null!;
    private Vector _vectorA_1K = null!;
    private Vector _vectorB_1K = null!;
    private Vector _vectorA_10K = null!;
    private Vector _vectorB_10K = null!;
    private Vector _vectorA_100K = null!;
    private Vector _vectorB_100K = null!;
    private Vector _vectorA_1M = null!;
    private Vector _vectorB_1M = null!;

    [GlobalSetup]
    public void Setup()
    {
        _gpu = new GPU();

        var data1K_A = Enumerable.Range(0, 1_000).Select(i => (float)i).ToArray();
        var data1K_B = Enumerable.Range(0, 1_000).Select(i => (float)(i * 2)).ToArray();
        _vectorA_1K = new Vector(_gpu, data1K_A, cache: true);
        _vectorB_1K = new Vector(_gpu, data1K_B, cache: true);

        var data10K_A = Enumerable.Range(0, 10_000).Select(i => (float)i).ToArray();
        var data10K_B = Enumerable.Range(0, 10_000).Select(i => (float)(i * 2)).ToArray();
        _vectorA_10K = new Vector(_gpu, data10K_A, cache: true);
        _vectorB_10K = new Vector(_gpu, data10K_B, cache: true);

        var data100K_A = Enumerable.Range(0, 100_000).Select(i => (float)i).ToArray();
        var data100K_B = Enumerable.Range(0, 100_000).Select(i => (float)(i * 2)).ToArray();
        _vectorA_100K = new Vector(_gpu, data100K_A, cache: true);
        _vectorB_100K = new Vector(_gpu, data100K_B, cache: true);

        var data1M_A = Enumerable.Range(0, 1_000_000).Select(i => (float)i).ToArray();
        var data1M_B = Enumerable.Range(0, 1_000_000).Select(i => (float)(i * 2)).ToArray();
        _vectorA_1M = new Vector(_gpu, data1M_A, cache: true);
        _vectorB_1M = new Vector(_gpu, data1M_B, cache: true);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        // GPU and Vector resources will be cleaned up automatically
    }

    #region Addition Benchmarks

    [Benchmark]
    public Vector VectorAddition_1K() => _vectorA_1K + _vectorB_1K;

    [Benchmark]
    public Vector VectorAddition_10K() => _vectorA_10K + _vectorB_10K;

    [Benchmark]
    public Vector VectorAddition_100K() => _vectorA_100K + _vectorB_100K;

    [Benchmark]
    public Vector VectorAddition_1M() => _vectorA_1M + _vectorB_1M;

    #endregion

    #region Subtraction Benchmarks

    [Benchmark]
    public Vector VectorSubtraction_1K() => _vectorA_1K - _vectorB_1K;

    [Benchmark]
    public Vector VectorSubtraction_10K() => _vectorA_10K - _vectorB_10K;

    [Benchmark]
    public Vector VectorSubtraction_100K() => _vectorA_100K - _vectorB_100K;

    [Benchmark]
    public Vector VectorSubtraction_1M() => _vectorA_1M - _vectorB_1M;

    #endregion

    #region Multiplication Benchmarks

    [Benchmark]
    public Vector VectorMultiplication_1K() => _vectorA_1K * _vectorB_1K;

    [Benchmark]
    public Vector VectorMultiplication_10K() => _vectorA_10K * _vectorB_10K;

    [Benchmark]
    public Vector VectorMultiplication_100K() => _vectorA_100K * _vectorB_100K;

    [Benchmark]
    public Vector VectorMultiplication_1M() => _vectorA_1M * _vectorB_1M;

    #endregion
}
