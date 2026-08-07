using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Core.VectorBase;

public class VectorBaseShapeTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
    [Fact]
    public void Is1D_ReturnsTrueForRow1D()
    {
        CreateVector([1f, 2f, 3f]).Is1D().Should().BeTrue();
    }

    [Fact]
    public void Is1D_ReturnsTrueForColumnVector()
    {
        BavclShape.Create(Gpu, [3, 1], [1f, 2f, 3f]).Is1D().Should().BeTrue();
    }

    [Fact]
    public void Is1DRowVector_ReturnsFalseForColumnVector()
    {
        BavclShape.Create(Gpu, [3, 1], [1f, 2f, 3f]).Is1DRowVector().Should().BeFalse();
    }

    [Fact]
    public void Is1DRowVector_ReturnsTrueForRow1D()
    {
        CreateVector([1f, 2f, 3f]).Is1DRowVector().Should().BeTrue();
    }

    [Fact]
    public void Is2D_ReturnsTrueForMatrix()
    {
        CreateVector(VectorFactory.Matrix3x5, columns: 5).Is2D().Should().BeTrue();
    }

    [Fact]
    public void Is2D_ReturnsFalseForColumnVector()
    {
        BavclShape.Create(Gpu, [3, 1], [1f, 2f, 3f]).Is2D().Should().BeFalse();
    }

    [Fact]
    public void ElementsPerRow_ReturnsLengthForFlat1D()
    {
        CreateVector([1f, 2f, 3f]).ElementsPerRow().Should().Be(3);
    }

    [Fact]
    public void ElementsPerRow_ReturnsColumnsForMatrix()
    {
        CreateVector(VectorFactory.Matrix3x5, columns: 5).ElementsPerRow().Should().Be(5);
    }

    [Fact]
    public void Shape_ColumnVector_ReturnsLengthByOne()
    {
        BavclShape.Create(Gpu, [3, 1], [1f, 2f, 3f]).Shape().Should().Be(new Shape(3, 1));
    }

    [Fact]
    public void IsRectangular_ReturnsTrueForValidMatrix()
    {
        CreateVector(VectorFactory.Matrix3x5, columns: 5).IsRectangular().Should().BeTrue();
    }

    [Fact]
    public void Shape_Flat1D_ReturnsOneByLength()
    {
        CreateVector([1f, 2f, 3f]).Shape().Should().Be(new Shape(1, 3));
    }

    [Fact]
    public void RowCount_ReturnsOneFor1D()
    {
        CreateVector([1f, 2f, 3f]).RowCount().Should().Be(1);
    }

    [Fact]
    public void RowCount_ReturnsRowsForMatrix()
    {
        CreateVector(VectorFactory.Matrix3x5, columns: 5).RowCount().Should().Be(3);
    }
}
