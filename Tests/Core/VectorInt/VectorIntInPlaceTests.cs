using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Core.VectorIntTests;

public class VectorIntInPlaceTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
    [Fact]
    public void IPOP_Broadcast_MatchesReference()
    {
        var shape = new[] { 2, 3 };
        var leftData = BroadcastReference.SequentialData(2, 3, 2, 1).Select(x => (int)x).ToArray();
        var rightData = BroadcastReference.SequentialData(1, 3, 1, 1).Select(x => (int)x).ToArray();

        var left = BavclShapeInt.Create(Gpu, shape, leftData);
        var right = BavclShapeInt.Create(Gpu, [1, 3], rightData);
        var expected = BavclShapeInt.ExpectedBinary(leftData, shape, rightData, [1, 3], (x, y) => x + y);

        left.IPOP(right, Operations.add);
        left.SyncCPU();

        left.Value.Should().Equal(expected);
    }

    [Fact]
    public void IPOP_ScalarDivide_MutatesInPlace()
    {
        var vector = CreateVectorInt([10, 20, 30]);

        vector /= 10;
        vector.SyncCPU();

        vector.Value.Should().Equal([1, 2, 3]);
    }
}
