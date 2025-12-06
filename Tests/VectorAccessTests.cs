using static BAVCL.Core.Enums;

namespace BAVCL.Tests;

public class VectorAccessTests
{
    private readonly GPU _gpu;
    private readonly Vector _testVector;

    public VectorAccessTests()
    {
        _gpu = new GPU();
        _testVector = new Vector(
            _gpu,
            new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 },
            columns: 5,
            cache: true
        );
    }

    [Theory]
    [InlineData(0, 1f)]
    [InlineData(5, 6f)]
    [InlineData(10, 11f)]
    [InlineData(14, 15f)]
    public void VectorIndexAccess_ShouldReturnCorrectValue(int index, float expected)
    {
        // Act
        var result = _testVector[index];

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [MemberData(nameof(ColumnAccessTestData))]
    public void VectorColumnAccess_ShouldReturnCorrectSlice(int column, float[] expected)
    {
        // Act
        var result = _testVector.GetSliceAsArray(column, Axis.Column);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [MemberData(nameof(RowAccessTestData))]
    public void VectorRowAccess_ShouldReturnCorrectSlice(int row, float[] expected)
    {
        // Act
        var result = _testVector.GetSliceAsArray(row, Axis.Row);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    public static IEnumerable<object[]> ColumnAccessTestData()
    {
        // Column 2 should be: indices 2, 7, 12 (values: 3, 8, 13) based on 5 columns
        yield return new object[]
        {
            2,
            new float[] { 3, 8, 13 }
        };
        yield return new object[]
        {
            0,
            new float[] { 1, 6, 11 }
        };
    }

    public static IEnumerable<object[]> RowAccessTestData()
    {
        // Row 2 (third row with 5 columns) should be indices 10-14
        yield return new object[]
        {
            2,
            new float[] { 11, 12, 13, 14, 15 }
        };
        yield return new object[]
        {
            0,
            new float[] { 1, 2, 3, 4, 5 }
        };
        yield return new object[]
        {
            1,
            new float[] { 6, 7, 8, 9, 10 }
        };
    }
}
