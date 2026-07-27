using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Core.VectorTests;

public class VectorSyncCoherenceTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
    [Fact]
    public void UpdateCache_AfterCpuResize_DoesNotShrinkValue()
    {
        var vector = CreateVector([1f, 2f, 3f]);
        vector.ID.Should().NotBe(0u);

        vector.Residence = Residence.Cpu;
        vector.Value = [1f, 2f, 3f, 4f, 5f, 6f];
        vector.Length = 6;

        vector.UpdateCache();

        vector.Length.Should().Be(6);
        vector.ToArray().ShouldBeCloseTo([1f, 2f, 3f, 4f, 5f, 6f]);
    }

    [Fact]
    public void Append_IP_PreservesValueAfterGpuCache()
    {
        var left = CreateVector([1f, 2f, 3f]);
        var right = CreateVector([4f, 5f, 6f]);

        left.Append_IP(right);

        left.Length.Should().Be(6);
        left.ToArray().ShouldBeCloseTo([1f, 2f, 3f, 4f, 5f, 6f]);
    }

    [Fact]
    public void UpdateCache_InSync_IsNoOpOnSecondCall()
    {
        var vector = CreateVector([1f, 2f, 3f]);
        uint idBefore = vector.ID;

        vector.UpdateCache();
        vector.UpdateCache();

        vector.ID.Should().Be(idBefore);
        ShouldBeInSyncWithValues(vector, [1f, 2f, 3f]);
    }

    [Fact]
    public void SyncCPU_InSync_IsNoOp()
    {
        var vector = CreateVector([1f, 2f, 3f]);
        vector.Residence = Residence.InSync;

        vector.SyncCPU();

        vector.Residence.Should().Be(Residence.InSync);
    }
}
