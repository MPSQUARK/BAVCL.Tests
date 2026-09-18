namespace BAVCL.Benchmarks;

/// <summary>
/// Read-heavy stats (Sum / Var / Min) on GPU-resident vectors.
/// </summary>
public class StatsReadBenchmarks
{
	Vector _vector = null!;

	[Params(BenchmarkSizes.Small, BenchmarkSizes.Typical, BenchmarkSizes.Large)]
	public int N { get; set; }

	[GlobalSetup]
	public void Setup()
	{
		float[] data = Enumerable.Range(0, N).Select(i => (float)i).ToArray();
		_vector = new Vector(GPUManager.Default, data, cache: true);
		_vector.Residence = Residence.Gpu;
	}

	[Benchmark] public float Sum() => _vector.Sum();
	[Benchmark] public float Var() => _vector.Var();
	[Benchmark] public float Min() => _vector.Min();
}
