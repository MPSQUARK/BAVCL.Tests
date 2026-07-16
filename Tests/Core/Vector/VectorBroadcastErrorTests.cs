using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Core.VectorTests;

public class VectorBroadcastErrorTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
    [Theory]
    [MemberData(nameof(BroadcastTestData.IncompatibleBroadcastPairs), MemberType = typeof(BroadcastTestData))]
    public void Operator_IncompatibleShapes_Throws(
        int[] shapeA, float[] dataA, int[] shapeB, float[] dataB)
    {
        BroadcastReference.CanBroadcast(shapeA, shapeB).Should().BeFalse();

        var a = BavclShape.Create(Gpu, shapeA, dataA);
        var b = BavclShape.Create(Gpu, shapeB, dataB);

        Action act = () => _ = a + b;

        act.Should().Throw<IndexOutOfRangeException>()
            .WithMessage("*EQUAL length*");
    }

    [Theory]
    [MemberData(nameof(BroadcastTestData.IncompatibleBroadcastPairs), MemberType = typeof(BroadcastTestData))]
    public void OP_IncompatibleShapes_Throws(
        int[] shapeA, float[] dataA, int[] shapeB, float[] dataB)
    {
        var a = BavclShape.Create(Gpu, shapeA, dataA);
        var b = BavclShape.Create(Gpu, shapeB, dataB);

        Action act = () => _ = Vector.OP(a, b, Operations.add);

        act.Should().Throw<IndexOutOfRangeException>();
    }

    [Fact]
    public void IPOP_Incompatible2DShapes_Throws()
    {
        var a = BavclShape.Create(Gpu, [2, 3], BroadcastReference.SequentialData(2, 3));
        var b = BavclShape.Create(Gpu, [3, 2], BroadcastReference.SequentialData(3, 2));

        Action act = () => a.IPOP(b, Operations.add);

        act.Should().Throw<IndexOutOfRangeException>();
    }

    [Fact]
    public void IPOP_SmallerLeftOperandWithLargerBroadcastOutput_Throws()
    {
        var row = BavclShape.Create(Gpu, [1, 3], BroadcastReference.SequentialData(1, 3));
        var matrix = BavclShape.Create(Gpu, [2, 3], BroadcastReference.SequentialData(2, 3));

        Action act = () => row.IPOP(matrix, Operations.add);

        act.Should().Throw<IndexOutOfRangeException>();
    }
}
