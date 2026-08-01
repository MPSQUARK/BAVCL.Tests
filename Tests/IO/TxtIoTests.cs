using BAVCL.Geometric;
using BAVCL.Modules.IO;
using BAVCL.Tests.Helpers;
using BAVCL.Types;

namespace BAVCL.Tests.IOTests;

[Trait("Category", "IO")]
public class TxtIoTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
	[Fact]
	public void Vector_1D_RoundTrip()
	{
		var original = CreateVector([1f, 2f, 3f]);
		IFormatter<Vector> formatter = TxtFormatter.Default;

		var loaded = formatter.Deserialize(Gpu, formatter.Serialize(original));

		loaded.Columns.Should().Be(0);
		SyncValues(loaded).ShouldBeCloseTo([1f, 2f, 3f]);
	}

	[Fact]
	public void Vector_Column_RoundTrip()
	{
		var original = CreateVector([1f, 2f, 3f], columns: 1);
		IFormatter<Vector> formatter = TxtFormatter.Default;

		var loaded = formatter.Deserialize(Gpu, formatter.Serialize(original));

		loaded.Columns.Should().Be(1);
		SyncValues(loaded).ShouldBeCloseTo([1f, 2f, 3f]);
	}

	[Fact]
	public void Vector_2D_RoundTrip()
	{
		var original = CreateVector([1f, 2f, 3f, 4f], columns: 2);
		IFormatter<Vector> formatter = TxtFormatter.Default;

		var loaded = formatter.Deserialize(Gpu, formatter.Serialize(original));

		loaded.Columns.Should().Be(2);
		SyncValues(loaded).ShouldBeCloseTo([1f, 2f, 3f, 4f]);
	}

	[Fact]
	public void Vector3_RoundTrip()
	{
		var original = new Vector3(Gpu, [1f, 2f, 3f, 4f, 5f, 6f]);
		IFormatter<Vector3> formatter = TxtFormatter.Default;

		var loaded = formatter.Deserialize(Gpu, formatter.Serialize(original));

		loaded.Columns.Should().Be(3);
		loaded.ToArray().ShouldBeCloseTo([1f, 2f, 3f, 4f, 5f, 6f]);
	}

	[Fact]
	public void Mask_1D_RoundTrip()
	{
		var original = CreateMask([true, false, true]);
		IFormatter<Mask> formatter = TxtFormatter.Default;

		var loaded = formatter.Deserialize(Gpu, formatter.Serialize(original));

		loaded.Columns.Should().Be(0);
		SyncMaskBits(loaded).Should().Equal(true, false, true);
	}

	[Fact]
	public void Mask_2D_RoundTrip()
	{
		var original = CreateMask([true, false, true, true], columns: 2);
		IFormatter<Mask> formatter = TxtFormatter.Default;

		var loaded = formatter.Deserialize(Gpu, formatter.Serialize(original));

		loaded.Columns.Should().Be(2);
		SyncMaskBits(loaded).Should().Equal(true, false, true, true);
	}

	[Fact]
	public void Vector_SpecialFloats_RoundTrip()
	{
		var original = CreateVector([float.NaN, float.PositiveInfinity, float.NegativeInfinity, -1.5f]);
		IFormatter<Vector> formatter = TxtFormatter.Default;

		float[] values = SyncValues(formatter.Deserialize(Gpu, formatter.Serialize(original)));

		float.IsNaN(values[0]).Should().BeTrue();
		float.IsPositiveInfinity(values[1]).Should().BeTrue();
		float.IsNegativeInfinity(values[2]).Should().BeTrue();
		values[3].Should().BeApproximately(-1.5f, 1e-3f);
	}

	[Fact]
	public void Vector_2D_Serialize_DoesNotStartWithNewline()
	{
		var original = CreateVector([1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f, 10f], columns: 2);
		IFormatter<Vector> formatter = TxtFormatter.Default;

		string text = formatter.Serialize(original);

		text.Should().NotStartWith("\n");
		text.Should().NotStartWith("\r\n");
	}

	[Fact]
	public void Vector3_Serialize_DoesNotStartWithNewline()
	{
		var original = new Vector3(Gpu, [1f, 2f, 3f, 4f, 5f, 6f]);
		IFormatter<Vector3> formatter = TxtFormatter.Default;

		string text = formatter.Serialize(original);

		text.Should().NotStartWith("\n");
		text.Should().NotStartWith("\r\n");
	}
}
