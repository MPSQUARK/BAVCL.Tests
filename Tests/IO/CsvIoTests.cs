using BAVCL.Geometric;
using BAVCL.Modules.IO;
using BAVCL.Modules.IO.Enums;
using BAVCL.Tests.Helpers;
using BAVCL.Types;

namespace BAVCL.Tests.IOTests;

[Trait("Category", "IO")]
public class CsvIoTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
	[Fact]
	public void Vector_1D_RoundTrip()
	{
		var original = CreateVector([1f, 2f, 3f]);
		IFormatter<Vector> formatter = CsvFormatter.Default;

		string csv = formatter.Serialize(original);
		var loaded = formatter.Deserialize(Gpu, csv);

		csv.Should().StartWith("schemaVersion,type,dtype,columns,data");
		loaded.Columns.Should().Be(0);
		SyncValues(loaded).ShouldBeCloseTo([1f, 2f, 3f]);
	}

	[Fact]
	public void Vector_2D_RoundTrip()
	{
		var original = CreateVector([1f, 2f, 3f, 4f], columns: 2);
		IFormatter<Vector> formatter = CsvFormatter.Default;

		var loaded = formatter.Deserialize(Gpu, formatter.Serialize(original));

		loaded.Columns.Should().Be(2);
		SyncValues(loaded).ShouldBeCloseTo([1f, 2f, 3f, 4f]);
	}

	[Fact]
	public void Vector3_RoundTrip()
	{
		var original = new Vector3(Gpu, [1f, 2f, 3f, 4f, 5f, 6f]);
		IFormatter<Vector3> formatter = CsvFormatter.Default;

		string csv = formatter.Serialize(original);
		var loaded = formatter.Deserialize(Gpu, csv);

		csv.Should().Contain("Vector3");
		csv.Should().Contain("1;2;3;4;5;6");
		loaded.Columns.Should().Be(3);
		loaded.ToArray().ShouldBeCloseTo([1f, 2f, 3f, 4f, 5f, 6f]);
	}

	[Fact]
	public void Mask_Bool_RoundTrip()
	{
		var original = CreateMask([true, false, true, true], columns: 2);
		IFormatter<Mask> formatter = CsvFormatter.Default;

		string csv = formatter.Serialize(original, MaskSerializeFlags.Bool);
		var loaded = formatter.Deserialize(Gpu, csv);

		csv.Should().Contain(",Mask,Boolean,");
		csv.Should().Contain("1;0;1;1");
		loaded.ElementCount.Should().Be(4);
		loaded.Columns.Should().Be(2);
		SyncMaskBits(loaded).Should().Equal(true, false, true, true);
	}

	[Fact]
	public void Mask_Packed_RoundTrip()
	{
		var original = CreateMask([true, false, true, true, false, false, false, false, true], columns: 3);
		IFormatter<Mask> formatter = CsvFormatter.Default;

		string csv = formatter.Serialize(original, MaskSerializeFlags.Packed);
		var loaded = formatter.Deserialize(Gpu, csv);

		csv.Should().Contain(",Mask,Int32,");
		csv.Should().Contain("schemaVersion,type,dtype,columns,count,data");
		loaded.ElementCount.Should().Be(9);
		loaded.Columns.Should().Be(3);
		SyncMaskBits(loaded).Should().Equal(true, false, true, true, false, false, false, false, true);
	}

	[Fact]
	public void Vector3_InvalidLength_Throws()
	{
		const string csv = """
			schemaVersion,type,dtype,columns,data
			1,Vector3,Single,3,1;2;3;4
			""";

		var act = () => ((IFormatter<Vector3>)CsvFormatter.Default).Deserialize(Gpu, csv);

		act.Should().Throw<FormatException>().WithMessage("*multiple of 3*");
	}

	[Fact]
	public void Mask_Packed_InvalidWordCount_Throws()
	{
		const string csv = """
			schemaVersion,type,dtype,columns,count,data
			1,Mask,Int32,0,33,1
			""";

		var act = () => ((IFormatter<Mask>)CsvFormatter.Default).Deserialize(Gpu, csv);

		act.Should().Throw<FormatException>().WithMessage("*word*");
	}

	[Fact]
	public void Vector_InvalidFloat_Throws()
	{
		const string csv = """
			schemaVersion,type,dtype,columns,data
			1,Vector,Single,0,1;abc;3
			""";

		var act = () => ((IFormatter<Vector>)CsvFormatter.Default).Deserialize(Gpu, csv);

		act.Should().Throw<FormatException>().WithMessage("*invalid float*");
	}

	[Fact]
	public void MissingHeaderRow_Throws()
	{
		const string csv = "1,Vector,Single,0,1;2;3";

		var act = () => ((IFormatter<Vector>)CsvFormatter.Default).Deserialize(Gpu, csv);

		act.Should().Throw<FormatException>().WithMessage("*header row*");
	}

	[Fact]
	public void Vector3_TypeMismatch_Throws()
	{
		var vectorCsv = ((IFormatter<Vector>)CsvFormatter.Default).Serialize(CreateVector([1f, 2f, 3f]));
		var act = () => ((IFormatter<Vector3>)CsvFormatter.Default).Deserialize(Gpu, vectorCsv);

		act.Should().Throw<FormatException>().WithMessage("*Vector3*");
	}

	[Fact]
	public void Vector_NaNAndInfinity_RoundTrip()
	{
		var original = CreateVector([float.NaN, float.PositiveInfinity, float.NegativeInfinity]);
		IFormatter<Vector> formatter = CsvFormatter.Default;

		float[] values = SyncValues(formatter.Deserialize(Gpu, formatter.Serialize(original)));

		float.IsNaN(values[0]).Should().BeTrue();
		float.IsPositiveInfinity(values[1]).Should().BeTrue();
		float.IsNegativeInfinity(values[2]).Should().BeTrue();
	}

	[Fact]
	public void Vector_Empty_RoundTrips()
	{
		var original = CreateVector([], columns: 0, cache: false);
		IFormatter<Vector> formatter = CsvFormatter.Default;

		var loaded = formatter.Deserialize(Gpu, formatter.Serialize(original));

		loaded.Columns.Should().Be(0);
		loaded.Length.Should().Be(0);
	}

	[Fact]
	public void Vector_EmptyDataToken_Throws()
	{
		const string csv = """
			schemaVersion,type,dtype,columns,data
			1,Vector,Single,0,1;;2
			""";

		var act = () => ((IFormatter<Vector>)CsvFormatter.Default).Deserialize(Gpu, csv);

		act.Should().Throw<FormatException>().WithMessage("*empty element*");
	}
}
