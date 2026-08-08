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
		SyncMaskBits(vector.CompareEqualsX(other)).Should().Equal([false, true, false, true]);
		SyncMaskBits(vector.CompareNotEqualsX(other)).Should().Equal([true, false, true, false]);
	}

	[Fact]
	public void VectorScalarCompare()
	{
		Vector vector = CreateVector([0.5f, 1.5f, 2.5f]);

		SyncMaskBits(vector > 1f).Should().Equal([false, true, true]);
		SyncMaskBits(vector.CompareEqualsX(1.5f)).Should().Equal([false, true, false]);
	}

	[Fact]
	public void VectorCompare_NanEquals()
	{
		Vector a = CreateVector([float.NaN, 1f]);
		Vector b = CreateVector([float.NaN, 2f]);

		SyncMaskBits(a.CompareEqualsX(b)).Should().Equal([true, false]);
		SyncMaskBits(a > b).Should().Equal([false, false]);
	}

	[Fact]
	public void VectorMask_AndOperator()
	{
		Vector vector = CreateVector([1f, 2f, 3f, 4f]);
		Mask mask = CreateMask([true, false, true, false]);

		float[] masked = SyncValues(vector & mask);
		masked.Should().Equal([1f, 0f, 3f, 0f]);

		float[] custom = SyncValues(vector & (mask, -1f));
		custom.Should().Equal([1f, -1f, 3f, -1f]);
	}

	[Fact]
	public void VectorFilter_OperatorAndIndexer()
	{
		Vector vector = CreateVector([10f, 20f, 30f, 40f]);
		Mask mask = CreateMask([true, false, true, false]);

		float[] viaOp = SyncValues(vector | mask);
		float[] viaIndexer = SyncValues(vector[mask]);
		float[] viaMethod = SyncValues(vector.FilterX(mask));

		viaOp.Should().Equal([10f, 30f]);
		viaIndexer.Should().Equal([10f, 30f]);
		viaMethod.Should().Equal([10f, 30f]);
		viaOp.Should().HaveCount(2);
	}

	[Fact]
	public void VectorPartition_SplitsLanes()
	{
		Vector vector = CreateVector([1f, 2f, 3f, 4f]);
		Mask mask = CreateMask([true, false, true, false]);

		(Vector trueLanes, Vector falseLanes) = vector / mask;

		SyncValues(trueLanes).Should().Equal([1f, 3f]);
		SyncValues(falseLanes).Should().Equal([2f, 4f]);
	}

	[Fact]
	public void VectorPartition_ExtensionMethod_MatchesOperator()
	{
		Vector vector = CreateVector([1f, 2f, 3f, 4f]);
		Mask mask = CreateMask([true, false, true, false]);

		(Vector opTrue, Vector opFalse) = vector / mask;
		(Vector methodTrue, Vector methodFalse) = vector.PartitionX(mask);

		SyncValues(methodTrue).ShouldBeCloseTo(SyncValues(opTrue));
		SyncValues(methodFalse).ShouldBeCloseTo(SyncValues(opFalse));
	}

	[Fact]
	public void VectorPartition_AllTrue_ReturnsFullVectorAndEmptyFalseLanes()
	{
		Vector vector = CreateVector([1f, 2f, 3f]);
		Mask mask = CreateMask([true, true, true]);

		(Vector trueLanes, Vector falseLanes) = vector.PartitionX(mask);

		SyncValues(trueLanes).ShouldBeCloseTo([1f, 2f, 3f]);
		falseLanes.Length.Should().Be(0);
		falseLanes.Columns.Should().Be(0);
	}

	[Fact]
	public void VectorPartition_AllFalse_ReturnsEmptyTrueLanesAndFullFalseVector()
	{
		Vector vector = CreateVector([1f, 2f, 3f]);
		Mask mask = CreateMask([false, false, false]);

		(Vector trueLanes, Vector falseLanes) = vector.PartitionX(mask);

		trueLanes.Length.Should().Be(0);
		trueLanes.Columns.Should().Be(0);
		SyncValues(falseLanes).ShouldBeCloseTo([1f, 2f, 3f]);
	}

	[Fact]
	public void VectorPartition_2D_BroadcastMask_SplitsInRowMajorOrder()
	{
		Vector vector = CreateVector([1f, 2f, 3f, 4f], columns: 2);
		Mask mask = CreateMask([true, false], columns: 0);

		(Vector trueLanes, Vector falseLanes) = vector.PartitionX(mask);

		SyncValues(trueLanes).ShouldBeCloseTo([1f, 3f]);
		SyncValues(falseLanes).ShouldBeCloseTo([2f, 4f]);
	}

	[Fact]
	public void VectorFilter_EmptyMask_ReturnsZeroLength()
	{
		Vector vector = CreateVector([1f, 2f, 3f]);
		Mask mask = CreateMask([false, false, false]);

		Vector filtered = vector | mask;
		filtered.Length.Should().Be(0);
		filtered.Columns.Should().Be(0);
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
	public void VectorMask_Broadcast_1DMaskTo2DVector()
	{
		Vector vector = CreateVector([1f, 2f, 3f, 4f], columns: 2);
		Mask mask = CreateMask([true, false], columns: 0);

		float[] masked = SyncValues(vector & mask);
		masked.Should().Equal([1f, 0f, 3f, 0f]);
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

	[Fact]
	public void VectorIntCompare_OperatorsAndMethods()
	{
		VectorInt vector = CreateVectorInt([1, 2, 3, 4]);
		VectorInt other = CreateVectorInt([0, 2, 5, 4]);

		SyncMaskBits(vector > other).Should().Equal([true, false, false, false]);
		SyncMaskBits(vector >= other).Should().Equal([true, true, false, true]);
		SyncMaskBits(vector.CompareEqualsX(other)).Should().Equal([false, true, false, true]);
	}

	[Fact]
	public void VectorIntMask_AndOperator()
	{
		VectorInt vector = CreateVectorInt([1, 2, 3, 4]);
		Mask mask = CreateMask([true, false, true, false]);

		SyncValues(vector & mask).Should().Equal([1, 0, 3, 0]);
		SyncValues(vector & (mask, -1)).Should().Equal([1, -1, 3, -1]);
	}

	[Fact]
	public void VectorIntFilter_EmptyMask_ReturnsZeroLength()
	{
		VectorInt vector = CreateVectorInt([1, 2, 3]);
		Mask mask = CreateMask([false, false, false]);

		VectorInt filtered = vector | mask;
		filtered.Length.Should().Be(0);
		filtered.Columns.Should().Be(0);
	}

	[Fact]
	public void VectorIntMask_Broadcast_1DMaskTo2DVector()
	{
		VectorInt vector = CreateVectorInt([1, 2, 3, 4], columns: 2);
		Mask mask = CreateMask([true, false], columns: 0);

		SyncValues(vector & mask).Should().Equal([1, 0, 3, 0]);
	}

	[Fact]
	public void VectorIntPartition_SplitsLanes()
	{
		VectorInt vector = CreateVectorInt([1, 2, 3, 4]);
		Mask mask = CreateMask([true, false, true, false]);

		(VectorInt trueLanes, VectorInt falseLanes) = vector / mask;

		SyncValues(trueLanes).Should().Equal([1, 3]);
		SyncValues(falseLanes).Should().Equal([2, 4]);
	}

	[Fact]
	public void VectorIntPartition_ExtensionMethod_MatchesOperator()
	{
		VectorInt vector = CreateVectorInt([1, 2, 3, 4]);
		Mask mask = CreateMask([true, false, true, false]);

		(VectorInt opTrue, VectorInt opFalse) = vector / mask;
		(VectorInt methodTrue, VectorInt methodFalse) = vector.PartitionX(mask);

		SyncValues(methodTrue).Should().Equal(SyncValues(opTrue));
		SyncValues(methodFalse).Should().Equal(SyncValues(opFalse));
	}

	[Fact]
	public void VectorIntPartition_AllTrue_ReturnsFullVectorAndEmptyFalseLanes()
	{
		VectorInt vector = CreateVectorInt([1, 2, 3]);
		Mask mask = CreateMask([true, true, true]);

		(VectorInt trueLanes, VectorInt falseLanes) = vector.PartitionX(mask);

		SyncValues(trueLanes).Should().Equal([1, 2, 3]);
		falseLanes.Length.Should().Be(0);
		falseLanes.Columns.Should().Be(0);
	}

	[Fact]
	public void VectorIntPartition_AllFalse_ReturnsEmptyTrueLanesAndFullFalseVector()
	{
		VectorInt vector = CreateVectorInt([1, 2, 3]);
		Mask mask = CreateMask([false, false, false]);

		(VectorInt trueLanes, VectorInt falseLanes) = vector.PartitionX(mask);

		trueLanes.Length.Should().Be(0);
		trueLanes.Columns.Should().Be(0);
		SyncValues(falseLanes).Should().Equal([1, 2, 3]);
	}

	[Fact]
	public void VectorIntPartition_2D_BroadcastMask_SplitsInRowMajorOrder()
	{
		VectorInt vector = CreateVectorInt([1, 2, 3, 4], columns: 2);
		Mask mask = CreateMask([true, false], columns: 0);

		(VectorInt trueLanes, VectorInt falseLanes) = vector.PartitionX(mask);

		SyncValues(trueLanes).Should().Equal([1, 3]);
		SyncValues(falseLanes).Should().Equal([2, 4]);
	}
}
