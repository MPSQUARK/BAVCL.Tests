using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Core.VectorTests;

public class VectorStructuralTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
    [Fact]
    public void Transpose_SwapsRowsAndColumns()
    {
        var vector = CreateVector(VectorFactory.Matrix3x5, columns: 5);

        var result = vector.Transpose();

        result.Columns.Should().Be(3);
        result.RowCount().Should().Be(5);
        SyncValues(result).ShouldBeCloseTo(CpuReference.Transpose(VectorFactory.Matrix3x5, 5));
    }

    [Fact]
    public void Dot_ReturnsScalarProduct()
    {
        var a = CreateVector([1f, 2f, 3f]);
        var b = CreateVector([4f, 5f, 6f]);

        a.Dot(b).Should().Be(32f);
        a.Dot(b).Should().Be(32f);
    }

    // Known LRU issue: CPU-side append updates Value but GPU cache can resync stale data
    // when lengths mismatch (IsICacheableLive / SyncCPU path). Length not updating is a symptom.
    [Fact]
    public void Concat_RowAxis_AppendsViaAppend()
    {
        var a = CreateVector([1f, 2f, 3f]);
        var b = CreateVector([4f, 5f, 6f]);

        var result = a.Concat(b, axis: 'r');

        result.SyncCPU();
        result.Length.Should().Be(6);
        result.Value.ShouldBeCloseTo([1f, 2f, 3f, 4f, 5f, 6f]);
    }

    [Fact]
    public void Append_JoinsVectors()
    {
        var a = CreateVector([1f, 2f]);
        var b = CreateVector([3f, 4f]);

        var result = a.Append(b);

        SyncValues(result).ShouldBeCloseTo([1f, 2f, 3f, 4f]);
    }

    [Fact]
    public void Prepend_PutsSecondVectorFirst()
    {
        var a = CreateVector([1f, 2f]);
        var b = CreateVector([3f, 4f]);

        var result = a.Prepend(b);

        SyncValues(result).ShouldBeCloseTo([3f, 4f, 1f, 2f]);
    }

    [Fact]
    public void Merge_RemovesDuplicates()
    {
        var a = CreateVector([1f, 2f, 3f]);
        var b = CreateVector([3f, 4f, 5f]);

        var result = a.Merge(b);

        result.Length.Should().Be(5);
        SyncValues(result).Should().Contain([1f, 2f, 3f, 4f, 5f]);
    }
}
