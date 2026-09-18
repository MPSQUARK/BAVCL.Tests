using BAVCL.Core.Helpers;
using BAVCL.Modules.Statistics;
using BenchmarkDotNet.Attributes;

namespace BAVCL.Benchmarks;

/// <summary>
/// A/B: legacy mask materialization vs grouped non-zero scan for <c>AllX</c>.
/// </summary>
[MemoryDiagnoser]
public class AllXBenchmark
{
	static readonly GPU Gpu = GPUManager.Default;

	Vector _vector = null!;
	VectorInt _intVector = null!;

	[Params(BenchmarkSizes.Typical, BenchmarkSizes.Large)]
	public int N { get; set; }

	[GlobalSetup]
	public void Setup()
	{
		KernelModuleLoader.Load<float>(Gpu, KernelWorkloads.Statistics);
		KernelModuleLoader.Load<float>(Gpu, KernelDomain.Mask);
		KernelModuleLoader.Load<int>(Gpu, KernelWorkloads.Statistics);
		KernelModuleLoader.Load<int>(Gpu, KernelDomain.Mask);

		var rng = new Random(17);
		float[] floatData = new float[N];
		int[] intData = new int[N];
		for (int i = 0; i < N; i++)
		{
			floatData[i] = rng.Next(1, 100);
			intData[i] = rng.Next(1, 100);
		}

		_vector = new Vector(Gpu, floatData, cache: true);
		_intVector = new VectorInt(Gpu, intData, cache: true);
	}

	[Benchmark(Baseline = true)]
	public bool FloatAllX_GroupedScan() => _vector.AllX();

	[Benchmark]
	public bool FloatAll_LegacyMaskMaterialization() => AllViaMask(_vector);

	[Benchmark]
	public bool IntAllX_GroupedScan() => _intVector.AllX();

	[Benchmark]
	public bool IntAll_LegacyMaskMaterialization() => AllViaMask(_intVector);

	static bool AllViaMask(Vector vector)
	{
		if (vector.Length == 0)
			return true;

		Mask mask = vector.CompareNotEqualsX(0f);
		return MaskBitOps.AreAllLogicalBitsSet(mask.ElementCount, mask.RetrieveReadOnlySpan());
	}

	static bool AllViaMask(VectorInt vector)
	{
		if (vector.Length == 0)
			return true;

		Mask mask = vector.CompareNotEqualsX(0);
		return MaskBitOps.AreAllLogicalBitsSet(mask.ElementCount, mask.RetrieveReadOnlySpan());
	}
}
