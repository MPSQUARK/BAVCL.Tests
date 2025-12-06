using FluentAssertions;
using Xunit;

namespace BAVCL.Tests;

public class VectorOperationsTests
{
    private readonly GPU _gpu;

    public VectorOperationsTests()
    {
        _gpu = new GPU();
    }

    [Theory]
    [MemberData(nameof(VectorAdditionTestData))]
    public void VectorAddition_ShouldAddElementWise(float[] valuesA, float[] valuesB, float[] expected)
    {
        // Arrange
        var vectorA = new Vector(_gpu, valuesA, cache: true);
        var vectorB = new Vector(_gpu, valuesB, cache: true);

        // Act
        var result = vectorA + vectorB;

        // Assert
        result.SyncCPU();
        result.Value.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [MemberData(nameof(VectorSubtractionTestData))]
    public void VectorSubtraction_ShouldSubtractElementWise(float[] valuesA, float[] valuesB, float[] expected)
    {
        // Arrange
        var vectorA = new Vector(_gpu, valuesA, cache: true);
        var vectorB = new Vector(_gpu, valuesB, cache: true);

        // Act
        var result = vectorA - vectorB;

        // Assert
        result.SyncCPU();
        result.Value.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [MemberData(nameof(VectorMultiplicationTestData))]
    public void VectorMultiplication_ShouldMultiplyElementWise(float[] valuesA, float[] valuesB, float[] expected)
    {
        // Arrange
        var vectorA = new Vector(_gpu, valuesA, cache: true);
        var vectorB = new Vector(_gpu, valuesB, cache: true);

        // Act
        var result = vectorA * vectorB;

        // Assert
        result.SyncCPU();
        result.Value.Should().BeEquivalentTo(expected);
    }

    public static IEnumerable<object[]> VectorAdditionTestData()
    {
        yield return new object[]
        {
            new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 },
            new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, -1, -2, -2, -2, -2 },
            new float[] { 2, 4, 6, 8, 10, 12, 14, 16, 18, 20, 10, 10, 11, 12, 13 }
        };
        // Add more test cases for edge cases
        yield return new object[]
        {
            new float[] { 0, -1, 5.5f },
            new float[] { 0, 1, -2.5f },
            new float[] { 0, 0, 3.0f }
        };
    }

    public static IEnumerable<object[]> VectorSubtractionTestData()
    {
        yield return new object[]
        {
            new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 },
            new float[] { 2, 2, 2, 2, 2, 2, 2, 2, -2, -2, -2, -2, -2, -2, -2 },
            new float[] { -1, 0, 1, 2, 3, 4, 5, 6, 11, 12, 13, 14, 15, 16, 17 }
        };
    }

    public static IEnumerable<object[]> VectorMultiplicationTestData()
    {
        yield return new object[]
        {
            new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 },
            new float[] { 2, 2, 2, 2, 2, 2, 2, 2, -2, -2, -2, -2, -2, -2, -2 },
            new float[] { 2, 4, 6, 8, 10, 12, 14, 16, -18, -20, -22, -24, -26, -28, -30 }
        };
    }
}
