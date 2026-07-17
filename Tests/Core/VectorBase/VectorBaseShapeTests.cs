using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Core.VectorBase;

public class VectorBaseShapeTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
    [Fact]
    public void Is1D_ReturnsTrueForSingleColumn()
    {
        CreateVector([1f, 2f, 3f]).Is1D().Should().BeTrue();
    }

    [Fact]
    public void IsRectangular_ReturnsTrueForValidMatrix()
    {
        CreateVector(VectorFactory.Matrix3x5, columns: 5).IsRectangular().Should().BeTrue();
    }

    [Fact]
    public void RowCount_ReturnsLengthFor1D()
    {
        CreateVector([1f, 2f, 3f]).RowCount().Should().Be(3);
    }

    [Fact]
    public void RowCount_ReturnsRowsForMatrix()
    {
        CreateVector(VectorFactory.Matrix3x5, columns: 5).RowCount().Should().Be(3);
    }
}
