using BAVCL.Core;

namespace BAVCL.Tests.Extensions;

public class PrintExtensionsTests
{
    [Fact]
    public void ToStr_FloatArray_ReturnsBracketedString()
    {
        new float[] { 1f, 2f, 3f }.ToStr().Should().Contain("1");
    }

    [Fact]
    public void Print_Double2D_ThrowsNotImplemented()
    {
        var act = () => new double[,] { { 1, 2 } }.Print();

        act.Should().Throw<NotImplementedException>();
    }
}
