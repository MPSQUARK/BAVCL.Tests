using BAVCL.Core;
using BAVCL.Tests.Helpers;
namespace BAVCL.Tests.Core.VectorBase;

public class VectorBaseLifecycleTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
    [Fact]
    public void SyncCPU_RoundTripsGpuValues()
    {
        var vector = CreateVector([1f, 2f, 3f]);
        using (var scope = vector.CpuScope(syncOnDispose: true))
        {
            EditableView<float> view = scope.View;
            view[1] = 99f;
        }
        vector.SyncCPU();

        vector.Value[1].Should().Be(99f);
    }

    [Fact]
    public void Pull_ReturnsGpuValues()
    {
        var vector = CreateVector([4f, 5f, 6f]);

        var pulled = vector.Pull();

        pulled.ShouldBeCloseTo([4f, 5f, 6f]);
    }

    [Fact]
    public void ToArray_ReturnsCpuCopy()
    {
        var vector = CreateVector([7f, 8f, 9f]);

        vector.ToArray().ShouldBeCloseTo([7f, 8f, 9f]);
    }

    [Fact]
    public void UpdateCache_UpdatesGpuFromCpu()
    {
        var vector = CreateVector([1f, 2f, 3f]);
        vector.Value[0] = 100f;

        vector.UpdateCache();
        vector.SyncCPU();

        vector.Value[0].Should().Be(100f);
    }

    [Fact]
    public void DeCache_RemovesFromGpu()
    {
        var vector = CreateVector([1f, 2f, 3f]);

        vector.DeCache();

        vector.ID.Should().Be(0);
        vector.Value.ShouldBeCloseTo([1f, 2f, 3f]);
    }
}
