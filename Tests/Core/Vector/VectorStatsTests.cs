using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Core.VectorTests;

public class VectorStatsTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
    [Fact]
    public void Sum_ReturnsTotal()
    {
        var vector = CreateVector(VectorFactory.Small1D);

        vector.Sum().Should().Be(15f);
    }

    [Fact]
    public void Mean_ReturnsAverage()
    {
        var vector = CreateVector(VectorFactory.Small1D);

        vector.Mean().Should().Be(3f);
    }

    [Fact]
    public void MinMaxRange_ReturnCorrectValues()
    {
        var vector = CreateVector(VectorFactory.MixedSigns);

        vector.Min().Should().Be(-4f);
        vector.Max().Should().Be(5f);
        vector.Range().Should().Be(9f);
    }

    [Fact]
    public void VarAndStd_MatchExpectedForSimpleDataset()
    {
        var vector = CreateVector([2f, 4f, 4f, 4f, 5f, 5f, 7f, 9f]);

        vector.Mean().Should().Be(5f);
        vector.Var().ShouldBeCloseTo(4f, 1e-3f);
        vector.Std().ShouldBeCloseTo(2f, 1e-3f);
    }

    [Fact]
    public void Sum_LargeVector_UsesKahanSummation()
    {
        var values = Enumerable.Repeat(0.1f, 10_000).ToArray();
        var vector = CreateVector(values);

        vector.Sum().ShouldBeCloseTo(1000f, 1f);
    }
}
