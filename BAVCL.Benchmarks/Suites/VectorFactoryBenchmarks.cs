namespace BAVCL.Benchmarks;

public class VectorFactoryBenchmarks
{
	GPU _gpu = null!;
	Vector _left = null!;
	Vector _right = null!;
	Vector _appendTarget = null!;

	[Params(BenchmarkSizes.Small, BenchmarkSizes.Typical, BenchmarkSizes.Large)]
	public int N { get; set; }

	[GlobalSetup]
	public void Setup()
	{
		_gpu = GPUManager.Default;
		int half = Math.Max(1, N / 2);
		_left = new Vector(_gpu, Data(half), cache: true);
		_right = new Vector(_gpu, Data(half, half), cache: true);
		_appendTarget = new Vector(_gpu, Data(half), cache: true);
	}

	static float[] Data(int length, int offset = 0) =>
		Enumerable.Range(offset, length).Select(i => (float)i).ToArray();

	[Benchmark] public Vector Arange() => Vector.Arange(_gpu, 0f, N, 1f);
	[Benchmark] public Vector Linspace() => Vector.Linspace(_gpu, 0f, 1f, N);
	[Benchmark] public Vector Fill() => Vector.Fill(_gpu, 3.14f, N);
	[Benchmark] public Vector Ones() => Vector.Ones(_gpu, N);
	[Benchmark] public Vector Append() => Vector.Append(_left, _right);

	[Benchmark]
	public Vector Append_IP() => _appendTarget.Append_IP(_right);

	[IterationSetup(Target = nameof(Append_IP))]
	public void ResetAppendTarget()
	{
		int half = Math.Max(1, N / 2);
		_appendTarget = new Vector(_gpu, Data(half), cache: true);
	}

	[Benchmark] public Vector Merge() => Vector.Merge(_left, _right);
}
