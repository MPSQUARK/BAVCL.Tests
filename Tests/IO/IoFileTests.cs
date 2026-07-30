using BAVCL.Geometric;
using BAVCL.Modules.IO.Enums;
using BAVCL.Tests.Helpers;
using BAVCL.Types;

namespace BAVCL.Tests.IOTests;

/// <summary>
/// Disk path / overwrite / round-trip tests. Deletes temp dirs after each run.
/// Excluded from default test runs (Category=IO). Run with: dotnet test -p:IncludeIOTests=true --filter Category=IO
/// </summary>
[Trait("Category", "IO")]
public class IoFileTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
	[Fact]
	public void Json_Vector_FileRoundTrip()
	{
		string dir = TempFileHelper.CreateTempDirectory();
		try
		{
			var original = CreateVector([1f, 2f, 3f, 4f], columns: 2);

			IO.Serialize<Vector, JsonFormatter>(original, "vector", dir);

			var loaded = IO.Deserialize<Vector, JsonFormatter>(Gpu, "vector", dir);

			loaded.Columns.Should().Be(2);
			SyncValues(loaded).ShouldBeCloseTo([1f, 2f, 3f, 4f]);
			File.Exists(Path.Combine(dir, "vector.json")).Should().BeTrue();
		}
		finally
		{
			TempFileHelper.Cleanup(dir);
		}
	}

	[Fact]
	public void Json_Vector3_And_Mask_FileRoundTrip()
	{
		string dir = TempFileHelper.CreateTempDirectory();
		try
		{
			var vec3 = new Vector3(Gpu, [1f, 0f, 0f, 0f, 1f, 0f]);
			var mask = CreateMask([true, true, false, false], columns: 2);

			IO.Serialize<Vector3, JsonFormatter>(vec3, "v3", dir);
			IO.Serialize<Mask, JsonFormatter>(mask, "mask", dir);
			IO.Serialize<Mask, JsonFormatter>(mask, "mask-bool", dir, flags: MaskSerializeFlags.Bool);

			IO.Deserialize<Vector3, JsonFormatter>(Gpu, "v3", dir).ToArray()
				.ShouldBeCloseTo([1f, 0f, 0f, 0f, 1f, 0f]);
			SyncMaskBits(IO.Deserialize<Mask, JsonFormatter>(Gpu, "mask", dir))
				.Should().Equal(true, true, false, false);
			SyncMaskBits(IO.Deserialize<Mask, JsonFormatter>(Gpu, "mask-bool", dir))
				.Should().Equal(true, true, false, false);
		}
		finally
		{
			TempFileHelper.Cleanup(dir);
		}
	}

	[Fact]
	public void Csv_Vector_FileRoundTrip()
	{
		string dir = TempFileHelper.CreateTempDirectory();
		try
		{
			var original = CreateVector([1f, 2f, 3f, 4f, 5f], columns: 5);

			IO.Serialize<Vector, CsvFormatter>(original, "vector", dir);

			var loaded = IO.Deserialize<Vector, CsvFormatter>(Gpu, "vector", dir);
			SyncValues(loaded).ShouldBeCloseTo(SyncValues(original));
		}
		finally
		{
			TempFileHelper.Cleanup(dir);
		}
	}

	[Fact]
	public void Txt_Vector_WritesContent()
	{
		string dir = TempFileHelper.CreateTempDirectory();
		try
		{
			var vector = CreateVector([1f, 2f, 3f]);

			IO.Serialize<Vector, TxtFormatter>(vector, "vector", dir);

			string text = File.ReadAllText(Path.Combine(dir, "vector.txt"));
			text.Should().Contain("1");
			text.Should().Contain("2");
		}
		finally
		{
			TempFileHelper.Cleanup(dir);
		}
	}

	[Fact]
	public void Serialize_WhenFileExists_AndOverwriteFalse_Throws()
	{
		string dir = TempFileHelper.CreateTempDirectory();
		try
		{
			IO.Serialize<Vector, JsonFormatter>(CreateVector([1f]), "dup", dir, overwrite: false);

			var act = () => IO.Serialize<Vector, JsonFormatter>(CreateVector([2f]), "dup", dir, overwrite: false);

			act.Should().Throw<IOException>().WithMessage("*already exists*");
		}
		finally
		{
			TempFileHelper.Cleanup(dir);
		}
	}

	[Fact]
	public void Serialize_WhenOverwriteTrue_ReplacesFile()
	{
		string dir = TempFileHelper.CreateTempDirectory();
		try
		{
			IO.Serialize<Vector, JsonFormatter>(CreateVector([1f, 2f]), "dup", dir, overwrite: false);
			IO.Serialize<Vector, JsonFormatter>(CreateVector([9f, 8f, 7f]), "dup", dir, overwrite: true);

			SyncValues(IO.Deserialize<Vector, JsonFormatter>(Gpu, "dup", dir))
				.ShouldBeCloseTo([9f, 8f, 7f]);
		}
		finally
		{
			TempFileHelper.Cleanup(dir);
		}
	}

	[Fact]
	public void WriteRaw_WritesPlainText()
	{
		string dir = TempFileHelper.CreateTempDirectory();
		try
		{
			IO.CreateWriter<Vector, TxtFormatter>("note", dir).WriteRaw("hello world");

			File.ReadAllText(Path.Combine(dir, "note.txt")).Should().Be("hello world");
		}
		finally
		{
			TempFileHelper.Cleanup(dir);
		}
	}
}
