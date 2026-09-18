using BAVCL.Modules.Masking;
using BenchmarkDotNet.Attributes;

namespace BAVCL.Benchmarks;

/// <summary>GPU legacy (device gather from pre-built indices) vs GPU scan compaction.</summary>
[MemoryDiagnoser]
public class MaskCompactionAbGpuBenchmarks
{
	static readonly GPU Gpu = GPUManager.Default;

	static MaskCompactionAbGpuBenchmarks()
	{
		KernelModuleLoader.Load<float>(Gpu, KernelWorkloads.Default);
		KernelModuleLoader.Load<int>(Gpu, KernelWorkloads.Default);
		KernelModuleLoader.Load<float>(Gpu, KernelWorkloads.Statistics);
		KernelModuleLoader.Load<int>(Gpu, KernelWorkloads.Statistics);
	}

	Vector _vector = null!;
	Mask _mask = null!;
	int[] _filterIndices = null!;
	int[] _trueIndices = null!;
	int[] _falseIndices = null!;

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

		_vector = new Vector(Gpu, data, cache: true);
		_mask = new Mask(Gpu, bits, cache: true);

		Shape vectorShape = _vector.Shape();
		Shape maskShape = _mask.Shape();
		Shape outputShape = vectorShape.BroadcastWith(maskShape);

		_filterIndices = MaskCompactionReference.SelectedSourceIndices(_mask, maskShape, vectorShape, outputShape, selected: true);
		_trueIndices = _filterIndices;
		_falseIndices = MaskCompactionReference.SelectedSourceIndices(_mask, maskShape, vectorShape, outputShape, selected: false);
	}

	[Benchmark(Baseline = true)]
	public Vector Filter_LegacyGpuGather() => MaskCompactionReference.GatherFilterX(_vector, _filterIndices);

	[Benchmark]
	public Vector Filter_ScanGpu() => MaskCompaction.FilterX(_vector, _mask);

	[Benchmark]
	public (Vector True, Vector False) Partition_LegacyGpuGather() =>
		MaskCompactionReference.GatherPartitionX(_vector, _trueIndices, _falseIndices);

	[Benchmark]
	public (Vector True, Vector False) Partition_ScanGpu() => MaskCompaction.PartitionX(_vector, _mask);

	[Benchmark]
	public VectorInt Where_ScanGpu() => MaskCompaction.WhereX(_mask);
}
