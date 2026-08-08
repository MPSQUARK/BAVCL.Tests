using BAVCL.Core;
using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Core.VectorIntTests;

public class VectorIntStructuralTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
    [Fact]
    public void Transpose_SwapsRowsAndColumns()
    {
        var vector = CreateVectorInt(VectorIntFactory.Matrix3x5, columns: 5);

        var result = vector.TransposeX();

        result.Columns.Should().Be(3);
        result.RowCount().Should().Be(5);
        SyncValues(result).Should().Equal(TransposeInt(VectorIntFactory.Matrix3x5, 5));
    }

    [Fact]
    public void Concat_RowAxis_AppendsViaAppend()
    {
        var a = CreateVectorInt([1, 2, 3]);
        var b = CreateVectorInt([4, 5, 6]);

        var result = a.Concat(b, axis: ConcatAxis.Row);

        result.SyncCPU();
        result.Length.Should().Be(6);
        result.Value.Should().Equal([1, 2, 3, 4, 5, 6]);
    }

    [Fact]
    public void Append_JoinsVectors()
    {
        var a = CreateVectorInt([1, 2]);
        var b = CreateVectorInt([3, 4]);

        SyncValues(a.Append(b)).Should().Equal([1, 2, 3, 4]);
    }

    [Fact]
    public void Prepend_PutsSecondVectorFirst()
    {
        var a = CreateVectorInt([1, 2]);
        var b = CreateVectorInt([3, 4]);

        SyncValues(a.Prepend(b)).Should().Equal([3, 4, 1, 2]);
    }

    [Fact]
    public void Merge_RemovesDuplicates()
    {
        var a = CreateVectorInt([1, 2, 3]);
        var b = CreateVectorInt([3, 4, 5]);

        var result = a.Merge(b);

        result.Length.Should().Be(5);
        SyncValues(result).Should().Contain([1, 2, 3, 4, 5]);
    }

    [Fact]
    public void Flatten_SetsColumnsToZero()
    {
        var vector = CreateVectorInt([1, 2, 3, 4], columns: 2);

        vector.Flatten();

        vector.Columns.Should().Be(0);
        vector.Length.Should().Be(4);
    }

    static int[] TransposeInt(int[] data, int columns)
    {
        int rows = data.Length / columns;
        var result = new int[data.Length];
        for (int r = 0; r < rows; r++)
        for (int c = 0; c < columns; c++)
            result[c * rows + r] = data[r * columns + c];
        return result;
    }
}
