using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Core.VectorTests;

public class VectorAllTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
    [Fact]
    public void All_ReturnsTrueWhenNoZeros()
    {
        var vector = CreateVector([1f, 2f, 3f]);

        vector.All().Should().BeTrue();
        vector.All().Should().BeTrue();
    }

    [Fact]
    public void All_ReturnsFalseWhenContainsZero()
    {
        var vector = CreateVector([1f, 0f, 3f]);

        vector.All().Should().BeFalse();
    }
}
