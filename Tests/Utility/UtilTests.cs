using BAVCL.Utility;

namespace BAVCL.Tests.Utility;

public class UtilTests
{
    [Fact]
    public void IsClose_ReturnsTrueWithinTolerance()
    {
        Util.IsClose(1f, 1.000001f).Should().BeTrue();
        Util.IsClose(1f, 2f).Should().BeFalse();
    }

    [Fact]
    public void MinMax_NonInf_CapsInfinityContributionAt999()
    {
        var arr = new float[] { 5f, 2f, float.PositiveInfinity, float.NaN };

        Util.Max(arr, NonInf: true).Should().Be(999f);
        Util.Min(arr, NonInf: true).Should().Be(2f);
    }

    [Fact]
    public void MinMaxInf_DetectsInfinity()
    {
        var arr = new float[] { 1f, float.PositiveInfinity, 3f };

        var (min, max, hasInf) = Util.MinMaxInf(arr);

        hasInf.Should().BeTrue();
        min.Should().Be(1);
        max.Should().Be(999);
    }

    [Fact]
    public void MinMax_EmptyArray_Throws()
    {
        var act = () => Util.Min(new float[] { });

        act.Should().Throw<Exception>().WithMessage("*Length 0*");
    }
}
