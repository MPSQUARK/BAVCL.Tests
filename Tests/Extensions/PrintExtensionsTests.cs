using BAVCL.Core;

namespace BAVCL.Tests.Extensions;

public class PrintExtensionsTests
{
    [Fact]
    public void ToStr_FloatArray_ReturnsBracketedString()
    {
        BAVCL.Core.Extensions.ToStr(new float[] { 1f, 2f, 3f }).Should().Contain("1");
    }

    [Fact]
    public void Print_Double2D_ThrowsNotImplemented()
    {
        var act = () => BAVCL.Core.Extensions.Print(new double[,] { { 1, 2 } });

        act.Should().Throw<NotImplementedException>();
    }
}
