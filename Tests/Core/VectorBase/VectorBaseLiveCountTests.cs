using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Core.VectorBase;

public class VectorBaseLiveCountTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
    [Fact]
    public void IncrementAndDecrementLiveCount_TracksReferences()
    {
        var vector = CreateVector([1f, 2f, 3f]);

        vector.IncrementLiveCount();
        vector.LiveCount.Should().Be(1);

        vector.DecrementLiveCount();
        vector.LiveCount.Should().Be(0);
    }
}
