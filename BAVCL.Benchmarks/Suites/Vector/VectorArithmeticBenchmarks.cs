namespace BAVCL.Benchmarks;

public class VectorArithmeticBenchmarks
{
	GPU _gpu = null!;
	Vector _a = null!;
	Vector _b = null!;

	[Params(BenchmarkSizes.Small, BenchmarkSizes.Typical, BenchmarkSizes.Large)]
	public int N { get; set; }

	[GlobalSetup]
	public void Setup()
	{
		_gpu = GPUManager.Default;
		_a = new Vector(_gpu, Data(N), cache: true);
		_b = new Vector(_gpu, Data(N, 0.5f), cache: true);
	}

	static float[] Data(int n, float scale = 1f) =>
		Enumerable.Range(0, n).Select(i => (float)i * scale).ToArray();

	[Benchmark] public Vector Add() => _a + _b;
	[Benchmark] public Vector Sub() => _a - _b;
	[Benchmark] public Vector Mul() => _a * _b;
	[Benchmark] public Vector Div() => _a / _b;
}
