using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Core.VectorIntTests;

public class VectorIntIndexingTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
    [Theory]
    [InlineData(0, 1)]
    [InlineData(2, 3)]
    [InlineData(4, 5)]
    public void Indexer1D_ReturnsCorrectValue(int index, int expected)
    {
        var vector = CreateVectorInt(VectorIntFactory.Small1D);

        vector[index].Should().Be(expected);
    }

    [Theory]
    [InlineData(0, 0, 1)]
    [InlineData(1, 2, 8)]
    [InlineData(2, 4, 15)]
    public void GetAt2D_ReturnsCorrectValue(int row, int col, int expected)
    {
        var vector = CreateVectorInt(VectorIntFactory.Matrix3x5, columns: 5);

        vector.GetAt(row, col).Should().Be(expected);
    }

    [Fact]
    public void SetAt_UpdatesValue()
    {
        var vector = CreateVectorInt([.. VectorIntFactory.Small1D]);

        using (var scope = vector.CpuScopeAndSync())
        {
            EditableView<int> view = scope.View;
            view[2] = 99;
        }

        vector.SyncCPU();
        vector.Value[2].Should().Be(99);
    }

    [Fact]
    public void GetSliceAsArray_Column_ReturnsColumnValues()
    {
        var vector = CreateVectorInt(VectorIntFactory.Matrix3x5, columns: 5);

        vector.GetSliceAsArray(1, Axis.Column).Should().Equal([2, 7, 12]);
    }

    [Fact]
    public void GetSliceAsArray_Row_ReturnsRowValues()
    {
        var vector = CreateVectorInt(VectorIntFactory.Matrix3x5, columns: 5);

        vector.GetSliceAsArray(0, Axis.Row).Should().Equal([1, 2, 3, 4, 5]);
    }
}
