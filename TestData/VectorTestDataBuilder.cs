namespace BAVCL.Tests.TestData;

/// <summary>
/// Reusable test data builders for vector testing
/// </summary>
public static class VectorTestDataBuilder
{
    /// <summary>
    /// Generates random vector test cases for stress testing
    /// </summary>
    public static IEnumerable<object[]> GenerateRandomVectors(int size, int count, int seed = 42)
    {
        var random = new Random(seed);
        for (int i = 0; i < count; i++)
        {
            var vector = Enumerable.Range(0, size)
                .Select(_ => (float)(random.NextDouble() * 200 - 100)) // Range: -100 to 100
                .ToArray();
            yield return new object[] { vector };
        }
    }

    /// <summary>
    /// Generates edge case values for accuracy testing
    /// </summary>
    public static IEnumerable<object[]> GenerateEdgeCaseVectors()
    {
        // NaN and Infinity cases
        yield return new object[]
        {
            new float[] { float.NaN, float.PositiveInfinity, float.NegativeInfinity, 0f }
        };

        // Very small numbers (near underflow)
        yield return new object[]
        {
            new float[] { float.Epsilon, -float.Epsilon, 1e-30f, -1e-30f }
        };

        // Very large numbers (near overflow)
        yield return new object[]
        {
            new float[] { float.MaxValue / 2, -float.MaxValue / 2, 1e30f, -1e30f }
        };

        // Mixed precision
        yield return new object[]
        {
            new float[] { 1.0f, 1.0000001f, 0.9999999f, 1.0f + float.Epsilon }
        };

        // All zeros
        yield return new object[]
        {
            new float[] { 0f, 0f, 0f, 0f, 0f }
        };

        // All ones
        yield return new object[]
        {
            new float[] { 1f, 1f, 1f, 1f, 1f }
        };
    }

    /// <summary>
    /// Generates vector pairs for binary operations (add, subtract, multiply)
    /// </summary>
    public static IEnumerable<object[]> GenerateVectorPairs(int size, int count, int seed = 42)
    {
        var random = new Random(seed);
        for (int i = 0; i < count; i++)
        {
            var vectorA = Enumerable.Range(0, size)
                .Select(_ => (float)(random.NextDouble() * 20 - 10))
                .ToArray();
            var vectorB = Enumerable.Range(0, size)
                .Select(_ => (float)(random.NextDouble() * 20 - 10))
                .ToArray();

            yield return new object[] { vectorA, vectorB };
        }
    }

    /// <summary>
    /// Generates vectors of different sizes for performance testing
    /// </summary>
    public static IEnumerable<object[]> GenerateSizeVariations()
    {
        var sizes = new[] { 10, 100, 1000, 10000, 100000, 1000000 };
        foreach (var size in sizes)
        {
            var vector = Enumerable.Range(0, size)
                .Select(i => (float)i)
                .ToArray();
            yield return new object[] { size, vector };
        }
    }

    /// <summary>
    /// Creates vectors with specific patterns for testing matrix operations
    /// </summary>
    public static IEnumerable<object[]> GenerateMatrixShapedVectors()
    {
        // 3x3 matrix
        yield return new object[]
        {
            3,
            new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }
        };

        // 4x4 matrix
        yield return new object[]
        {
            4,
            new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 }
        };

        // 5x3 matrix
        yield return new object[]
        {
            3,
            new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }
        };
    }

    /// <summary>
    /// Creates vectors with known mathematical properties for validation
    /// </summary>
    public static IEnumerable<object[]> GenerateKnownResultVectors()
    {
        // Pythagorean triples (useful for distance calculations)
        yield return new object[]
        {
            new float[] { 3f, 4f },
            5f, // Expected magnitude
            "Pythagorean triple 3-4-5"
        };

        yield return new object[]
        {
            new float[] { 5f, 12f },
            13f,
            "Pythagorean triple 5-12-13"
        };

        // Unit vectors
        yield return new object[]
        {
            new float[] { 1f, 0f, 0f },
            1f,
            "Unit vector X"
        };

        yield return new object[]
        {
            new float[] { 0f, 1f, 0f },
            1f,
            "Unit vector Y"
        };
    }
}
