namespace BAVCL.Tests.Helpers;

public abstract class GpuTestBase : IClassFixture<GpuTestFixture>
{
    protected GPU Gpu { get; }

    protected GpuTestBase(GpuTestFixture fixture)
    {
        Gpu = fixture.Gpu;
    }

    protected Vector CreateVector(float[] values, int columns = 0, bool cache = true) =>
        new(Gpu, values, columns, cache);

    protected static float[] SyncValues(Vector vector) => vector.ToArray();

    protected static void ShouldBeInSyncWithValues(Vector vector, float[] expected)
    {
        vector.Residence.Should().HaveFlag(Residence.Cpu);
        vector.Residence.Should().HaveFlag(Residence.Gpu);
        vector.Value.ShouldBeCloseTo(expected);
        vector.Pull().ShouldBeCloseTo(expected);
    }

    protected Mask CreateMask(bool[] values, int columns = 0, bool cache = true) =>
        new(Gpu, values, columns, cache);

    protected static bool[] SyncMaskBits(Mask mask) => mask.ToBoolArray();
}
