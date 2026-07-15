namespace BAVCL.Tests.Helpers;

public abstract class GpuTestBase : IClassFixture<GpuTestFixture>
{
    protected GPU Gpu { get; }

    protected GpuTestBase(GpuTestFixture fixture)
    {
        Gpu = fixture.Gpu;
    }

    protected Vector CreateVector(float[] values, int columns = 1, bool cache = true) =>
        new(Gpu, values, columns, cache);

    protected static float[] SyncValues(Vector vector)
    {
        vector.SyncCPU();
        return vector.Value;
    }
}
