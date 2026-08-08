using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Core.VectorTests;

public class VectorSliceTests : GpuTestBase
{
    private readonly BAVCL.Vector _matrix;

    public VectorSliceTests(GpuTestFixture fixture) : base(fixture)
    {
        _matrix = CreateVector(VectorFactory.Matrix3x5, columns: 5);
    }

    [Fact]
    public void GetColumnAsArray_ReturnsColumn()
    {
        _matrix.GetColumnAsArray(0).ShouldBeCloseTo([1f, 6f, 11f]);
    }

    [Fact]
    public void GetRowAsArray_ReturnsRow()
    {
        _matrix.GetRowAsArray(1).ShouldBeCloseTo([6f, 7f, 8f, 9f, 10f]);
    }

    [Fact]
    public void GetColumnAsVector_ReturnsVector()
    {
        var column = _matrix.GetColumnAsVectorX(2);

        column.Length.Should().Be(3);
        SyncValues(column).ShouldBeCloseTo([3f, 8f, 13f]);
    }

    [Fact]
    public void GetRowAsVector_ReturnsVector()
    {
        var row = _matrix.GetRowAsVector(2);

        row.Length.Should().Be(5);
        SyncValues(row).ShouldBeCloseTo([11f, 12f, 13f, 14f, 15f]);
    }

    [Fact]
    public void GetSliceAsVector_Column_ReturnsSlice()
    {
        var slice = _matrix.GetSliceAsVectorX(3, Axis.Column);

        SyncValues(slice).ShouldBeCloseTo([4f, 9f, 14f]);
    }
}
