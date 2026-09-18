namespace BAVCL.Benchmarks;

public class VectorCreationBenchmarks
{
	GPU _gpu = null!;
	float[] _data = null!;

	[Params(BenchmarkSizes.Small, BenchmarkSizes.Typical, BenchmarkSizes.Large)]
	public int N { get; set; }

	[GlobalSetup]
	public void Setup()
	{
		_gpu = GPUManager.Default;
		_data = Enumerable.Range(0, N).Select(i => (float)i).ToArray();
	}

	[Benchmark] public Vector CreateCached() => new(_gpu, _data, cache: true);
	[Benchmark] public Vector CreateNonCached() => new(_gpu, _data, cache: false);
	[Benchmark] public Vector Zeros() => Vector.Zeros(_gpu, N);
}
