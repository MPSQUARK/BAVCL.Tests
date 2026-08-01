using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Core.VectorIntTests;

public class VectorIntStatsTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
    [Fact]
    public void VarAndStd_MatchExpectedForSimpleDataset()
    {
        var vector = CreateVectorInt([2, 4, 4, 4, 5, 5, 7, 9]);

        vector.Mean().Should().Be(5f);
        vector.Var().ShouldBeCloseTo(4f, 1e-3f);
        vector.Std().ShouldBeCloseTo(2f, 1e-3f);
    }

    [Fact]
    public void Linspace_OnGpu_CreatesVector()
    {
        var vector = VectorInt.Linspace(Gpu, 0, 10, 5);

        SyncValues(vector).Should().Equal([0, 2, 4, 6, 8]);
    }
}
