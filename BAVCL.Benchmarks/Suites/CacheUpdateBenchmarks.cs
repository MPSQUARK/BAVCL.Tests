namespace BAVCL.Benchmarks;

/// <summary>
/// UpdateCache hot path (same-length upload).
/// </summary>
public class CacheUpdateBenchmarks
{
	Vector _vector = null!;

	[Params(BenchmarkSizes.Small, BenchmarkSizes.Typical, BenchmarkSizes.Large)]
	public int N { get; set; }

	[GlobalSetup]
	public void Setup()
	{
		GPU gpu = GPUManager.Default;
		_vector = new Vector(gpu, Data(N), cache: true);
	}

	static float[] Data(int n) =>
		Enumerable.Range(0, n).Select(i => (float)i).ToArray();

	[Benchmark] public void UpdateCache() => _vector.UpdateCache();
}
