using BAVCL.Geometric;
using BAVCL.Modules.IO;
using BAVCL.Modules.IO.Enums;
using BAVCL.Tests.Helpers;
using BAVCL.Types;

namespace BAVCL.Tests.IOTests;

[Trait("Category", "IO")]
public class XmlIoTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
	[Fact]
	public void Vector_RoundTrip_PreservesColumnsAndData()
	{
		string dir = TempFileHelper.CreateTempDirectory();
		try
		{
			var original = CreateVector([1f, 2f, 3f, 4f], columns: 2);

			IO.Serialize<Vector, XmlFormatter>(original, "vector", dir);
			string xml = File.ReadAllText(Path.Combine(dir, "vector.xml"));
			var loaded = IO.Deserialize<Vector, XmlFormatter>(Gpu, "vector", dir);

			loaded.Columns.Should().Be(2);
			SyncValues(loaded).ShouldBeCloseTo([1f, 2f, 3f, 4f]);
			xml.Should().Contain("type=\"Vector\"");
			xml.Should().Contain("dtype=\"Single\"");
			xml.Should().Contain("schemaVersion=\"1\"");
		}
		finally
		{
			TempFileHelper.Cleanup(dir);
		}
	}

	[Fact]
	public void Vector3_RoundTrip_PreservesData()
	{
		var original = new Vector3(Gpu, [1f, 2f, 3f, 4f, 5f, 6f]);
		IFormatter<Vector3> formatter = XmlFormatter.Default;

		var loaded = formatter.Deserialize(Gpu, formatter.Serialize(original));

		loaded.Columns.Should().Be(3);
		loaded.ToArray().ShouldBeCloseTo([1f, 2f, 3f, 4f, 5f, 6f]);
	}

	[Fact]
	public void Vector3_TypeMismatch_Throws()
	{
		IFormatter<Vector> vectorFormatter = XmlFormatter.Default;
		string vectorXml = vectorFormatter.Serialize(CreateVector([1f, 2f, 3f]));
		IFormatter<Vector3> formatter = XmlFormatter.Default;

		var act = () => formatter.Deserialize(Gpu, vectorXml);

		act.Should().Throw<FormatException>().WithMessage("*vector3*");
	}

	[Fact]
	public void Mask_Packed_RoundTrip()
	{
		var original = CreateMask([true, false, true, true], columns: 2);
		IFormatter<Mask> formatter = XmlFormatter.Default;

		string xml = formatter.Serialize(original, MaskSerializeFlags.Packed);
		var loaded = formatter.Deserialize(Gpu, xml);

		loaded.ElementCount.Should().Be(4);
		loaded.Columns.Should().Be(2);
		SyncMaskBits(loaded).Should().Equal(true, false, true, true);
		xml.Should().Contain("count=\"4\"");
		xml.Should().Contain("dtype=\"Int32\"");
	}

	[Fact]
	public void Mask_Bool_RoundTrip()
	{
		var original = CreateMask([false, true, false], columns: 0);
		IFormatter<Mask> formatter = XmlFormatter.Default;

		string xml = formatter.Serialize(original, MaskSerializeFlags.Bool);
		var loaded = formatter.Deserialize(Gpu, xml);

		loaded.ElementCount.Should().Be(3);
		loaded.Columns.Should().Be(0);
		SyncMaskBits(loaded).Should().Equal(false, true, false);
		xml.Should().Contain("dtype=\"Boolean\"");
		xml.Should().NotContain("count=");
	}

	[Fact]
	public void Mask_Packed_InvalidWordCount_Throws()
	{
		const string xml = """<mask schemaVersion="1" type="Mask" dtype="Int32" columns="0" count="33"><data>1</data></mask>""";

		var act = () => ((IFormatter<Mask>)XmlFormatter.Default).Deserialize(Gpu, xml);

		act.Should().Throw<Exception>().WithMessage("*word*");
	}

	[Fact]
	public void Vector_NaNAndInfinity_RoundTrip()
	{
		var original = CreateVector([float.NaN, float.PositiveInfinity, float.NegativeInfinity]);
		IFormatter<Vector> formatter = XmlFormatter.Default;

		float[] values = SyncValues(formatter.Deserialize(Gpu, formatter.Serialize(original)));

		float.IsNaN(values[0]).Should().BeTrue();
		float.IsPositiveInfinity(values[1]).Should().BeTrue();
		float.IsNegativeInfinity(values[2]).Should().BeTrue();
	}

	[Fact]
	public void Mask_Bool_RoutesOnDtype_NotCountHeuristic()
	{
		const string xml = """<mask schemaVersion="1" type="Mask" dtype="Boolean" columns="0" count="99"><data>true</data><data>false</data><data>true</data></mask>""";

		var loaded = ((IFormatter<Mask>)XmlFormatter.Default).Deserialize(Gpu, xml);

		loaded.ElementCount.Should().Be(3);
		SyncMaskBits(loaded).Should().Equal(true, false, true);
	}
}
