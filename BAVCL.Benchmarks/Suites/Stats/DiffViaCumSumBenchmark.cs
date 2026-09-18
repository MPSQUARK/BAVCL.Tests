using BAVCL.Modules.Arithmetic;
using BAVCL.Modules.Scan;
using BenchmarkDotNet.Attributes;

namespace BAVCL.Benchmarks;

/// <summary>Optional experiment: dedicated <c>DiffX</c> vs host stencil on <c>CumSumX</c> output.</summary>
[MemoryDiagnoser]
public class DiffViaCumSumBenchmark
{
	static readonly GPU Gpu = GPUManager.Default;

	Vector _vector = null!;

	[Params(1_000, 100_000, 1_000_000)]
	public int N { get; set; }

	[GlobalSetup]
	public void Setup()
	{
		var rng = new Random(11);
		float[] data = new float[N];
		for (int i = 0; i < N; i++)
			data[i] = (float)rng.NextDouble() * 100f;

		_vector = new Vector(Gpu, data, cache: true);
	}

	[Benchmark(Baseline = true)]
	public Vector Diff_DedicatedKernel()
	{
		Vector copy = _vector.Copy();
		return copy.DiffX();
	}

	[Benchmark]
	public float[] Diff_ViaCumSumHostStencil()
	{
		Vector cum = _vector.CumSumX();
		float[] values = cum.ToArray();
		float[] diff = new float[values.Length - 1];
		for (int i = 1; i < values.Length; i++)
			diff[i - 1] = values[i] - values[i - 1];

		return diff;
	}
}
