using BAVCL.Core.Helpers;
using BenchmarkDotNet.Attributes;

namespace BAVCL.Benchmarks;

/// <summary>Production CPU variance throughput (streaming CGL on SIMD blocks).</summary>
[MemoryDiagnoser]
public class VarStrategyBenchmark
{
	float[] _data = null!;

	[Params(BenchmarkSizes.Typical, BenchmarkSizes.Large)]
	public int N { get; set; }

	[GlobalSetup]
	public void Setup()
	{
		var rng = new Random(41);
		_data = new float[N];
		for (int i = 0; i < N; i++)
			_data[i] = 100_000f + (float)rng.NextDouble() * 5f;
	}

	[Benchmark]
	public float Production() => VarianceAccumOps.PopulationVariance(_data);
}
