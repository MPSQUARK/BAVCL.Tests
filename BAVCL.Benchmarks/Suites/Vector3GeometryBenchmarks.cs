namespace BAVCL.Benchmarks;

public class Vector3GeometryBenchmarks
{
	GPU _gpu = null!;
	Vector3 _a = null!;
	Vector3 _b = null!;

	[Params(BenchmarkSizes.Small, BenchmarkSizes.Typical, BenchmarkSizes.Large)]
	public int N { get; set; }

	[GlobalSetup]
	public void Setup()
	{
		_gpu = GPUManager.Default;
		int length = N * 3;
		var values = Enumerable.Range(0, length).Select(i => (float)i).ToArray();
		_a = new Vector3(_gpu, values, cache: true);
		_b = new Vector3(_gpu, values.Select(v => v + 1f).ToArray(), cache: true);
	}

	[Benchmark] public Vector3 CrossX() => Vector3.CrossX(_a, _b);
	[Benchmark] public Vector MagnitudeX() => Vector3.MagnitudeX(_a);
	[Benchmark] public Vector DistanceX() => Vector3.DistanceX(_a, _b);
}
