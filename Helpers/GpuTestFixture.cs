namespace BAVCL.Tests.Helpers;

public sealed class GpuTestFixture
{
    public GPU Gpu { get; } = GPUManager.Default;
}
