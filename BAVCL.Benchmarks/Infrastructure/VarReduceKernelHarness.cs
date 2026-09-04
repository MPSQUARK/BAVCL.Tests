using BAVCL.Core.Helpers;
using BAVCL.Core.Kernels;
using BAVCL.Core.Memory;
using ILGPU;
using ILGPU.Runtime;

namespace BAVCL.Benchmarks.Infrastructure;

/// <summary>
/// Loads variance group-reduce kernels directly on the accelerator for tests and benchmarks.
/// Does not touch the production <see cref="GPU"/> kernel delegates selected at module load.
/// </summary>
public static class VarReduceKernelHarness
{
	public enum Path
	{
		SharedScratch,
		WarpTree,
	}

	public static float VarFloat(Vector vector, Path path)
	{
		GPU gpu = vector.Gpu;
		(KernelConfig config, int numGroups) = ReduceLaunch(vector.Length);
		int partialLength = numGroups * 3;
		var kernel = LoadFloatKernel(gpu, path);
		using BufferEntity<float> partial = BufferPools.For(gpu).Float.Rent(partialLength);
		using (GpuScope.Begin(partial.PinTarget, vector))
		{
			kernel(
				gpu.DefaultStream,
				config,
				vector.GetBuffer().View,
				partial.View,
				vector.Length);
			gpu.Synchronize();
		}

		return FoldGroupedVariance(partial.ReadRentedSpan());
	}

	public static float VarInt(VectorInt vector, Path path)
	{
		GPU gpu = vector.Gpu;
		(KernelConfig config, int numGroups) = ReduceLaunch(vector.Length);
		int partialLength = numGroups * 3;
		var kernel = LoadIntKernel(gpu, path);
		using BufferEntity<float> partial = BufferPools.For(gpu).Float.Rent(partialLength);
		using (GpuScope.Begin(partial.PinTarget, vector))
		{
			kernel(
				gpu.DefaultStream,
				config,
				vector.GetBuffer().View,
				partial.View,
				vector.Length);
			gpu.Synchronize();
		}

		return FoldGroupedVariance(partial.ReadRentedSpan());
	}

	static Action<AcceleratorStream, KernelConfig, ArrayView<float>, ArrayView<float>, int> LoadFloatKernel(
		GPU gpu,
		Path path) =>
		path switch
		{
			Path.SharedScratch => gpu.accelerator.LoadKernel<
				ArrayView<float>, ArrayView<float>, int>(GPU.VarReduceFloatGroupedShared_Kern),
			Path.WarpTree => gpu.accelerator.LoadKernel<
				ArrayView<float>, ArrayView<float>, int>(GPU.VarReduceFloatGroupedWarpTree_Kern),
			_ => throw new ArgumentOutOfRangeException(nameof(path)),
		};

	static Action<AcceleratorStream, KernelConfig, ArrayView<int>, ArrayView<float>, int> LoadIntKernel(
		GPU gpu,
		Path path) =>
		path switch
		{
			Path.SharedScratch => gpu.accelerator.LoadKernel<
				ArrayView<int>, ArrayView<float>, int>(GPU.VarReduceIntGroupedShared_Kern),
			Path.WarpTree => gpu.accelerator.LoadKernel<
				ArrayView<int>, ArrayView<float>, int>(GPU.VarReduceIntGroupedWarpTree_Kern),
			_ => throw new ArgumentOutOfRangeException(nameof(path)),
		};

	static (KernelConfig config, int numGroups) ReduceLaunch(int length)
	{
		int numGroups = (length + KernelSharedMemory.GlobalReduceGroupSize - 1) / KernelSharedMemory.GlobalReduceGroupSize;
		return ((numGroups, KernelSharedMemory.GlobalReduceGroupSize), numGroups);
	}

	static float FoldGroupedVariance(ReadOnlySpan<float> triples)
	{
		int count = 0;
		float mean = 0f;
		float m2 = 0f;
		for (int i = 0; i + 2 < triples.Length; i += 3)
		{
			int groupCount = (int)triples[i];
			VarianceAccumOps.Merge(ref count, ref mean, ref m2, groupCount, triples[i + 1], triples[i + 2]);
		}

		return VarianceAccumOps.FromAccumulators(count, m2);
	}
}
