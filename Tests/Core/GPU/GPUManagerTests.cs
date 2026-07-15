using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Core.GPU;

public class GPUManagerTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
    [Fact]
    public void Default_ReturnsInitializedGpu()
    {
        Gpu.Should().NotBeNull();
        GPUManager.IsInitialized.Should().BeTrue();
    }

    [Fact]
    public void GetGPU_WithInvalidMemoryCap_Throws()
    {
        var actLow = () => GPUManager.GetGPU(0f);
        var actHigh = () => GPUManager.GetGPU(1f);

        actLow.Should().Throw<Exception>().WithMessage("*Memory Cap*");
        actHigh.Should().Throw<Exception>().WithMessage("*Memory Cap*");
    }

    [Fact]
    public void GetGPU_ForceCpu_ReturnsCpuAccelerator()
    {
        using var cpuGpu = GPUManager.GetGPU(forceCPU: true);

        cpuGpu.accelerator.AcceleratorType.Should().Be(ILGPU.Runtime.AcceleratorType.CPU);
    }
}
