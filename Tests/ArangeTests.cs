namespace BAVCL.Tests.Tests;

public class ArangeTests
{
    public ArangeTests() { }

    [Fact]
    public void Arange_ShouldCreateAPositiveRangeOfNumbers()
    {
        var result = Vector.Arange(0, 10, 0.5f);

        var expected = new float[] {
            0.0f, 0.5f, 1.0f, 1.5f, 2.0f, 2.5f, 3.0f, 3.5f, 4.0f, 4.5f,
            5.0f, 5.5f, 6.0f, 6.5f, 7.0f, 7.5f, 8.0f, 8.5f, 9.0f, 9.5f
        };

        result.Should().NotBeNull();
        result.Length.Should().Be(20);
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Arange_ShouldCreateANegativeRangeOfNumbers()
    {
        var result = Vector.Arange(0, -10, -1f);

        var expected = new float[] {
            0f, -1f, -2f, -3f, -4f, -5f, -6f, -7f, -8f, -9f
        };

        result.Should().NotBeNull();
        result.Length.Should().Be(10);
        result.Should().BeEquivalentTo(expected);

        var result2 = Vector.Arange(0, -10, 1f);
        result2.Should().NotBeNull();
        result2.Length.Should().Be(10);
        result2.Should().BeEquivalentTo(expected);
    }
}
