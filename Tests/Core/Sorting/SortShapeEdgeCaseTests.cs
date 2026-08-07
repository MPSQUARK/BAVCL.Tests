using BAVCL.Modules.Sorting;
using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Core.Sorting;

/// <summary>
/// Edge-case layout tests: flat (columns=0), column (columns=1), and large-N GPU paths.
/// </summary>
[Collection("GpuSerial")]
public class SortShapeEdgeCaseTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
	const int LargeN = 100_001;

	static readonly Random Rng = new(2026);

	[Fact]
	public void SortAscIP_ColumnVector_MatchesArraySort()
	{
		int[] data = RandomIntData(512);
		var vector = CreateVectorInt(data, columns: 1);
		var expected = (int[])data.Clone();
		Array.Sort(expected);

		vector.SortAscIP();

		SyncValues(vector).Should().Equal(expected);
	}

	[Fact]
	public void SortAscXIP_Flat1D_Large_MatchesCpu()
	{
		int[] data = RandomIntData(LargeN);
		var cpu = CreateVectorInt(data);
		var gpu = CreateVectorInt(data);

		cpu.SortAscIP();
		gpu.SortAscXIP();

		SyncValues(gpu).Should().Equal(SyncValues(cpu));
	}

	[Fact]
	public void SortAscXIP_ColumnVector_Large_MatchesCpu()
	{
		int[] data = RandomIntData(LargeN);
		var cpu = CreateVectorInt(data, columns: 1);
		var gpu = CreateVectorInt(data, columns: 1);

		cpu.SortAscIP();
		gpu.SortAscXIP();

		SyncValues(gpu).Should().Equal(SyncValues(cpu));
	}

	[Fact]
	public void SortDescXIP_Flat1D_Large_MatchesCpu_Float()
	{
		float[] data = RandomFloatData(LargeN);
		var cpu = CreateVector(data);
		var gpu = CreateVector(data);

		cpu.SortDescIP();
		gpu.SortDescXIP();

		SyncValues(gpu).ShouldBeCloseTo(SyncValues(cpu));
	}

	[Fact]
	public void SortDescXIP_ColumnVector_Large_MatchesCpu_Float()
	{
		float[] data = RandomFloatData(LargeN);
		var cpu = CreateVector(data, columns: 1);
		var gpu = CreateVector(data, columns: 1);

		cpu.SortDescIP();
		gpu.SortDescXIP();

		SyncValues(gpu).ShouldBeCloseTo(SyncValues(cpu));
	}

	[Fact]
	public void ArgsortAscX_Flat1D_Large_MatchesCpu()
	{
		int[] data = RandomIntData(LargeN);
		var cpu = CreateVectorInt(data);
		var gpu = CreateVectorInt(data);

		int[] expected = SyncValues(cpu.ArgsortAsc());
		int[] actual = SyncValues(gpu.ArgsortAscX());

		ArgsortTestsHelper.AssertGlobalArgsort(data, actual, SortOrder.Ascending);
		ArgsortTestsHelper.AssertGlobalArgsort(data, expected, SortOrder.Ascending);
	}

	[Fact]
	public void ArgsortAscX_ColumnVector_Large_MatchesCpu()
	{
		int[] data = RandomIntData(LargeN);
		var cpu = CreateVectorInt(data, columns: 1);
		var gpu = CreateVectorInt(data, columns: 1);

		int[] expected = SyncValues(cpu.ArgsortAsc());
		int[] actual = SyncValues(gpu.ArgsortAscX());

		ArgsortTestsHelper.AssertGlobalArgsort(data, actual, SortOrder.Ascending);
		ArgsortTestsHelper.AssertGlobalArgsort(data, expected, SortOrder.Ascending);
	}

	[Fact]
	public void ArgsortDescX_Flat1D_Large_MatchesCpu_Float()
	{
		float[] data = RandomFloatData(LargeN);
		var cpu = CreateVector(data);
		var gpu = CreateVector(data);

		int[] expected = SyncValues(cpu.ArgsortDesc());
		int[] actual = SyncValues(gpu.ArgsortDescX());

		ArgsortTestsHelper.AssertGlobalArgsort(data, actual, SortOrder.Descending);
		ArgsortTestsHelper.AssertGlobalArgsort(data, expected, SortOrder.Descending);
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

/// <summary>Shared argsort assertions for tests in this folder.</summary>
internal static class ArgsortTestsHelper
{
	internal static void AssertGlobalArgsort<T>(T[] values, int[] indices, SortOrder order)
		where T : IComparable<T>
	{
		for (int i = 0; i < values.Length - 1; i++)
		{
			int cmp = values[indices[i]].CompareTo(values[indices[i + 1]]);
			if (order == SortOrder.Descending)
				cmp.Should().BeGreaterThanOrEqualTo(0);
			else
				cmp.Should().BeLessThanOrEqualTo(0);
		}
	}
}
