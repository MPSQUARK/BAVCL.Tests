using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Core.VectorTests;

public class VectorFactoriesTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
    [Fact]
    public void Zeros_CreatesZeroFilledVector()
    {
        var vector = Zeros(Gpu, 5, 0);

        SyncValues(vector).Should().AllBeEquivalentTo(0f);
        vector.Length.Should().Be(5);
    }

    [Fact]
    public void Ones_CreatesOneFilledVector()
    {
        var vector = Ones(Gpu, 4);

        SyncValues(vector).Should().AllBeEquivalentTo(1f);
    }

    [Fact]
    public void Fill_CreatesConstantVector()
    {
        var vector = Fill(Gpu, 3.5f, 3, 0);

        SyncValues(vector).Should().AllBeEquivalentTo(3.5f);
    }

    [Fact]
    public void Arange_StaticArray_ReturnsExpectedSequence()
    {
        var values = Arange(0f, 4f, 1f);

        values.Should().HaveCount(4);
        values.ShouldBeCloseTo([0f, 1f, 2f, 3f]);
    }

    [Fact]
    public void Arange_NegativeRangeWithPositiveStep_AdjustsStep()
    {
        var values = Arange(0f, -4f, 1f);

        values.Should().HaveCount(4);
        values.ShouldBeCloseTo([0f, -1f, -2f, -3f]);
    }

    [Fact]
    public void Arange_OnGpu_CreatesVector()
    {
        var vector = Arange(Gpu, 2f, 8f, 2f);

        SyncValues(vector).ShouldBeCloseTo([2f, 4f, 6f]);
    }

    [Fact]
    public void Linspace_StaticArray_ReturnsEvenlySpacedValues()
    {
        var values = Linspace(0f, 10f, 5);

        values.Should().HaveCount(5);
        values[0].Should().Be(0f);
        values[^1].Should().Be(10f);
    }

    [Fact]
    public void Linspace_OnGpu_CreatesVector()
    {
        var vector = Linspace(Gpu, 0f, 4f, 3);

        SyncValues(vector).ShouldBeCloseTo([0f, 2f, 4f]);
    }
}
