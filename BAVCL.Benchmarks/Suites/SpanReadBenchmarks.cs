namespace BAVCL.Benchmarks;

/// <summary>
/// Zero-copy span reads vs heap copy on GPU-resident vectors.
/// </summary>
public class SpanReadBenchmarks
{
	GPU _gpu = null!;
	Vector _vector = null!;

	[Params(BenchmarkSizes.Small, BenchmarkSizes.Typical, BenchmarkSizes.Large)]
	public int N { get; set; }

	[GlobalSetup]
	public void Setup()
	{
		_gpu = GPUManager.Default;
		float[] data = Enumerable.Range(0, N).Select(i => (float)i).ToArray();
		_vector = new Vector(_gpu, data, cache: true);
		_vector.Residence = Residence.Gpu;
	}

	[Benchmark] public float RetrieveReadOnlySpan() => Sum(_vector.RetrieveReadOnlySpan());
	[Benchmark] public float GetCpuReadOnlySpan() => Sum(_vector.GetCpuReadOnlySpan());
	[Benchmark] public float ToArray() => Sum(_vector.ToArray());

	static float Sum(ReadOnlySpan<float> data)
	{
		float total = 0f;
		for (int i = 0; i < data.Length; i++)
			total += data[i];
		return total;
	}

	static float Sum(float[] data)
	{
		float total = 0f;
		for (int i = 0; i < data.Length; i++)
			total += data[i];
		return total;
	}
}
