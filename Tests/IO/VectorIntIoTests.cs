using BAVCL.Tests.Helpers;
using BAVCL.Types;

namespace BAVCL.Tests.IOTests;

/// <summary>
/// VectorInt round-trip tests across all formatters via the public IO API.
/// Run with: dotnet test -p:IncludeIOTests=true --filter Category=IO
/// </summary>
[Trait("Category", "IO")]
public class VectorIntIoTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
	[Fact]
	public void Json_RoundTrip_PreservesColumnsAndData()
	{
		string dir = TempFileHelper.CreateTempDirectory();
		try
		{
			var original = CreateVectorInt([1, -2, 3, 4], columns: 2);

			IO.Serialize<VectorInt, JsonFormatter>(original, "vectorint", dir);
			string json = File.ReadAllText(Path.Combine(dir, "vectorint.json"));
			var loaded = IO.Deserialize<VectorInt, JsonFormatter>(Gpu, "vectorint", dir);

			loaded.Columns.Should().Be(2);
			SyncValues(loaded).Should().Equal([1, -2, 3, 4]);
			json.Should().Contain("\"type\":\"VectorInt\"");
			json.Should().Contain("\"dtype\":\"Int32\"");
		}
		finally
		{
			TempFileHelper.Cleanup(dir);
		}
	}

	[Fact]
	public void Csv_RoundTrip_PreservesData()
	{
		string dir = TempFileHelper.CreateTempDirectory();
		try
		{
			var original = CreateVectorInt([1, 2, 3, 4, 5], columns: 5);

			IO.Serialize<VectorInt, CsvFormatter>(original, "vectorint", dir);
			string csv = File.ReadAllText(Path.Combine(dir, "vectorint.csv"));
			var loaded = IO.Deserialize<VectorInt, CsvFormatter>(Gpu, "vectorint", dir);

			csv.Should().StartWith($"schemaVersion,1{Environment.NewLine}type,dtype,columns,data");
			SyncValues(loaded).Should().Equal(SyncValues(original));
		}
		finally
		{
			TempFileHelper.Cleanup(dir);
		}
	}

	[Fact]
	public void Xml_RoundTrip_PreservesData()
	{
		string dir = TempFileHelper.CreateTempDirectory();
		try
		{
			var original = CreateVectorInt([-1, 2, -3]);

			IO.Serialize<VectorInt, XmlFormatter>(original, "vectorint", dir);
			var loaded = IO.Deserialize<VectorInt, XmlFormatter>(Gpu, "vectorint", dir);

			SyncValues(loaded).Should().Equal([-1, 2, -3]);
		}
		finally
		{
			TempFileHelper.Cleanup(dir);
		}
	}

	[Fact]
	public void Txt_RoundTrip_PreservesData()
	{
		string dir = TempFileHelper.CreateTempDirectory();
		try
		{
			var original = CreateVectorInt([1, 2, 3]);

			IO.Serialize<VectorInt, TxtFormatter>(original, "vectorint", dir);
			var loaded = IO.Deserialize<VectorInt, TxtFormatter>(Gpu, "vectorint", dir);

			SyncValues(loaded).Should().Equal([1, 2, 3]);
		}
		finally
		{
			TempFileHelper.Cleanup(dir);
		}
	}

	[Fact]
	public void Json_Collection_RoundTrips_MultipleItems()
	{
		string dir = TempFileHelper.CreateTempDirectory();
		try
		{
			using (var writer = IO.CreateWriter<VectorInt, JsonFormatter>("collection", dir))
			{
				writer.Append(CreateVectorInt([1, 2]));
				writer.Append(CreateVectorInt([3, 4, 5]));
			}

			var loaded = IO.DeserializeAll<VectorInt, JsonFormatter>(Gpu, "collection", dir);

			loaded.Should().HaveCount(2);
			SyncValues(loaded[0]).Should().Equal([1, 2]);
			SyncValues(loaded[1]).Should().Equal([3, 4, 5]);
		}
		finally
		{
			TempFileHelper.Cleanup(dir);
		}
	}
}
