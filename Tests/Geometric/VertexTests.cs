using BAVCL.Geometric;
using BAVCL.Tests.Helpers;
using BAVCL.Utility;

namespace BAVCL.Tests.Geometric;

public class VertexTests
{
    [Fact]
    public void Constructor_SetsComponents()
    {
        var vertex = new Vertex(1f, 2f, 3f);

        vertex.X.Should().Be(1f);
        vertex.Y.Should().Be(2f);
        vertex.Z.Should().Be(3f);
    }

    [Fact]
    public void Constructor_InvalidArrayLength_Throws()
    {
        var act = () => new Vertex([1f, 2f]);

        act.Should().Throw<Exception>().WithMessage("*3 Values*");
    }

    [Theory]
    [InlineData("UP", 0f, 1f, 0f)]
    [InlineData("DOWN", 0f, -1f, 0f)]
    [InlineData("FORWARD", 1f, 0f, 0f)]
    public void PresetVertices_ReturnExpectedValues(string name, float x, float y, float z)
    {
        var vertex = name switch
        {
            "UP" => Vertex.UP(),
            "DOWN" => Vertex.DOWN(),
            "FORWARD" => Vertex.FORWARD(),
            _ => throw new ArgumentException(name)
        };

        vertex.X.Should().Be(x);
        vertex.Y.Should().Be(y);
        vertex.Z.Should().Be(z);
    }

    [Fact]
    public void Addition_AddsComponents()
    {
        var a = new Vertex(1f, 2f, 3f);
        var b = new Vertex(4f, 5f, 6f);

        var result = a + b;

        result.X.Should().Be(5f);
        result.Y.Should().Be(7f);
        result.Z.Should().Be(9f);
    }

    [Fact]
    public void Magnitude_ReturnsLength()
    {
        new Vertex(3f, 4f, 0f).Magnitude().Should().Be(5f);
    }

    [Fact]
    public void Distance_ReturnsDistanceBetweenVertices()
    {
        var a = new Vertex(0f, 0f, 0f);
        var b = new Vertex(3f, 4f, 0f);

        a.Distance(b).Should().Be(5f);
    }

    [Fact]
    public void Dot_ReturnsScalarProduct()
    {
        var a = new Vertex(1f, 2f, 3f);
        var b = new Vertex(4f, 5f, 6f);

        a.Dot(b).Should().Be(32f);
    }

    [Fact]
    public void Equals_UsesTolerance()
    {
        var a = new Vertex(1f, 2f, 3f);
        var b = new Vertex(1.000001f, 2f, 3f);

        Vertex.Equals(a, b).Should().BeTrue();
    }

    [Fact]
    public void UnitVector_NormalizesToLengthOne()
    {
        var vertex = new Vertex(3f, 4f, 0f);
        vertex.UnitVector_IP();

        vertex.Magnitude().ShouldBeCloseTo(1f);
    }
}
