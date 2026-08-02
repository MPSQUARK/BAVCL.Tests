namespace BAVCL.Benchmarks;

public class VectorStructuralBenchmarks
{
	GPU _gpu = null!;
	Vector _vector = null!;
	Vector _matrix = null!;

	[Params(BenchmarkSizes.Small, BenchmarkSizes.Typical, BenchmarkSizes.Large)]
	public int N { get; set; }

	[GlobalSetup]
	public void Setup()
	{
		_gpu = GPUManager.Default;
		_vector = new Vector(_gpu, Enumerable.Range(0, N).Select(i => (float)i).ToArray(), cache: true);

		int side = Math.Max(2, (int)Math.Sqrt(N));
		int matrixLength = side * side;
		_matrix = new Vector(_gpu, Enumerable.Range(0, matrixLength).Select(i => (float)i).ToArray(), columns: side, cache: true);
	}

	[Benchmark] public Vector Transpose() => Vector.Transpose(_matrix);
	[Benchmark] public float Dot() => Vector.Dot(_vector, _vector);
	[Benchmark] public Vector Concat() => Vector.Concat(_vector, _vector);
}
