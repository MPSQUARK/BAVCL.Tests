using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Core.VectorTests;

public class VectorStructuralTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
    [Fact]
    public void Transpose_SwapsRowsAndColumns()
    {
        var vector = CreateVector(VectorFactory.Matrix3x5, columns: 5);

        var result = Vector.Transpose(vector);

        result.Columns.Should().Be(3);
        result.RowCount().Should().Be(5);
        SyncValues(result).ShouldBeCloseTo(CpuReference.Transpose(VectorFactory.Matrix3x5, 5));
    }

    [Fact]
    public void Dot_ReturnsScalarProduct()
    {
        var a = CreateVector([1f, 2f, 3f]);
        var b = CreateVector([4f, 5f, 6f]);

        Vector.Dot(a, b).Should().Be(32f);
        a.Dot(b).Should().Be(32f);
    }

    [Fact]
    public void Concat_RowAxis_AppendsViaAppend()
    {
        var a = CreateVector([1f, 2f, 3f]);
        var b = CreateVector([4f, 5f, 6f]);

        var result = Vector.Concat(a, b, axis: 'r');

        result.SyncCPU();
        result.Length.Should().Be(6);
        result.Value.ShouldBeCloseTo([1f, 2f, 3f, 4f, 5f, 6f]);
    }

    [Fact]
    public void Append_JoinsVectors()
    {
        var a = CreateVector([1f, 2f]);
        var b = CreateVector([3f, 4f]);

        var result = Vector.Append(a, b);

        SyncValues(result).ShouldBeCloseTo([1f, 2f, 3f, 4f]);
    }

    [Fact]
    public void Prepend_PutsSecondVectorFirst()
    {
        var a = CreateVector([1f, 2f]);
        var b = CreateVector([3f, 4f]);

        var result = Vector.Prepend(a, b);

        SyncValues(result).ShouldBeCloseTo([3f, 4f, 1f, 2f]);
    }

    [Fact]
    public void Merge_RemovesDuplicates()
    {
        var a = CreateVector([1f, 2f, 3f]);
        var b = CreateVector([3f, 4f, 5f]);

        var result = Vector.Merge(a, b);

        result.Length.Should().Be(5);
        SyncValues(result).Should().Contain([1f, 2f, 3f, 4f, 5f]);
    }
}
