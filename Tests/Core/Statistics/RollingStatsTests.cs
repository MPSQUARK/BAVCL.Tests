using BAVCL.Modules.Statistics;
using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Core.Statistics;

[Collection("GpuSerial")]
public class RollingStatsTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
	[Fact]
	public void RollingSum_K2_MatchesManual()
	{
		float[] data = [1f, 2f, 3f, 4f, 5f];
		var vector = CreateVector(data);

		float[] rolling = SyncValues(VectorStatistics.RollingSum(vector, window: 2));

		rolling.ShouldBeCloseTo([1f, 3f, 5f, 7f, 9f]);
	}

	[Fact]
	public void MovingAverage_K3_MatchesPartialEdges()
	{
		float[] data = [1f, 2f, 3f, 4f, 5f];
		var vector = CreateVector(data);

		float[] avg = SyncValues(VectorStatistics.MovingAverage(vector, window: 3));

		avg.ShouldBeCloseTo([1f, 1.5f, 2f, 3f, 4f]);
	}

	[Fact]
	public void RollingSumX_MatchesCpu()
	{
		float[] data = [1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f];
		var cpu = CreateVector(data);
		var gpu = CreateVector(data);

		float[] expected = SyncValues(VectorStatistics.RollingSum(cpu, window: 4));
		float[] actual = SyncValues(VectorStatistics.RollingSumX(gpu, window: 4));

		actual.ShouldBeCloseTo(expected);
	}

	[Fact]
	public void MovingAverageX_MatchesCpu()
	{
		float[] data = [1f, 2f, 3f, 4f, 5f];
		var cpu = CreateVector(data);
		var gpu = CreateVector(data);

		float[] expected = SyncValues(VectorStatistics.MovingAverage(cpu, window: 3));
		float[] actual = SyncValues(VectorStatistics.MovingAverageX(gpu, window: 3));

		actual.ShouldBeCloseTo(expected);
	}
}
