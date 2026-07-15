using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Core.VectorBase;

public class VectorBaseOutputTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
    [Fact]
    public void ToCSV_1D_IncludesCommaSeparatedValues()
    {
        var vector = CreateVector([1f, 2f, 3f]);

        var csv = vector.ToCSV();

        csv.Should().Contain("1,");
        csv.Should().Contain("2,");
        csv.Should().Contain("3,");
    }

    [Fact]
    public void ToStr_ReturnsFormattedString()
    {
        var vector = CreateVector([1f, 2f, 3f]);

        vector.ToStr().Should().Contain("1");
    }
}
