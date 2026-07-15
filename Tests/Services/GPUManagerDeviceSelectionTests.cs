using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Services;

public class GPUManagerDeviceSelectionTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
    [Fact]
    public void Context_IsAvailableAfterInitialization()
    {
        GPUManager.Context.Should().NotBeNull();
        GPUManager.Context.Devices.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public void DefaultGpu_HasLoadedKernels()
    {
        var vector = CreateVector([1f, 2f, 3f]);
        var result = vector + vector;

        SyncValues(result).ShouldBeCloseTo([2f, 4f, 6f]);
    }
}
