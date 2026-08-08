using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Core.VectorIntTests;

public class VectorIntCompareTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
    [Fact]
    public void VectorCompare_OperatorsAndMethods()
    {
        var vector = CreateVectorInt([1, 2, 3, 4]);
        var other = CreateVectorInt([0, 2, 5, 4]);

        SyncMaskBits(vector > other).Should().Equal([true, false, false, false]);
        SyncMaskBits(vector >= other).Should().Equal([true, true, false, true]);
        SyncMaskBits(vector < other).Should().Equal([false, false, true, false]);
        SyncMaskBits(vector <= other).Should().Equal([false, true, true, true]);
        SyncMaskBits(vector.CompareEqualsX(other)).Should().Equal([false, true, false, true]);
        SyncMaskBits(vector.CompareNotEqualsX(other)).Should().Equal([true, false, true, false]);
    }

    [Fact]
    public void VectorScalarCompare()
    {
        var vector = CreateVectorInt([0, 1, 2]);

        SyncMaskBits(vector > 1).Should().Equal([false, false, true]);
        SyncMaskBits(vector >= 1).Should().Equal([false, true, true]);
        SyncMaskBits(vector < 1).Should().Equal([true, false, false]);
        SyncMaskBits(vector <= 1).Should().Equal([true, true, false]);
        SyncMaskBits(vector.CompareEqualsX(1)).Should().Equal([false, true, false]);
    }
}
