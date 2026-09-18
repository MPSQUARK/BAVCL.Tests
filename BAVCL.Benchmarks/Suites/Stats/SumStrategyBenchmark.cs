using BAVCL.Core.Helpers;
using BenchmarkDotNet.Attributes;

namespace BAVCL.Benchmarks;

/// <summary>Production CPU sum throughput (widened SIMD Neumaier).</summary>
[MemoryDiagnoser]
public class SumStrategyBenchmark
{
	float[] _data = null!;

	[Params(BenchmarkSizes.Typical, BenchmarkSizes.Large)]
	public int N { get; set; }

	[GlobalSetup]
	public void Setup() => _data = Enumerable.Repeat(0.1f, N).ToArray();

	[Benchmark]
	public float Production() => CompensatedSumOps.SumNeumaier(_data);
}
