using BAVCL.Core.Exceptions;
using BAVCL.Modules.Statistics;
using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Modules.Statistics;

/// <summary>
/// Math correctness for CPU vs GPU statistics APIs on fixed edge-case inputs
/// (signs, zeros, unsorted order, non-finite values, GPU-resident buffers).
/// </summary>
[Collection("GpuSerial")]
public class StatisticsMathEdgeCaseTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
	const float OrderTolerance = 1e-5f;
	const float ReduceTolerance = 1e-5f;
	const float VarTolerance = 1e-5f;

	[Fact]
	public void MedianX_UnsortedGpuResident_MatchesCpu_KnownRegressionDataset()
	{
		float[] data = [1f, 324f, 312f, 5645f, 756f, 890f, 123f, 456f, 789f, 12f, 198f];
		var vector = CreateVector(data);
		PinToGpu(vector);

		vector.MedianX().ShouldBeCloseTo(324f, OrderTolerance);
		vector.MedianX().ShouldBeCloseTo(vector.Median(), OrderTolerance);
	}

	[Theory]
	[MemberData(nameof(FloatOrderStatEdgeCases))]
	public void OrderStatsX_OnGpu_MatchCpu(float[] data, string _)
	{
		var vector = CreateVector(data);
		PinToGpu(vector);

		vector.PercentileX(0f).ShouldBeCloseTo(vector.Percentile(0f), OrderTolerance);
		vector.PercentileX(50f).ShouldBeCloseTo(vector.Percentile(50f), OrderTolerance);
		vector.PercentileX(100f).ShouldBeCloseTo(vector.Percentile(100f), OrderTolerance);
		vector.MedianX().ShouldBeCloseTo(vector.Median(), OrderTolerance);
		vector.Quartile1X().ShouldBeCloseTo(vector.Quartile1(), OrderTolerance);
		vector.Quartile3X().ShouldBeCloseTo(vector.Quartile3(), OrderTolerance);
		vector.IqrX().ShouldBeCloseTo(vector.Iqr(), OrderTolerance);
	}

	[Theory]
	[MemberData(nameof(IntOrderStatEdgeCases))]
	public void VectorInt_OrderStatsX_OnGpu_MatchCpu(int[] data, string _)
	{
		var vector = CreateVectorInt(data);
		PinToGpu(vector);

		vector.MedianX().ShouldBeCloseTo(vector.Median(), OrderTolerance);
		vector.Quartile1X().ShouldBeCloseTo(vector.Quartile1(), OrderTolerance);
		vector.Quartile3X().ShouldBeCloseTo(vector.Quartile3(), OrderTolerance);
		vector.IqrX().ShouldBeCloseTo(vector.Iqr(), OrderTolerance);
	}

	[Theory]
	[MemberData(nameof(FloatDescriptiveEdgeCases))]
	public void DescriptiveStatsX_OnGpu_MatchCpu(float[] data, string _)
	{
		var vector = CreateVector(data);
		PinToGpu(vector);

		vector.SumX().ShouldBeCloseTo(vector.Sum(), ReduceTolerance);
		vector.MinX().ShouldBeCloseTo(vector.Min(), ReduceTolerance);
		vector.MaxX().ShouldBeCloseTo(vector.Max(), ReduceTolerance);
		vector.MinMaxX().Should().Be(vector.MinMax());
		vector.RangeX().ShouldBeCloseTo(vector.Range(), ReduceTolerance);
		vector.MeanX().ShouldBeCloseTo(vector.Mean(), ReduceTolerance);
		vector.VarX().ShouldBeCloseTo(vector.Var(), VarTolerance);
		vector.StdX().ShouldBeCloseTo(vector.Std(), VarTolerance);
		if (data.Length >= 2)
		{
			vector.SampleVarX().ShouldBeCloseTo(vector.SampleVar(), VarTolerance);
			vector.SampleStdX().ShouldBeCloseTo(vector.SampleStd(), VarTolerance);
		}

		vector.AllX().Should().Be(vector.All());
	}

	[Theory]
	[MemberData(nameof(IntDescriptiveEdgeCases))]
	public void VectorInt_DescriptiveStatsX_OnGpu_MatchCpu(int[] data, string _)
	{
		var vector = CreateVectorInt(data);
		PinToGpu(vector);

		vector.SumX().ShouldBeCloseTo(vector.Sum(), ReduceTolerance);
		vector.MinX().Should().Be(vector.Min());
		vector.MaxX().Should().Be(vector.Max());
		vector.MinMaxX().Should().Be(vector.MinMax());
		vector.RangeX().Should().Be(vector.Range());
		vector.MeanX().ShouldBeCloseTo(vector.Mean(), ReduceTolerance);
		vector.VarX().ShouldBeCloseTo(vector.Var(), VarTolerance);
		vector.StdX().ShouldBeCloseTo(vector.Std(), VarTolerance);
		if (data.Length >= 2)
		{
			vector.SampleVarX().ShouldBeCloseTo(vector.SampleVar(), VarTolerance);
			vector.SampleStdX().ShouldBeCloseTo(vector.SampleStd(), VarTolerance);
		}

		vector.AllX().Should().Be(vector.All());
	}

	[Fact]
	public void OrderStats_WithPositiveInfinity_MatchCpu()
	{
		float[] data = [1f, 2f, 3f, float.PositiveInfinity];
		var vector = CreateVector(data);
		PinToGpu(vector);

		vector.MinX().ShouldBeCloseTo(vector.Min(), ReduceTolerance);
		vector.MaxX().ShouldBeCloseTo(vector.Max(), ReduceTolerance);
		vector.MedianX().ShouldBeCloseTo(vector.Median(), OrderTolerance);
	}

	[Fact]
	public void OrderStats_WithNegativeInfinity_MatchCpu()
	{
		float[] data = [float.NegativeInfinity, -1f, 0f, 1f];
		var vector = CreateVector(data);
		PinToGpu(vector);

		vector.MinX().ShouldBeCloseTo(vector.Min(), ReduceTolerance);
		vector.MaxX().ShouldBeCloseTo(vector.Max(), ReduceTolerance);
		vector.MedianX().ShouldBeCloseTo(vector.Median(), OrderTolerance);
	}

	[Fact]
	public void OrderStats_WithNaN_MatchCpu()
	{
		float[] data = [1f, float.NaN, 3f, 4f];
		var vector = CreateVector(data);
		PinToGpu(vector);

		vector.MedianX().ShouldBeCloseTo(vector.Median(), OrderTolerance);
		vector.Quartile1X().ShouldBeCloseTo(vector.Quartile1(), OrderTolerance);
		vector.Quartile3X().ShouldBeCloseTo(vector.Quartile3(), OrderTolerance);
	}

	[Fact]
	public void DescriptiveStats_AllZeros_OnGpu_MatchCpu()
	{
		float[] data = [0f, 0f, 0f, 0f, 0f];
		var vector = CreateVector(data);
		PinToGpu(vector);

		vector.SumX().Should().Be(0f);
		vector.MeanX().Should().Be(0f);
		vector.VarX().ShouldBeCloseTo(0f, VarTolerance);
		vector.StdX().ShouldBeCloseTo(0f, VarTolerance);
		vector.SampleVarX().ShouldBeCloseTo(0f, VarTolerance);
		vector.SampleStdX().ShouldBeCloseTo(0f, VarTolerance);
		vector.AllX().Should().BeFalse();
		vector.All().Should().BeFalse();
	}

	[Theory]
	[InlineData(-1f)]
	[InlineData(101f)]
	[InlineData(float.NaN)]
	public void PercentileX_OutOfRange_ThrowsBeforeSort(float percentile)
	{
		var vector = CreateVector([3f, 1f, 2f]);
		PinToGpu(vector);

		var act = () => vector.PercentileX(percentile);
		act.Should().Throw<FixedRangeException>().Where(ex => ex.ParamName == "percentile");
	}

	static void PinToGpu(Vector vector) => vector.Residence = Residence.Gpu;

	static void PinToGpu(VectorInt vector) => vector.Residence = Residence.Gpu;

	public static IEnumerable<object[]> FloatOrderStatEdgeCases()
	{
		yield return [new float[] { 42f }, "single"];
		yield return [new float[] { 10f, 20f }, "two_even_median"];
		yield return [new float[] { -100f, -50f, -10f, -1f, -1f }, "all_negative"];
		yield return [new float[] { 0f, 0f, 0f, 0f, 0f }, "all_zero"];
		yield return [new float[] { -5f, 0f, 10f, 20f }, "mixed_signs_with_zero"];
		yield return [new float[] { 0f, 0f, 5f, 10f, 100f }, "mostly_positive_with_zeros"];
		yield return [new float[] { 1f, 2f, 3f, 4f, 5f }, "ascending_small"];
		yield return [new float[] { 5f, 4f, 3f, 2f, 1f }, "descending_unsorted"];
		yield return [new float[] { 1f, 324f, 312f, 5645f, 756f, 890f, 123f, 456f, 789f, 12f, 198f }, "regression_unsorted"];
	}

	public static IEnumerable<object[]> IntOrderStatEdgeCases()
	{
		yield return [new int[] { 7 }, "single"];
		yield return [new int[] { -10, -5, 0, 5, 10 }, "mixed_signs"];
		yield return [new int[] { 0, 0, 0, 0 }, "all_zero"];
		yield return [new int[] { 9, 1, 8, 2, 7, 3 }, "unsorted"];
	}

	public static IEnumerable<object[]> FloatDescriptiveEdgeCases()
	{
		yield return [new float[] { 42f }, "single"];
		yield return [new float[] { -100f, -50f, -10f, -1f, 1f }, "negatives_and_positive"];
		yield return [new float[] { 0f, 0f, 0f, 0f }, "all_zero"];
		yield return [new float[] { -5f, 0f, 10f, 20f }, "mixed_with_zero"];
		yield return [new float[] { 1f, 1f, 1f, 1f }, "constant"];
		yield return [new float[] { 100f, -100f, 50f, -50f, 0f }, "wide_spread"];
	}

	public static IEnumerable<object[]> IntDescriptiveEdgeCases()
	{
		yield return [new int[] { -3, -1, 0, 2, 4 }, "mixed_signs"];
		yield return [new int[] { 0, 0, 0 }, "all_zero"];
		yield return [new int[] { 5, 5, 5, 5 }, "constant"];
	}
}
