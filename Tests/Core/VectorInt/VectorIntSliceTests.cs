using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Core.VectorIntTests;

public class VectorIntSliceTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
    private readonly VectorInt _matrix = new(fixture.Gpu, VectorIntFactory.Matrix3x5, columns: 5);

    [Fact]
    public void GetColumnAsArray_ReturnsColumn()
    {
        _matrix.GetColumnAsArray(0).Should().Equal([1, 6, 11]);
    }

    [Fact]
    public void GetRowAsArray_ReturnsRow()
    {
        _matrix.GetRowAsArray(1).Should().Equal([6, 7, 8, 9, 10]);
    }

    [Fact]
    public void GetColumnAsVector_ReturnsVector()
    {
        var column = _matrix.GetColumnAsVectorX(2);

        column.Length.Should().Be(3);
        SyncValues(column).Should().Equal([3, 8, 13]);
    }

    [Fact]
    public void GetRowAsVector_ReturnsVector()
    {
        var row = _matrix.GetRowAsVector(2);

        row.Length.Should().Be(5);
        SyncValues(row).Should().Equal([11, 12, 13, 14, 15]);
    }

    [Fact]
    public void GetSliceAsVector_Column_ReturnsSlice()
    {
        var slice = _matrix.GetSliceAsVectorX(3, Axis.Column);

        SyncValues(slice).Should().Equal([4, 9, 14]);
    }
}
