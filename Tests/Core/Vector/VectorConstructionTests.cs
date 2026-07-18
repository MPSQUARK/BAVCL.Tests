using BAVCL.Core;
using BAVCL.Geometric;
using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Core.VectorTests;

public class VectorConstructionTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Constructor_WithValues_SetsDimensions(bool cache)
    {
        var vector = CreateVector(VectorFactory.Small1D, cache: cache);

        vector.Length.Should().Be(5);
        vector.Columns.Should().Be(0);
        vector.MemorySize.Should().Be(5 * sizeof(float));
        SyncValues(vector).ShouldBeCloseTo(VectorFactory.Small1D);
    }

    [Fact]
    public void Constructor_WithColumns_StoresMatrixShape()
    {
        var vector = CreateVector(VectorFactory.Matrix3x5, columns: 5);

        vector.Length.Should().Be(15);
        vector.Columns.Should().Be(5);
        vector.RowCount().Should().Be(3);
        vector.IsRectangular().Should().BeTrue();
        vector.Is1D().Should().BeFalse();
    }

    [Fact]
    public void Copy_CreatesIndependentVector()
    {
        var original = CreateVector(VectorFactory.Small1D);
        var copy = original.Copy();

        copy.Equals(original).Should().BeTrue();
        copy.Should().NotBeSameAs(original);
    }

    [Fact]
    public void Equals_ReturnsFalseForDifferentValues()
    {
        var a = CreateVector([1f, 2f, 3f]);
        var b = CreateVector([1f, 2f, 4f]);

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void Shape_ReturnsLengthAndColumns()
    {
        var vector = CreateVector(VectorFactory.Matrix3x5, columns: 5);

        vector.Shape().Should().Be(new Shape(3, 5));
    }

    [Fact]
    public void Flatten_SetsColumnsToZero()
    {
        var vector = CreateVector(VectorFactory.Matrix3x5, columns: 5);

        vector.Flatten();

        vector.Columns.Should().Be(0);
    }

    [Fact]
    public void ToVector3_RequiresMultipleOfThree()
    {
        var vector = CreateVector(VectorFactory.Vector3Data);

        var vec3 = vector.ToVector3();

        vec3.Length.Should().Be(9);
        vec3.Columns.Should().Be(3);
    }

    [Fact]
    public void ToVector3_ThrowsWhenLengthNotMultipleOfThree()
    {
        var vector = CreateVector([1f, 2f, 3f, 4f]);

        var act = () => vector.ToVector3();

        act.Should().Throw<Exception>().WithMessage("*multiple of 3*");
    }
}
