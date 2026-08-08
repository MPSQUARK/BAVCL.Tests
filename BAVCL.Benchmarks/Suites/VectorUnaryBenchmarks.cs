namespace BAVCL.Benchmarks;

public class VectorUnaryBenchmarks
{
	GPU _gpu = null!;
	Vector _data = null!;

	[Params(BenchmarkSizes.Small, BenchmarkSizes.Typical, BenchmarkSizes.Large)]
	public int N { get; set; }

	[GlobalSetup]
	public void Setup()
	{
		_gpu = GPUManager.Default;
		var values = Enumerable.Range(0, N).Select(i => (float)(i % 100 - 50)).ToArray();
		_data = new Vector(_gpu, values, cache: true);
	}

	[Benchmark] public Vector AbsX() => Vector.AbsX(_data);
	[Benchmark] public Vector ReverseX() => Vector.ReverseX(_data);
	[Benchmark] public Vector DiffX() => Vector.DiffX(_data);
	[Benchmark] public Vector NanToNum() => Vector.NanToNumX(_data, 0f);
}
