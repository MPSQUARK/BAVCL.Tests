using BAVCL.Modules.Masking;
using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Core.Masking;

[Collection("GpuSerial")]
public class MaskAggregatesTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
	[Fact]
	public void CountTrue_MatchesPopcount()
	{
		Mask mask = CreateMask([true, false, true, true, false, false, true]);

		mask.CountTrue().Should().Be(4);
	}

	[Fact]
	public void CountTrueX_MatchesCpu()
	{
		bool[] bits = [.. Enumerable.Range(0, 70).Select(i => i % 3 == 0)];
		Mask mask = CreateMask(bits);

		mask.CountTrueX().Should().Be(mask.CountTrue());
	}

	[Fact]
	public void Any_All_FullAndSparse()
	{
		Mask full = CreateMask([true, true, true]);
		full.Any().Should().BeTrue();
		full.All().Should().BeTrue();

		Mask sparse = CreateMask([false, false, true, false]);
		sparse.Any().Should().BeTrue();
		sparse.All().Should().BeFalse();
	}

	[Fact]
	public void AnyX_AllX_MatchCpu()
	{
		Mask mask = CreateMask([true, false, false, true]);

		mask.AnyX().Should().Be(mask.Any());
		mask.AllX().Should().Be(mask.All());
	}

	[Fact]
	public void FirstLastTrueIndex_RespectTailWord()
	{
		Mask mask = CreateMask([.. Enumerable.Range(0, 33).Select(i => i == 32)]);

		mask.FirstTrueIndex().Should().Be(32);
		mask.LastTrueIndex().Should().Be(32);
		mask.FirstTrueIndexX().Should().Be(32);
		mask.LastTrueIndexX().Should().Be(32);
	}

	[Fact]
	public void FirstTrueIndex_AllFalse_ReturnsMinusOne()
	{
		Mask mask = CreateMask([false, false, false]);

		mask.FirstTrueIndex().Should().Be(-1);
		mask.FirstTrueIndexX().Should().Be(-1);
	}
}
