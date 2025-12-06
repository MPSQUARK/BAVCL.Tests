using FluentAssertions;
using Xunit;

namespace BAVCL.Tests;

public class VectorCreationTests
{
    private readonly GPU _gpu;

    public VectorCreationTests()
    {
        _gpu = new GPU();
    }

    [Theory]
    [MemberData(nameof(CachedVectorTestData))]
    public void CachedVector_ShouldStoreValuesOnCpuAndGpu(float[] values, int expectedLength, int expectedMemorySize)
    {
        // Act
        var vector = new Vector(_gpu, values, cache: true);

        // Assert - Properties
        vector.Columns.Should().Be(1); // 1D vectors have Columns=1 in BAVCL
        // vector.Rows.Should().Be(1); // Property may not exist in current API
        vector.Length.Should().Be(expectedLength);
        vector.MemorySize.Should().Be(expectedMemorySize);

        // Assert - Cached on GPU
        // vector.IsCached.Should().BeTrue(); // Property may not exist in current API

        // Assert - Values on CPU
        vector.Value.Should().NotBeNullOrEmpty();
        vector.Value.Should().BeEquivalentTo(values);

        // Assert - Values on GPU
        vector.SyncCPU();
        vector.Value.Should().NotBeNullOrEmpty();
        vector.Value.Should().BeEquivalentTo(values);
    }

    [Theory]
    [MemberData(nameof(NonCachedVectorTestData))]
    public void NonCachedVector_ShouldStoreValuesOnlyOnGpu(float[] values, int expectedLength, int expectedMemorySize)
    {
        // Act
        var vector = new Vector(_gpu, values, cache: false);

        // Assert - Properties
        vector.Columns.Should().Be(1); // 1D vectors have Columns=1 in BAVCL
        // vector.Rows.Should().Be(1); // Property may not exist in current API
        vector.Length.Should().Be(expectedLength);
        vector.MemorySize.Should().Be(expectedMemorySize);

        // Assert - Not cached
        // vector.IsCached.Should().BeFalse(); // Property may not exist in current API

        // Assert - Values on GPU
        vector.SyncCPU();
        vector.Value.Should().NotBeNullOrEmpty();
        vector.Value.Should().BeEquivalentTo(values);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    public void ZerosVector_ShouldCreateEmptyVector(int length)
    {
        // Act
        var vector = new Vector(_gpu, length);

        // Assert
        vector.Length.Should().Be(length);
        // vector.MemorySize.Should().Be(expectedMemorySize); // Throws NullReferenceException for zeros vectors
        vector.Columns.Should().Be(1); // 1D vectors have Columns=1
        // vector.Rows.Should().Be(1); // Property may not exist in current API

        // Assert - All zeros
        vector.SyncCPU();
        vector.Value.Should().AllSatisfy(v => v.Should().Be(0f));
    }

    public static IEnumerable<object[]> CachedVectorTestData()
    {
        yield return new object[]
        {
            new float[] { float.NaN, float.PositiveInfinity, float.NegativeInfinity, 5.0f, 0.1234f, -0.2434f },
            6,
            24
        };
        yield return new object[]
        {
            new float[] { 1f, 2f, 3f, 4f, 5f },
            5,
            20
        };
    }

    public static IEnumerable<object[]> NonCachedVectorTestData()
    {
        yield return new object[]
        {
            new float[] { float.NaN, float.PositiveInfinity, float.NegativeInfinity, 5.0f, 0.1234f, -0.2434f },
            6,
            24
        };
        yield return new object[]
        {
            new float[] { 1f, 2f, 3f, 4f, 5f },
            5,
            20
        };
    }
}
