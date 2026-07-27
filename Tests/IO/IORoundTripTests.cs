using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.IOTests;

public class IORoundTripTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
    [Fact]
    public void WriteAndRead_Csv_RoundTripsValues()
    {
        var tempDir = TempFileHelper.CreateTempDirectory();
        try
        {
            var original = CreateVector([1f, 2f, 3f, 4f, 5f]);

            IO.WriteToFile(original, "vector", "csv", tempDir);

            var loaded = IO.CSV2Vector(Gpu, "vector", "csv", tempDir);

            SyncValues(loaded).ShouldBeCloseTo(SyncValues(original));
        }
        finally
        {
            TempFileHelper.Cleanup(tempDir);
        }
    }

    [Fact]
    public void WriteToFile_String_WritesContent()
    {
        var tempDir = TempFileHelper.CreateTempDirectory();
        try
        {
            IO.WriteToFile("hello world", "test", "txt", tempDir);

            var path = Path.Combine(tempDir, "saved_data", "test.txt");
            File.ReadAllText(path).Should().Be("hello world");
        }
        finally
        {
            TempFileHelper.Cleanup(tempDir);
        }
    }
}
