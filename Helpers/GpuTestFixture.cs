namespace BAVCL.Tests.Helpers;

public sealed class GpuTestFixture
{
    public GPU Gpu { get; } = GPUManager.Default;

    public GpuTestFixture()
    {
        KernelModuleLoader.Load<float>(Gpu, KernelDomain.Geometry);
        KernelModuleLoader.Load<int>(Gpu, KernelWorkloads.Default);
    }
}
