using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Core.GPU;

public class GPUMemoryTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
    [Fact]
    public void GetMemUsage_ReturnsParseableValue()
    {
        _ = CreateVector([1f, 2f, 3f]);

        long.Parse(Gpu.GetMemUsage()).Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public void CachedVector_HasNonZeroId()
    {
        var vector = CreateVector([1f, 2f, 3f, 4f, 5f]);

        vector.ID.Should().BeGreaterThan(0u);
    }
}
