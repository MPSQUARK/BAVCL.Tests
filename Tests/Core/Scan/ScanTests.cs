using BAVCL.Modules.Arithmetic;
using BAVCL.Modules.Scan;
using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Core.Scan;

[Collection("GpuSerial")]
public class ScanTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
	[Fact]
	public void CumSumIP_1D_MatchesReference()
	{
		float[] data = [3f, 1f, 4f, 1f, 5f];
		var vector = CreateVector(data);

		vector.CumSumIP();
		SyncValues(vector).ShouldBeCloseTo([3f, 4f, 8f, 9f, 14f]);
	}

	[Fact]
	public void CumSumExclusiveIP_1D_MatchesReference()
	{
		float[] data = [3f, 1f, 4f, 1f, 5f];
		var vector = CreateVector(data);

		vector.CumSumExclusiveIP();
		SyncValues(vector).ShouldBeCloseTo([0f, 3f, 4f, 8f, 9f]);
	}

	[Fact]
	public void CumProdIP_1D_MatchesReference()
	{
		float[] data = [2f, 3f, 4f];
		var vector = CreateVector(data);

		vector.CumProdIP();
		SyncValues(vector).ShouldBeCloseTo([2f, 6f, 24f]);
	}

	[Fact]
	public void CumSumXIP_1D_MatchesCpu()
	{
		float[] data = [1f, 2f, 3f, 4f, 5f, 6f, 7f];
		var cpu = CreateVector(data);
		var gpu = CreateVector(data);

		cpu.CumSumIP();
		gpu.CumSumXIP();

		SyncValues(gpu).ShouldBeCloseTo(SyncValues(cpu));
	}

	[Fact]
	public void CumSumExclusiveXIP_1D_MatchesCpu()
	{
		float[] data = [1f, 2f, 3f, 4f];
		var cpu = CreateVector(data);
		var gpu = CreateVector(data);

		cpu.CumSumExclusiveIP();
		gpu.CumSumExclusiveXIP();

		SyncValues(gpu).ShouldBeCloseTo(SyncValues(cpu));
	}

	[Fact]
	public void CumSum_2D_PerRow()
	{
		float[] data = [1f, 2f, 3f, 4f];
		var vector = CreateVector(data, columns: 2);

		vector.CumSumIP();
		SyncValues(vector).ShouldBeCloseTo([1f, 3f, 3f, 7f]);
	}

	[Fact]
	public void CumSumX_2D_PerRow_MatchesCpu()
	{
		float[] data = [1f, 2f, 10f, 20f];
		var cpu = CreateVector(data, columns: 2);
		var gpu = CreateVector(data, columns: 2);

		cpu.CumSumIP();
		gpu.CumSumXIP();

		SyncValues(gpu).ShouldBeCloseTo(SyncValues(cpu));
	}

	[Fact]
	public void VectorInt_CumSumXIP_MatchesCpu()
	{
		int[] data = [1, 2, 3, 4, 5];
		var cpu = CreateVectorInt(data);
		var gpu = CreateVectorInt(data);

		cpu.CumSumIP();
		gpu.CumSumXIP();

		SyncValues(gpu).Should().Equal(SyncValues(cpu));
	}

	[Fact]
	public void Diff_AfterCumSumX_RoundTrip()
	{
		float[] data = [3f, 1f, 4f, 1f, 5f];
		var original = CreateVector(data);
		var scanned = CreateVector(data);

		scanned.CumSumXIP();
		Vector diff = scanned.DiffX();

		diff.Length.Should().Be(data.Length - 1);
		SyncValues(diff).ShouldBeCloseTo(data[1..]);
	}
}
