using BAVCL.Modules.Masking;
using BAVCL.Tests.Helpers;
using BAVCL.Types;

namespace BAVCL.Tests.Core.Masking;

[Collection("GpuSerial")]
[Trait("Category", "AB")]
public class MaskCompactionAbTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
	[Fact]
	public void FilterX_MatchesReference_1D()
	{
		Vector vector = CreateVector([10f, 20f, 30f, 40f]);
		Mask mask = CreateMask([true, false, true, false]);

		float[] expected = SyncValues(MaskCompactionReference.FilterX(vector, mask));
		float[] actual = SyncValues(MaskCompaction.FilterX(vector, mask));

		actual.ShouldBeCloseTo(expected);
	}

	[Fact]
	public void PartitionX_MatchesReference_1D()
	{
		Vector vector = CreateVector([1f, 2f, 3f, 4f]);
		Mask mask = CreateMask([true, false, true, false]);

		(Vector expectedTrue, Vector expectedFalse) = MaskCompactionReference.PartitionX(vector, mask);
		(Vector actualTrue, Vector actualFalse) = MaskCompaction.PartitionX(vector, mask);

		SyncValues(actualTrue).ShouldBeCloseTo(SyncValues(expectedTrue));
		SyncValues(actualFalse).ShouldBeCloseTo(SyncValues(expectedFalse));
	}

	[Fact]
	public void WhereX_MatchesReference_Sparse()
	{
		Mask mask = CreateMask([false, true, false, true, true]);

		int[] expected = SyncValues(MaskCompactionReference.Where(mask));
		int[] actual = SyncValues(MaskCompaction.WhereX(mask));

		actual.Should().Equal(expected);
	}

	[Fact]
	public void FilterX_MatchesReference_TailWord()
	{
		bool[] bits = [.. Enumerable.Range(0, 33).Select(i => i % 2 == 0)];
		Vector vector = CreateVector([.. Enumerable.Range(0, 33).Select(i => (float)i)]);
		Mask mask = CreateMask(bits);

		float[] expected = SyncValues(MaskCompactionReference.FilterX(vector, mask));
		float[] actual = SyncValues(MaskCompaction.FilterX(vector, mask));

		actual.ShouldBeCloseTo(expected);
	}

	[Fact]
	public void VectorInt_FilterX_MatchesReference()
	{
		VectorInt vector = CreateVectorInt([1, 2, 3, 4]);
		Mask mask = CreateMask([true, false, true, false]);

		int[] expected = SyncValues(MaskCompactionReference.FilterX(vector, mask));
		int[] actual = SyncValues(MaskCompaction.FilterX(vector, mask));

		actual.Should().Equal(expected);
	}

	[Fact]
	public void FilterCpu_MatchesLegacyCpu()
	{
		Vector vector = CreateVector([10f, 20f, 30f, 40f]);
		Mask mask = CreateMask([true, false, true, false]);

		float[] legacy = SyncValues(MaskCompactionReference.Filter(vector, mask, syncToGpu: false));
		float[] scan = SyncValues(MaskCompactionCpu.Filter(vector, mask, syncToGpu: false));

		scan.ShouldBeCloseTo(legacy);
	}

	[Fact]
	public void Filter_Operator_MatchesReference()
	{
		Vector vector = CreateVector([5f, 6f, 7f, 8f]);
		Mask mask = CreateMask([true, true, false, false]);

		float[] viaReference = SyncValues(MaskCompactionReference.FilterX(vector, mask));
		float[] viaOperator = SyncValues(vector | mask);

		viaOperator.ShouldBeCloseTo(viaReference);
	}
}
