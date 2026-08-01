using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Core.VectorTests;
public class VectorIndexingTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
    [Theory]
    [InlineData(0, 1f)]
    [InlineData(2, 3f)]
    [InlineData(4, 5f)]
    public void Indexer1D_ReturnsCorrectValue(int index, float expected)
    {
        var vector = CreateVector(VectorFactory.Small1D);

        vector[index].Should().Be(expected);
    }

    [Theory]
    [InlineData(0, 0, 1f)]
    [InlineData(1, 2, 8f)]
    [InlineData(2, 4, 15f)]
    public void GetAt2D_ReturnsCorrectValue(int row, int col, float expected)
    {
        var vector = CreateVector(VectorFactory.Matrix3x5, columns: 5);

        vector.GetAt(row, col).Should().Be(expected);
    }

    [Fact]
    public void SetAt_UpdatesValue()
    {
        var vector = CreateVector([.. VectorFactory.Small1D]);

        using (var scope = vector.CpuScopeAndSync())
        {
            EditableView<float> view = scope.View;
            view[2] = 99f;
        }
        vector.SyncCPU();
        vector.Value[2].Should().Be(99f);
    }

    [Fact]
    public void GetSliceAsArray_Column_ReturnsColumnValues()
    {
        var vector = CreateVector(VectorFactory.Matrix3x5, columns: 5);

        var column = vector.GetSliceAsArray(1, Axis.Column);

        column.ShouldBeCloseTo([2f, 7f, 12f]);
    }

    [Fact]
    public void GetSliceAsArray_Row_ReturnsRowValues()
    {
        var vector = CreateVector(VectorFactory.Matrix3x5, columns: 5);

        var row = vector.GetSliceAsArray(0, Axis.Row);

        row.ShouldBeCloseTo([1f, 2f, 3f, 4f, 5f]);
    }
}
