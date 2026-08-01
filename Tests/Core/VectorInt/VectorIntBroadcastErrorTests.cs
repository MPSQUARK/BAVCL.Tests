using BAVCL.Core.Exceptions;
using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Core.VectorIntTests;

public class VectorIntBroadcastErrorTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
    [Theory]
    [MemberData(nameof(BroadcastTestData.IncompatibleBroadcastPairs), MemberType = typeof(BroadcastTestData))]
    public void Operator_IncompatibleShapes_Throws(
        int[] shapeA, float[] dataA, int[] shapeB, float[] dataB)
    {
        BroadcastReference.CanBroadcast(shapeA, shapeB).Should().BeFalse();

        int[] intA = dataA.Select(x => (int)x).ToArray();
        int[] intB = dataB.Select(x => (int)x).ToArray();
        var a = BavclShapeInt.Create(Gpu, shapeA, intA);
        var b = BavclShapeInt.Create(Gpu, shapeB, intB);

        Action act = () => _ = a + b;

        act.Should().Throw<ShapeMismatchException>();
    }

    [Fact]
    public void IPOP_Incompatible2DShapes_Throws()
    {
        var a = BavclShapeInt.Create(Gpu, [2, 3], [1, 2, 3, 4, 5, 6]);
        var b = BavclShapeInt.Create(Gpu, [3, 2], [1, 2, 3, 4, 5, 6]);

        Action act = () => a.IPOP(b, Operations.add);

        act.Should().Throw<ShapeMismatchException>();
    }
}
