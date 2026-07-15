using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Core.VectorTests;

public class VectorUnaryTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
    [Fact]
    public void Abs_CpuPath_ReturnsAbsoluteValues()
    {
        var vector = CreateVector(VectorFactory.MixedSigns);

        var result = Vector.Abs(vector);

        SyncValues(result).ShouldBeCloseTo(CpuReference.Abs(VectorFactory.MixedSigns));
    }

    [Fact]
    public void AbsX_GpuPath_MatchesAbs()
    {
        var vector = CreateVector(VectorFactory.MixedSigns);

        var cpu = SyncValues(Vector.Abs(vector));
        var gpu = SyncValues(Vector.AbsX(vector));

        cpu.ShouldBeCloseTo(gpu);
    }

    [Fact]
    public void Reverse_CpuPath_ReversesOrder()
    {
        var vector = CreateVector(VectorFactory.Small1D);

        var result = Vector.Reverse(vector);

        SyncValues(result).ShouldBeCloseTo(CpuReference.Reverse(VectorFactory.Small1D));
    }

    [Fact]
    public void ReverseX_GpuPath_MatchesReverse()
    {
        var vector = CreateVector(VectorFactory.Small1D);

        var cpu = SyncValues(Vector.Reverse(vector));
        var gpu = SyncValues(Vector.ReverseX(vector));

        cpu.ShouldBeCloseTo(gpu);
    }

    [Fact]
    public void Reciprocal_ReturnsOneOverValues()
    {
        var vector = CreateVector([2f, 4f, 5f]);

        var result = Vector.Reciprocal(vector);

        SyncValues(result).ShouldBeCloseTo([0.5f, 0.25f, 0.2f]);
    }

    [Fact]
    public void Rsqrt_ReturnsReciprocalSqrt()
    {
        var vector = CreateVector([4f, 9f, 16f]);

        var result = Vector.Rsqrt(vector);

        SyncValues(result).ShouldBeCloseTo([0.5f, 1f / 3f, 0.25f], tolerance: 1e-3f);
    }

    [Fact]
    public void RsqrtX_GpuPath_MatchesRsqrt()
    {
        var vector = CreateVector([4f, 9f, 16f]);

        var cpu = SyncValues(Vector.Rsqrt(vector));
        var gpu = SyncValues(Vector.RsqrtX(vector));

        cpu.ShouldBeCloseTo(gpu);
    }

    [Fact]
    public void Diff_ReturnsAdjacentDifferences()
    {
        var vector = CreateVector([1f, 3f, 6f, 10f]);

        var result = Vector.Diff(vector);

        SyncValues(result).ShouldBeCloseTo([2f, 3f, 4f]);
    }

    [Fact]
    public void Nan_to_num_ReplacesNonFiniteValues()
    {
        var vector = CreateVector(VectorFactory.EdgeNaNInf);

        var result = Vector.Nan_to_num(vector, -1f);

        SyncValues(result).ShouldBeCloseTo([-1f, -1f, -1f, 0f]);
    }

    [Fact]
    public void Log_IP_ComputesLogWithBase()
    {
        var vector = CreateVector([1f, 10f, 100f]);
        var copy = vector.Copy();

        copy.Log_IP(10f);

        SyncValues(copy).ShouldBeCloseTo([0f, 1f, 2f], tolerance: 1e-3f);
    }
}
