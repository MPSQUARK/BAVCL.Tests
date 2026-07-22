using BAVCL.Modules.Arithmetic;
using BAVCL.Modules.Statistics;

namespace BAVCL.Tests.Extensions;

public class ArrayExtensionsTests
{
    [Fact]
    public void Sum_FloatArray_ReturnsTotal()
    {
        new float[] { 1f, 2f, 3f, 4f }.Sum().Should().Be(10f);
    }

    [Fact]
    public void Sum_DoubleArray_ReturnsTotal()
    {
        new double[] { 1, 2, 3, 4 }.Sum().Should().Be(10);
    }

    [Fact]
    public void Average_FloatArray_ReturnsMean()
    {
        new float[] { 2f, 4f, 6f }.Average().Should().Be(4f);
    }

    [Fact]
    public void MinMax_FloatArray_ReturnExtremes()
    {
        var arr = new float[] { 3f, 1f, 5f, 2f };

        arr.Min().Should().Be(1f);
        arr.Max().Should().Be(5f);
    }

    [Fact]
    public void MinMax_IntArray_ReturnExtremes()
    {
        var arr = new int[] { 3, 1, 5, 2 };

        arr.Min().Should().Be(1);
        arr.Max().Should().Be(5);
    }
}
