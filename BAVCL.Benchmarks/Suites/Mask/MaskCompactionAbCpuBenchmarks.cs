using BAVCL.Modules.Masking;
using BenchmarkDotNet.Attributes;

namespace BAVCL.Benchmarks;

/// <summary>CPU legacy (index loop + gather) vs CPU scan compaction.</summary>
[MemoryDiagnoser]
public class MaskCompactionAbCpuBenchmarks
{
	Vector _vector = null!;
	Mask _mask = null!;

	[Params(1_000, 100_000, 10_000_000)]
	public int N { get; set; }

	[GlobalSetup]
	public void Setup()
	{
		var rng = new Random(17);
		float[] data = new float[N];
		bool[] bits = new bool[N];
		for (int i = 0; i < N; i++)
		{
			data[i] = (float)rng.NextDouble();
			bits[i] = rng.NextDouble() > 0.5;
		}

		_vector = new Vector(GPUManager.Default, data, cache: true);
		_mask = new Mask(GPUManager.Default, bits, cache: true);
	}

	[Benchmark(Baseline = true)]
	public Vector Filter_LegacyCpu() => MaskCompactionReference.Filter(_vector, _mask, syncToGpu: false);

	[Benchmark]
	public Vector Filter_ScanCpu() => MaskCompactionCpu.Filter(_vector, _mask, syncToGpu: false);

	[Benchmark]
	public (Vector True, Vector False) Partition_LegacyCpu() =>
		MaskCompactionReference.Partition(_vector, _mask, syncToGpu: false);

	[Benchmark]
	public (Vector True, Vector False) Partition_ScanCpu() =>
		MaskCompactionCpu.Partition(_vector, _mask, syncToGpu: false);
}
