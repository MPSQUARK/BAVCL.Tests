using System.Numerics;
using BAVCL.Modules.Sorting;
using ILGPU.Algorithms;
using ILGPU.Algorithms.RadixSortOperations;
using ILGPU.Runtime;

namespace BAVCL.Tests.Core.Sorting;

/// <summary>
/// Regression coverage for the GpuScope pin ordering in <c>SortAlgorithms</c>: every input/output/
/// scratch <see cref="BAVCL.Core.ICacheable"/> must be pinned before any later allocation in the same
/// call can trigger <see cref="BAVCL.Core.LRU.GC"/>. Runs against a deliberately tiny, isolated
/// <see cref="TightMemoryGpuFixture.Gpu"/> so each argsort call forces real evictions of the previous
/// iteration's now-unpinned leftovers, without ever starving the currently-pinned input/indices/scratch trio.
/// </summary>
[Collection("GpuSerial")]
public sealed class SortMemoryPressureTests(TightMemoryGpuFixture fixture) : IClassFixture<TightMemoryGpuFixture>
{
	const int VectorLength = 512;

	static readonly Random Rng = new(2027);

	readonly global::BAVCL.GPU _gpu = fixture.Gpu;

	[Fact]
	public void ArgsortAscX_1D_Int_SurvivesRepeatedEvictionPressure()
	{
		for (int iteration = 0; iteration < 4; iteration++)
			AssertIntArgsortRoundTrip(columns: 0);
	}

	[Fact]
	public void ArgsortDescX_2D_Int_SurvivesRepeatedEvictionPressure()
	{
		for (int iteration = 0; iteration < 4; iteration++)
			AssertIntArgsortRoundTrip(columns: 8);
	}

	[Fact]
	public void ArgsortAscX_1D_Float_SurvivesRepeatedEvictionPressure()
	{
		for (int iteration = 0; iteration < 4; iteration++)
			AssertFloatArgsortRoundTrip(columns: 0);
	}

	[Fact]
	public void ArgsortDescX_2D_Float_SurvivesRepeatedEvictionPressure()
	{
		for (int iteration = 0; iteration < 4; iteration++)
			AssertFloatArgsortRoundTrip(columns: 8);
	}

	void AssertIntArgsortRoundTrip(int columns)
	{
		int[] data = RandomIntData(VectorLength);
		var input = new VectorInt(_gpu, data, columns);
		int[] before = input.ToArray();

		VectorInt indices = columns == 0 ? input.ArgsortAscX() : input.ArgsortDescX();
		SortOrder order = columns == 0 ? SortOrder.Ascending : SortOrder.Descending;

		input.ToArray().Should().Equal(before);
		AssertSortedOrder(before, indices.ToArray(), columns, order);
	}

	void AssertFloatArgsortRoundTrip(int columns)
	{
		float[] data = RandomFloatData(VectorLength);
		var input = new Vector(_gpu, data, columns);
		float[] before = input.ToArray();

		VectorInt indices = columns == 0 ? input.ArgsortAscX() : input.ArgsortDescX();
		SortOrder order = columns == 0 ? SortOrder.Ascending : SortOrder.Descending;

		input.ToArray().Should().Equal(before);
		AssertSortedOrder(before, indices.ToArray(), columns, order);
	}

	static void AssertSortedOrder(int[] values, int[] indices, int columns, SortOrder order)
	{
		int segmentLength = columns == 0 ? values.Length : columns;
		int rowCount = columns == 0 ? 1 : values.Length / columns;

		for (int row = 0; row < rowCount; row++)
		{
			int offset = row * segmentLength;
			for (int i = 0; i < segmentLength - 1; i++)
			{
				int cmp = values[offset + indices[offset + i]].CompareTo(values[offset + indices[offset + i + 1]]);
				if (order == SortOrder.Descending)
					cmp.Should().BeGreaterThanOrEqualTo(0);
				else
					cmp.Should().BeLessThanOrEqualTo(0);
			}
		}
	}

	static void AssertSortedOrder(float[] values, int[] indices, int columns, SortOrder order)
	{
		int segmentLength = columns == 0 ? values.Length : columns;
		int rowCount = columns == 0 ? 1 : values.Length / columns;

		for (int row = 0; row < rowCount; row++)
		{
			int offset = row * segmentLength;
			for (int i = 0; i < segmentLength - 1; i++)
			{
				int cmp = values[offset + indices[offset + i]].CompareTo(values[offset + indices[offset + i + 1]]);
				if (order == SortOrder.Descending)
					cmp.Should().BeGreaterThanOrEqualTo(0);
				else
					cmp.Should().BeLessThanOrEqualTo(0);
			}
		}
	}

	static int[] RandomIntData(int length)
	{
		var data = new int[length];
		for (int i = 0; i < length; i++)
			data[i] = Rng.Next(-1_000_000, 1_000_000);
		return data;
	}

	static float[] RandomFloatData(int length)
	{
		var data = new float[length];
		for (int i = 0; i < length; i++)
			data[i] = (float)(Rng.NextDouble() * 2000.0 - 1000.0);
		return data;
	}
}

/// <summary>
/// A dedicated <see cref="global::BAVCL.GPU"/> instance, built once per test class run, whose memory
/// budget is tight enough to force real LRU evictions during argsort calls without ever starving the
/// currently-pinned input/indices/scratch objects.
/// </summary>
/// <remarks>
/// LRU budget matches peak concurrent <c>MemoryUsed</c> for these argsort paths (input + indices + pool
/// scratch), including 1D radix pairs temp sized via
/// <see cref="Accelerator.ComputeRadixSortPairsTempStorageSize{TKey,TValue,TOperation}"/>, plus one
/// <c>VectorLength</c> vector of slack so repeated iterations can evict prior leftovers.
/// </remarks>
public sealed class TightMemoryGpuFixture : IDisposable
{
	const int VectorLength = 512;

	public global::BAVCL.GPU Gpu { get; }

	public TightMemoryGpuFixture()
	{
		Accelerator accelerator = BAVCL.GPUManager.GetPreferedAccelerator(forceCPU: false);
		long tightBudgetBytes = ComputeTightArgsortBudgetBytes(accelerator, VectorLength);
		var memoryManager = new BAVCL.Core.LRU { AvailableMemory = tightBudgetBytes };
		Gpu = new global::BAVCL.GPU(accelerator, memoryManager);
		KernelModuleLoader.Load<float>(Gpu, KernelWorkloads.Sorting);
		KernelModuleLoader.Load<int>(Gpu, KernelWorkloads.Sorting);
	}

	public void Dispose() => Gpu.Dispose();

	/// <summary>Matches <see cref="ElementBufferPool{T}"/> capacity rounding and <c>SortAlgorithms</c> rents.</summary>
	static long ComputeTightArgsortBudgetBytes(Accelerator accelerator, int length)
	{
		int radixPairsElements = (int)accelerator.ComputeRadixSortPairsTempStorageSize<int, int, AscendingInt32>(length);

		long peak1DBytes =
			2L * length * sizeof(int) +
			PoolCapacity(length + radixPairsElements) * sizeof(int);

		long peak2DBytes =
			2L * length * sizeof(int) +
			PoolCapacity(length * 3) * sizeof(int);

		long peakBytes = Math.Max(peak1DBytes, peak2DBytes);
		return peakBytes + length * sizeof(int);
	}

	static int PoolCapacity(int length) =>
		(int)BitOperations.RoundUpToPowerOf2((uint)Math.Max(length, 1));
}
