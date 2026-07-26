using BAVCL.Core;
using BAVCL.Core.Exceptions;
using BAVCL.Modules.Masking;
using BAVCL.Tests.Helpers;
using BAVCL.Types;

namespace BAVCL.Tests.Core.Masking;

public class MaskOperationsTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
	[Fact]
	public void MaskBitwise_AndOrXor()
	{
		Mask a = CreateMask([true, false, true, false]);
		Mask b = CreateMask([true, true, false, false]);

		bool[] and = SyncMaskBits(a & b);
		bool[] or = SyncMaskBits(a | b);
		bool[] xor = SyncMaskBits(a ^ b);

		and.Should().Equal([true, false, false, false]);
		or.Should().Equal([true, true, true, false]);
		xor.Should().Equal([false, true, true, false]);
	}

	[Fact]
	public void MaskUnary_NotSetClear()
	{
		Mask mask = CreateMask([true, false, true]);

		SyncMaskBits(~mask).Should().Equal([false, true, false]);
		SyncMaskBits(+mask).Should().Equal([true, true, true]);
		SyncMaskBits(-mask).Should().Equal([false, false, false]);
	}

	[Fact]
	public void VectorCompare_OperatorsAndMethods()
	{
		Vector vector = CreateVector([1f, 2f, 3f, 4f]);
		Vector other = CreateVector([0f, 2f, 5f, 4f]);

		SyncMaskBits(vector > other).Should().Equal([true, false, false, false]);
		SyncMaskBits(vector >= other).Should().Equal([true, true, false, true]);
		SyncMaskBits(vector.CompareEquals(other)).Should().Equal([false, true, false, true]);
		SyncMaskBits(vector.CompareNotEquals(other)).Should().Equal([true, false, true, false]);
	}

	[Fact]
	public void VectorScalarCompare()
	{
		Vector vector = CreateVector([0.5f, 1.5f, 2.5f]);

		SyncMaskBits(vector > 1f).Should().Equal([false, true, true]);
		SyncMaskBits(vector.CompareEquals(1.5f)).Should().Equal([false, true, false]);
	}

	[Fact]
	public void VectorCompare_NanEquals()
	{
		Vector a = CreateVector([float.NaN, 1f]);
		Vector b = CreateVector([float.NaN, 2f]);

		SyncMaskBits(a.CompareEquals(b)).Should().Equal([true, false]);
		SyncMaskBits(a > b).Should().Equal([false, false]);
	}

	[Fact]
	public void VectorFilter_AndOperator()
	{
		Vector vector = CreateVector([1f, 2f, 3f, 4f]);
		Mask mask = CreateMask([true, false, true, false]);

		float[] filtered = SyncValues(vector & mask);
		filtered.Should().Equal([1f, 0f, 3f, 0f]);

		float[] custom = SyncValues(vector.Filter(mask, -1f));
		custom.Should().Equal([1f, -1f, 3f, -1f]);
	}

	[Fact]
	public void VectorSelect_OperatorAndIndexer()
	{
		Vector vector = CreateVector([10f, 20f, 30f, 40f]);
		Mask mask = CreateMask([true, false, true, false]);

		float[] viaOp = SyncValues(vector << mask);
		float[] viaIndexer = SyncValues(vector[mask]);
		float[] viaMethod = SyncValues(vector.Select(mask));

		viaOp.Should().Equal([10f, 30f]);
		viaIndexer.Should().Equal([10f, 30f]);
		viaMethod.Should().Equal([10f, 30f]);
		viaOp.Should().HaveCount(2);
	}

	[Fact]
	public void VectorSelect_EmptyMask_ReturnsZeroLength()
	{
		Vector vector = CreateVector([1f, 2f, 3f]);
		Mask mask = CreateMask([false, false, false]);

		Vector selected = vector << mask;
		selected.Length.Should().Be(0);
		selected.Columns.Should().Be(0);
	}

	[Fact]
	public void MaskNand_MatchesComposed()
	{
		Mask a = CreateMask([true, true, false]);
		Mask b = CreateMask([true, false, true]);

		SyncMaskBits(a.Nand(b)).Should().Equal(SyncMaskBits(~(a & b)));
	}

	[Fact]
	public void MaskBitwise_Broadcast_1DTo2D()
	{
		Mask row = CreateMask([true, false], columns: 0);
		Mask grid = CreateMask([true, false, true, false], columns: 2);

		bool[] broadcastAnd = SyncMaskBits(row & grid);
		broadcastAnd.Should().Equal([true, false, true, false]);
	}

	[Fact]
	public void VectorFilter_Broadcast_1DMaskTo2DVector()
	{
		Vector vector = CreateVector([1f, 2f, 3f, 4f], columns: 2);
		Mask mask = CreateMask([true, false], columns: 0);

		float[] filtered = SyncValues(vector & mask);
		filtered.Should().Equal([1f, 0f, 3f, 0f]);
	}

	[Fact]
	public void MaskComplement_SpanningWords_LeavesPaddingLanesClear()
	{
		bool[] source = [.. Enumerable.Range(0, 70).Select(i => i % 3 == 0)];
		Mask mask = CreateMask(source);

		Mask complement = ~mask;
		SyncMaskBits(complement).Should().Equal([.. source.Select(bit => !bit)]);

		int paddingLanes = ~((1 << (70 & 31)) - 1);
		(complement.ToWordArray()[2] & paddingLanes).Should().Be(0);
	}

	[Fact]
	public void MaskInPlace_Broadcast_1DTo2D()
	{
		Mask grid = CreateMask([true, true, true, false, false, false], columns: 3);
		Mask row = CreateMask([true, false, true], columns: 3);

		grid &= row;

		SyncMaskBits(grid).Should().Equal([true, false, true, false, false, false]);
	}

	[Fact]
	public void MaskInPlace_ResizingLeftOperand_Throws()
	{
		Mask row = CreateMask([true, false, true], columns: 3);
		Mask grid = CreateMask([true, true, true, false, false, false], columns: 3);

		Action inPlace = () => Mask.IPOP(row, grid, MaskOperation.And);

		inPlace.Should().Throw<PerformanceException>();
	}
}
