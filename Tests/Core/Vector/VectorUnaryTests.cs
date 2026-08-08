using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Core.VectorTests;

public class VectorUnaryTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
    [Fact]
    public void Abs_CpuPath_ReturnsAbsoluteValues()
    {
        var vector = CreateVector(VectorFactory.MixedSigns);

        var result = vector.Abs();

        SyncValues(result).ShouldBeCloseTo(CpuReference.Abs(VectorFactory.MixedSigns));
    }

    [Fact]
    public void AbsX_GpuPath_MatchesAbs()
    {
        var vector = CreateVector(VectorFactory.MixedSigns);

        var cpu = SyncValues(vector.Abs());
        var gpu = SyncValues(vector.AbsX());

        cpu.ShouldBeCloseTo(gpu);
    }

    [Fact]
    public void Reverse_CpuPath_ReversesOrder()
    {
        var vector = CreateVector(VectorFactory.Small1D);

        var result = vector.Reverse();

        SyncValues(result).ShouldBeCloseTo(CpuReference.Reverse(VectorFactory.Small1D));
    }

    [Fact]
    public void ReverseX_GpuPath_MatchesReverse()
    {
        var vector = CreateVector(VectorFactory.Small1D);

        var cpu = SyncValues(vector.Reverse());
        var gpu = SyncValues(vector.ReverseX());

        cpu.ShouldBeCloseTo(gpu);
    }

    [Fact]
    public void Reciprocal_ReturnsOneOverValues()
    {
        var vector = CreateVector([2f, 4f, 5f]);

        var result = vector.ReciprocalX();

        SyncValues(result).ShouldBeCloseTo([0.5f, 0.25f, 0.2f]);
    }

    [Theory]
    [InlineData(4f, 0.5f)]
    [InlineData(9f, 1f / 3f)]
    [InlineData(16f, 0.25f)]
    public void Rsqrt_PositiveValues_ReturnReciprocalSqrt(float input, float expected)
    {
        var vector = CreateVector([input]);

        var result = vector.Rsqrt();

        SyncValues(result)[0].ShouldBeCloseTo(expected, 1e-3f);
    }

    [Fact]
    public void Rsqrt_Zero_ReturnsPositiveInfinity()
    {
        var vector = CreateVector([0f]);

        var result = vector.Rsqrt();

        float.IsPositiveInfinity(SyncValues(result)[0]).Should().BeTrue();
    }

    [Fact]
    public void Rsqrt_NegativeValue_ReturnsNaN()
    {
        var vector = CreateVector([-4f]);

        var result = vector.Rsqrt();

        float.IsNaN(SyncValues(result)[0]).Should().BeTrue();
    }

    [Fact]
    public void Rsqrt_MixedSigns_ComputesPerElement()
    {
        var vector = CreateVector([4f, 0f, -9f, 16f]);

        var result = vector.Rsqrt();
        var values = SyncValues(result);

        values[0].ShouldBeCloseTo(0.5f, 1e-3f);
        float.IsPositiveInfinity(values[1]).Should().BeTrue();
        float.IsNaN(values[2]).Should().BeTrue();
        values[3].ShouldBeCloseTo(0.25f, 1e-3f);
    }

    [Fact]
    public void RsqrtX_GpuPath_MatchesRsqrt()
    {
        var vector = CreateVector([4f, 0f, -9f, 16f]);

        var cpu = SyncValues(vector.Rsqrt());
        var gpu = SyncValues(vector.RsqrtX());

        cpu.ShouldBeCloseTo(gpu);
    }

    [Fact]
    public void Diff_ReturnsAdjacentDifferences()
    {
        var vector = CreateVector([1f, 3f, 6f, 10f]);

        var result = vector.DiffX();

        SyncValues(result).ShouldBeCloseTo([2f, 3f, 4f]);
    }

    [Fact]
    public void Nan_to_num_ReplacesNonFiniteValues()
    {
        var vector = CreateVector(VectorFactory.EdgeNaNInf);

        var result = vector.NanToNumX(-1f);

        SyncValues(result).ShouldBeCloseTo([-1f, -1f, -1f, 0f]);
    }

    [Fact]
    public void Log_IP_ComputesLogWithBase()
    {
        var vector = CreateVector([1f, 10f, 100f]);
        var copy = vector.Copy();

        copy.LogXIP(10f);

        SyncValues(copy).ShouldBeCloseTo([0f, 1f, 2f], tolerance: 1e-3f);
    }
}
