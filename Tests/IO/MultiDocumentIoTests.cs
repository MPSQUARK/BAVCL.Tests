using BAVCL.Geometric;
using BAVCL.Modules.IO;
using BAVCL.Modules.IO.Enums;
using BAVCL.Tests.Helpers;
using BAVCL.Types;

namespace BAVCL.Tests.IOTests;

/// <summary>
/// Multi-document (batch) read/write tests: Append/Flush/DeserializeAll across all four formats.
/// Run with: dotnet test -p:IncludeIOTests=true --filter Category=IO
/// </summary>
[Trait("Category", "IO")]
public class MultiDocumentIoTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
	[Fact]
	public void Csv_Append_ThreeVectors_RoundTripsAll()
	{
		string dir = TempFileHelper.CreateTempDirectory();
		try
		{
			using (var writer = IO.CreateWriter<Vector, CsvFormatter>("vectors", dir))
			{
				writer.Append(CreateVector([1f, 2f]));
				writer.Append(CreateVector([3f, 4f]));
				writer.Append(CreateVector([5f, 6f]));
			}

			IReadOnlyList<Vector> loaded = IO.DeserializeAll<Vector, CsvFormatter>(Gpu, "vectors", dir);

			loaded.Should().HaveCount(3);
			SyncValues(loaded[0]).ShouldBeCloseTo([1f, 2f]);
			SyncValues(loaded[1]).ShouldBeCloseTo([3f, 4f]);
			SyncValues(loaded[2]).ShouldBeCloseTo([5f, 6f]);

			string csv = File.ReadAllText(Path.Combine(dir, "vectors.csv"));
			csv.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Should().HaveCount(5); // schema line + header + 3 rows
		}
		finally
		{
			TempFileHelper.Cleanup(dir);
		}
	}

	[Fact]
	public void Json_Append_ThreeVectors_RoundTripsAll()
	{
		string dir = TempFileHelper.CreateTempDirectory();
		try
		{
			using (var writer = IO.CreateWriter<Vector, JsonFormatter>("vectors", dir))
			{
				writer.Append(CreateVector([1f, 2f]));
				writer.Append(CreateVector([3f, 4f]));
				writer.Append(CreateVector([5f, 6f]));
			}

			string json = File.ReadAllText(Path.Combine(dir, "vectors.json"));
			json.Should().StartWith("{\"schemaVersion\":1,\"items\":[").And.EndWith("]}");
			json.Should().NotContain("\"schemaVersion\":1,\"type\"");

			IReadOnlyList<Vector> loaded = IO.DeserializeAll<Vector, JsonFormatter>(Gpu, "vectors", dir);

			loaded.Should().HaveCount(3);
			SyncValues(loaded[1]).ShouldBeCloseTo([3f, 4f]);
		}
		finally
		{
			TempFileHelper.Cleanup(dir);
		}
	}

	[Fact]
	public void Xml_Append_ThreeVectors_RoundTripsAll()
	{
		string dir = TempFileHelper.CreateTempDirectory();
		try
		{
			using (var writer = IO.CreateWriter<Vector, XmlFormatter>("vectors", dir))
			{
				writer.Append(CreateVector([1f, 2f]));
				writer.Append(CreateVector([3f, 4f]));
				writer.Append(CreateVector([5f, 6f]));
			}

			string xml = File.ReadAllText(Path.Combine(dir, "vectors.xml"));
			xml.Should().StartWith("<root schemaVersion=\"1\">").And.EndWith("</root>");
			xml.Should().NotContain("<vector type=");

			IReadOnlyList<Vector> loaded = IO.DeserializeAll<Vector, XmlFormatter>(Gpu, "vectors", dir);

			loaded.Should().HaveCount(3);
			SyncValues(loaded[2]).ShouldBeCloseTo([5f, 6f]);
		}
		finally
		{
			TempFileHelper.Cleanup(dir);
		}
	}

	[Fact]
	public void Txt_Append_ThreeVectors_RoundTripsAll()
	{
		string dir = TempFileHelper.CreateTempDirectory();
		try
		{
			using (var writer = IO.CreateWriter<Vector, TxtFormatter>("vectors", dir))
			{
				writer.Append(CreateVector([1f, 2f]));
				writer.Append(CreateVector([3f, 4f]));
				writer.Append(CreateVector([5f, 6f]));
			}

			string text = File.ReadAllText(Path.Combine(dir, "vectors.txt"));
			text.Should().Contain("---");

			IReadOnlyList<Vector> loaded = IO.DeserializeAll<Vector, TxtFormatter>(Gpu, "vectors", dir);

			loaded.Should().HaveCount(3);
			SyncValues(loaded[0]).ShouldBeCloseTo([1f, 2f]);
			SyncValues(loaded[2]).ShouldBeCloseTo([5f, 6f]);
		}
		finally
		{
			TempFileHelper.Cleanup(dir);
		}
	}

	[Fact]
	public void Csv_Append_ThreeMasks_RoundTripsAll()
	{
		string dir = TempFileHelper.CreateTempDirectory();
		try
		{
			using (var writer = IO.CreateWriter<Mask, CsvFormatter>("masks", dir))
			{
				writer.Append(CreateMask([true, false], columns: 2), MaskSerializeFlags.Bool);
				writer.Append(CreateMask([false, true], columns: 2), MaskSerializeFlags.Bool);
			}

			IReadOnlyList<Mask> loaded = IO.DeserializeAll<Mask, CsvFormatter>(Gpu, "masks", dir);

			loaded.Should().HaveCount(2);
			SyncMaskBits(loaded[0]).Should().Equal(true, false);
			SyncMaskBits(loaded[1]).Should().Equal(false, true);
		}
		finally
		{
			TempFileHelper.Cleanup(dir);
		}
	}

	[Fact]
	public void Json_Append_ThreeMasks_RoundTripsAll()
	{
		string dir = TempFileHelper.CreateTempDirectory();
		try
		{
			using (var writer = IO.CreateWriter<Mask, JsonFormatter>("masks", dir))
			{
				writer.Append(CreateMask([true, false, true], columns: 3), MaskSerializeFlags.Packed);
				writer.Append(CreateMask([false, false, true], columns: 3), MaskSerializeFlags.Packed);
			}

			IReadOnlyList<Mask> loaded = IO.DeserializeAll<Mask, JsonFormatter>(Gpu, "masks", dir);

			loaded.Should().HaveCount(2);
			SyncMaskBits(loaded[0]).Should().Equal(true, false, true);
			SyncMaskBits(loaded[1]).Should().Equal(false, false, true);
		}
		finally
		{
			TempFileHelper.Cleanup(dir);
		}
	}

	[Fact]
	public void Deserialize_WhenFileHasMultipleItems_ThrowsAndPointsToDeserializeAll()
	{
		string dir = TempFileHelper.CreateTempDirectory();
		try
		{
			using (var writer = IO.CreateWriter<Vector, JsonFormatter>("vectors", dir))
			{
				writer.Append(CreateVector([1f]));
				writer.Append(CreateVector([2f]));
			}

			var act = () => IO.Deserialize<Vector, JsonFormatter>(Gpu, "vectors", dir);

			act.Should().Throw<InvalidOperationException>().WithMessage("*DeserializeAll*");
		}
		finally
		{
			TempFileHelper.Cleanup(dir);
		}
	}

	[Fact]
	public void Json_SerializeAll_MatchesAppendedFile()
	{
		string dir = TempFileHelper.CreateTempDirectory();
		try
		{
			Vector[] vectors = [CreateVector([1f, 2f]), CreateVector([3f, 4f]), CreateVector([5f, 6f])];

			using (var writer = IO.CreateWriter<Vector, JsonFormatter>("appended", dir))
			{
				foreach (Vector vector in vectors)
					writer.Append(vector);
			}

			ICollectionFormatter<Vector> formatter = JsonFormatter.Default;
			string serialized = formatter.SerializeAll(vectors);

			serialized.Should().Be(File.ReadAllText(Path.Combine(dir, "appended.json")));
		}
		finally
		{
			TempFileHelper.Cleanup(dir);
		}
	}

	[Fact]
	public void SerializeAll_WithEmptyCollection_Throws()
	{
		ICollectionFormatter<Vector> formatter = JsonFormatter.Default;

		var act = () => formatter.SerializeAll([]);

		act.Should().Throw<ArgumentException>().WithMessage("*at least one item*");
	}

	[Fact]
	public void FileSession_Serialize_DoesNotTouchDisk()
	{
		string dir = TempFileHelper.CreateTempDirectory();
		try
		{
			var writer = IO.CreateWriter<Vector, JsonFormatter>("untouched", dir);

			string fragment = writer.Serialize(CreateVector([1f, 2f]));

			fragment.Should().Contain("\"type\":\"Vector\"");
			File.Exists(Path.Combine(dir, "untouched.json")).Should().BeFalse();
		}
		finally
		{
			TempFileHelper.Cleanup(dir);
		}
	}

	[Fact]
	public void Append_WithMismatchedMaskFlags_Throws()
	{
		string dir = TempFileHelper.CreateTempDirectory();
		try
		{
			using var writer = IO.CreateWriter<Mask, CsvFormatter>("masks", dir);
			writer.Append(CreateMask([true, false], columns: 2), MaskSerializeFlags.Bool);

			var act = () => writer.Append(CreateMask([true, true], columns: 2), MaskSerializeFlags.Packed);

			act.Should().Throw<InvalidOperationException>().WithMessage("*consistent*");
		}
		finally
		{
			TempFileHelper.Cleanup(dir);
		}
	}

	[Fact]
	public void WriteRaw_AfterAppendStarted_Throws()
	{
		string dir = TempFileHelper.CreateTempDirectory();
		try
		{
			using var writer = IO.CreateWriter<Vector, JsonFormatter>("mixed", dir);
			writer.Append(CreateVector([1f]));

			var act = () => writer.WriteRaw("not json");

			act.Should().Throw<InvalidOperationException>().WithMessage("*Flush*");
		}
		finally
		{
			TempFileHelper.Cleanup(dir);
		}
	}

	[Fact]
	public void Flush_CalledAfterDispose_IsIdempotent()
	{
		string dir = TempFileHelper.CreateTempDirectory();
		try
		{
			var writer = IO.CreateWriter<Vector, JsonFormatter>("vector", dir);
			writer.Append(CreateVector([1f, 2f]));
			writer.Dispose();

			var act = () => writer.Flush();

			act.Should().NotThrow();
			IO.Deserialize<Vector, JsonFormatter>(Gpu, "vector", dir).Length.Should().Be(2);
		}
		finally
		{
			TempFileHelper.Cleanup(dir);
		}
	}

	[Fact]
	public void Write_AfterAppendStarted_Throws()
	{
		string dir = TempFileHelper.CreateTempDirectory();
		try
		{
			using var writer = IO.CreateWriter<Vector, JsonFormatter>("vector", dir);
			writer.Append(CreateVector([1f]));

			var act = () => writer.Write(CreateVector([2f]));

			act.Should().Throw<InvalidOperationException>().WithMessage("*Write after Append*");
		}
		finally
		{
			TempFileHelper.Cleanup(dir);
		}
	}
}
