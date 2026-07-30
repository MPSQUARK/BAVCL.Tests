using BAVCL.Geometric;
using BAVCL.Modules.IO.Enums;
using BAVCL.Tests.Helpers;
using BAVCL.Types;

namespace BAVCL.Tests.IOTests;

/// <summary>
/// JSON schema and round-trip tests via the public IO API.
/// Run with: dotnet test -p:IncludeIOTests=true --filter Category=IO
/// </summary>
[Trait("Category", "IO")]
public class JsonIoTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
	[Fact]
	public void Vector_RoundTrip_PreservesColumnsAndData()
	{
		string dir = TempFileHelper.CreateTempDirectory();
		try
		{
			var original = CreateVector([1f, 2f, 3f, 4f], columns: 2);

			IO.Serialize<Vector, JsonFormatter>(original, "vector", dir);
			string json = File.ReadAllText(Path.Combine(dir, "vector.json"));
			var loaded = IO.Deserialize<Vector, JsonFormatter>(Gpu, "vector", dir);

			loaded.Columns.Should().Be(2);
			SyncValues(loaded).ShouldBeCloseTo([1f, 2f, 3f, 4f]);
			json.Should().Contain("\"type\":\"Vector\"");
			json.Should().Contain("\"dtype\":\"float32\"");
			json.Should().Contain("\"schemaVersion\":1");
		}
		finally
		{
			TempFileHelper.Cleanup(dir);
		}
	}

	[Fact]
	public void Vector_Empty_RoundTrips()
	{
		string dir = TempFileHelper.CreateTempDirectory();
		try
		{
			var original = CreateVector([], columns: 0, cache: false);

			IO.Serialize<Vector, JsonFormatter>(original, "empty", dir);
			var loaded = IO.Deserialize<Vector, JsonFormatter>(Gpu, "empty", dir);

			loaded.Columns.Should().Be(0);
			loaded.Length.Should().Be(0);
		}
		finally
		{
			TempFileHelper.Cleanup(dir);
		}
	}

	[Fact]
	public void Vector_NaNAndInfinity_RoundTrip()
	{
		string dir = TempFileHelper.CreateTempDirectory();
		try
		{
			var original = CreateVector([float.NaN, float.PositiveInfinity, float.NegativeInfinity]);

			IO.Serialize<Vector, JsonFormatter>(original, "special", dir);
			float[] values = SyncValues(IO.Deserialize<Vector, JsonFormatter>(Gpu, "special", dir));

			float.IsNaN(values[0]).Should().BeTrue();
			float.IsPositiveInfinity(values[1]).Should().BeTrue();
			float.IsNegativeInfinity(values[2]).Should().BeTrue();
		}
		finally
		{
			TempFileHelper.Cleanup(dir);
		}
	}

	[Fact]
	public void Vector3_RoundTrip_PreservesData()
	{
		string dir = TempFileHelper.CreateTempDirectory();
		try
		{
			var original = new Vector3(Gpu, [1f, 2f, 3f, 4f, 5f, 6f]);

			IO.Serialize<Vector3, JsonFormatter>(original, "v3", dir);
			var loaded = IO.Deserialize<Vector3, JsonFormatter>(Gpu, "v3", dir);

			loaded.Columns.Should().Be(3);
			loaded.ToArray().ShouldBeCloseTo([1f, 2f, 3f, 4f, 5f, 6f]);
		}
		finally
		{
			TempFileHelper.Cleanup(dir);
		}
	}

	[Fact]
	public void Vector3_TypeMismatch_Throws()
	{
		string dir = TempFileHelper.CreateTempDirectory();
		try
		{
			IO.Serialize<Vector, JsonFormatter>(CreateVector([1f, 2f, 3f]), "vector", dir);

			var act = () => IO.Deserialize<Vector3, JsonFormatter>(Gpu, "vector", dir);

			act.Should().Throw<Exception>().WithMessage("*Vector*");
		}
		finally
		{
			TempFileHelper.Cleanup(dir);
		}
	}

	[Fact]
	public void Mask_Packed_RoundTrip()
	{
		string dir = TempFileHelper.CreateTempDirectory();
		try
		{
			var original = CreateMask([true, false, true, true], columns: 2);

			IO.Serialize<Mask, JsonFormatter>(original, "mask", dir);
			string json = File.ReadAllText(Path.Combine(dir, "mask.json"));
			var loaded = IO.Deserialize<Mask, JsonFormatter>(Gpu, "mask", dir);

			loaded.ElementCount.Should().Be(4);
			loaded.Columns.Should().Be(2);
			SyncMaskBits(loaded).Should().Equal(true, false, true, true);
			json.Should().Contain("\"count\":4");
			json.Should().Contain("\"dtype\":\"int32\"");
		}
		finally
		{
			TempFileHelper.Cleanup(dir);
		}
	}

	[Fact]
	public void Mask_Bool_RoundTrip()
	{
		string dir = TempFileHelper.CreateTempDirectory();
		try
		{
			var original = CreateMask([false, true, false], columns: 0);

			IO.Serialize<Mask, JsonFormatter>(original, "mask", dir, flags: MaskSerializeFlags.Bool);
			string json = File.ReadAllText(Path.Combine(dir, "mask.json"));
			var loaded = IO.Deserialize<Mask, JsonFormatter>(Gpu, "mask", dir);

			loaded.ElementCount.Should().Be(3);
			loaded.Columns.Should().Be(0);
			SyncMaskBits(loaded).Should().Equal(false, true, false);
			json.Should().Contain("\"dtype\":\"bool\"");
			json.Should().NotContain("\"count\"");
		}
		finally
		{
			TempFileHelper.Cleanup(dir);
		}
	}

	[Fact]
	public void Mask_Packed_InvalidWordCount_Throws()
	{
		string dir = TempFileHelper.CreateTempDirectory();
		try
		{
			const string json = """{"schemaVersion":1,"type":"Mask","dtype":"int32","columns":0,"count":33,"data":[1]}""";

			IO.CreateWriter<Mask, JsonFormatter>("bad", dir).WriteRaw(json);

			var act = () => IO.Deserialize<Mask, JsonFormatter>(Gpu, "bad", dir);

			act.Should().Throw<Exception>().WithMessage("*word*");
		}
		finally
		{
			TempFileHelper.Cleanup(dir);
		}
	}
}
