using BAVCL.Core;
using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Core.VectorTests;

public class VectorTransferTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
    [Fact]
    public void TransferBuffer_MovesGpuBufferToTarget()
    {
        var target = CreateVector([1f, 2f, 3f]);
        var temp = CreateVector([9f, 8f, 7f]);

        var originalId = temp.ID;
        target.TransferBuffer(temp);

        target.ID.Should().Be(originalId);
        temp.ID.Should().Be(0);
        target.Residence.Should().Be(Residence.InSync);
        temp.Residence.Should().Be(Residence.Cpu);
        SyncValues(target).ShouldBeCloseTo([9f, 8f, 7f]);
    }
}
