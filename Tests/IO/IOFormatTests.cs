using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.IOTests;

public class IOFormatTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
    [Fact]
    public void ToFileFormat_Txt_ReturnsToString()
    {
        var vector = CreateVector([1f, 2f, 3f]);

        var text = IO.ToFileFormat(vector, "txt");

        text.Should().Contain("1");
        text.Should().Contain("2");
    }

    [Fact]
    public void ToFileFormat_Csv_ReturnsCsvContent()
    {
        var vector = CreateVector([1f, 2f, 3f]);

        var csv = IO.ToFileFormat(vector, "csv");

        csv.Should().Contain("1,");
        csv.Should().Contain("2,");
    }

    [Fact]
    public void ToFileFormat_UnknownFormat_Throws()
    {
        var vector = CreateVector([1f, 2f, 3f]);

        var act = () => IO.ToFileFormat(vector, "xml");

        act.Should().Throw<Exception>().WithMessage("*No format*");
    }
}
