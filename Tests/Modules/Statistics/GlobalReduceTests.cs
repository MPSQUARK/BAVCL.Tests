using BAVCL.Core.Helpers;
using BAVCL.Modules.Arithmetic;
using BAVCL.Core.Exceptions;
using BAVCL.Modules.Statistics;
using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Modules.Statistics;

[Collection("GpuSerial")]
public class GlobalReduceTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
    static readonly Random Rng = new(31);

    [Theory]
    [InlineData(1)]
    [InlineData(7)]
    [InlineData(256)]
    [InlineData(4096)]
    public void Sum_MatchesSumX(int length)
    {
        float[] data = RandomData(length);
        var vector = CreateVector(data);

        vector.Sum().ShouldBeCloseTo(vector.SumX(), 1e-4f);
    }

    [Fact]
    public void SumX_LargeN_MatchesErrorCorrectedSum()
    {
        var vector = CreateVector(Enumerable.Repeat(0.1f, 10_000).ToArray());
        vector.SumX().ShouldBeCloseTo(1000f, 0.1f);
        vector.Sum().ShouldBeCloseTo(vector.SumX(), 0.1f);
    }

    [Fact]
    public void SumX_LargeN_WithinSixDecimalPlaces()
    {
        const int count = 1_000_000;
        float expected = count * 0.1f;
        var vector = CreateVector(Enumerable.Repeat(0.1f, count).ToArray());

        MathF.Abs(vector.SumX() - expected).Should().BeLessThan(1e-5f);
    }

    [Fact]
    public void SumX_10M_RepeatedTenth_MatchesCpuSum()
    {
        const int count = 10_000_000;
        var vector = CreateVector(Enumerable.Repeat(0.1f, count).ToArray());

        vector.SumX().ShouldBeCloseTo(1_000_000f, 0.1f);
        vector.Sum().ShouldBeCloseTo(vector.SumX(), 0.1f);
    }

    [Fact]
    public void SumX_SmallN_AccurateOnRepeatedTenth()
    {
        var vector = CreateVector(Enumerable.Repeat(0.1f, 100).ToArray());
        vector.SumX().ShouldBeCloseTo(10f, 1e-4f);
    }

    [Fact]
    public void Sum_LargeRepeatedValue_ErrorCorrectedMoreAccurateThanNaiveLoop()
    {
        const int count = 10_000;
        float[] data = Enumerable.Repeat(0.1f, count).ToArray();
        var vector = CreateVector(data);

        float naive = 0f;
        foreach (float value in data)
            naive += value;

        float corrected = vector.Sum();
        MathF.Abs(corrected - 1000f).Should().BeLessThan(MathF.Abs(naive - 1000f));
        corrected.ShouldBeCloseTo(1000f, 0.1f);
    }

    [Fact]
    public void Dot_LargeRepeatedValue_ErrorCorrectedMoreAccurateThanNaiveLoop()
    {
        const int count = 10_000;
        float[] values = Enumerable.Repeat(0.1f, count).ToArray();
        var left = CreateVector(values);
        var right = CreateVector(Enumerable.Repeat(1f, count).ToArray());

        float naive = 0f;
        for (int i = 0; i < count; i++)
            naive += values[i];

        float corrected = left.Dot(right);
        MathF.Abs(corrected - 1000f).Should().BeLessThan(MathF.Abs(naive - 1000f));
        corrected.ShouldBeCloseTo(1000f, 0.1f);
    }

    [Fact]
    public void DotX_SmallN_AccurateOnRepeatedTenth()
    {
        const int count = 100;
        float[] values = Enumerable.Repeat(0.1f, count).ToArray();
        var left = CreateVector(values);
        var right = CreateVector(Enumerable.Repeat(1f, count).ToArray());

        left.DotX(right).ShouldBeCloseTo(10f, 1e-4f);
    }

    [Fact]
    public void VarX_ConstantVector_IsZero()
    {
        var vector = CreateVector(Enumerable.Repeat(0.1f, 100).ToArray());
        vector.VarX().ShouldBeCloseTo(0f, 1e-4f);
    }

    [Theory]
    [InlineData(10_000)]
    [InlineData(1_000_000)]
    public void AllX_LargeN_MatchesCpuAll(int length)
    {
        int[] data = RandomIntData(length, includeZero: true);
        var vector = CreateVectorInt(data);
        vector.All().Should().Be(vector.AllX());

        float[] floatData = data.Select(v => (float)v).ToArray();
        var floatVector = CreateVector(floatData);
        floatVector.All().Should().Be(floatVector.AllX());
    }

    [Fact]
    public void VectorInt_Sum_Overflow_WrapsUnchecked_CpuMatchesGpu()
    {
        int[] data = [int.MaxValue, 2];
        int expected = unchecked(int.MaxValue + 2);
        var vector = CreateVectorInt(data);

        vector.Sum().Should().Be(expected);
        vector.SumX().Should().Be(expected);
    }

    [Fact]
    public void Dot_Scalar_UsesScalarTimesSum()
    {
        float[] data = [2f, 3f, 5f];
        var vector = CreateVector(data);
        vector.Dot(4f).Should().Be(40f);
        vector.DotX(4f).ShouldBeCloseTo(40f, 1e-5f);
    }

    [Theory]
    [InlineData(16)]
    [InlineData(1024)]
    public void MinMaxMean_VarStd_All_MatchGpu(int length)
    {
        float[] data = RandomData(length, includeZero: true);
        var vector = CreateVector(data);

        vector.Min().ShouldBeCloseTo(vector.MinX(), 1e-5f);
        vector.Max().ShouldBeCloseTo(vector.MaxX(), 1e-5f);
        vector.MinMax().Should().Be(vector.MinMaxX());
        vector.Range().ShouldBeCloseTo(vector.RangeX(), 1e-5f);
        vector.Mean().ShouldBeCloseTo(vector.MeanX(), 1e-4f);
        vector.Var().ShouldBeCloseTo(vector.VarX(), 1e-5f);
        vector.Std().ShouldBeCloseTo(vector.StdX(), 1e-5f);
        vector.All().Should().Be(vector.AllX());
    }

    [Theory]
    [InlineData(16)]
    [InlineData(128)]
    public void Percentile_MedianQuartiles_MatchSortedReference(int length)
    {
        float[] data = RandomData(length);
        Array.Sort(data);
        var vector = CreateVector(data);

        vector.Median().ShouldBeCloseTo(PercentileReference(data, 50f), 1e-5f);
        vector.Quartile1().ShouldBeCloseTo(PercentileReference(data, 25f), 1e-5f);
        vector.Quartile3().ShouldBeCloseTo(PercentileReference(data, 75f), 1e-5f);
        vector.Iqr().ShouldBeCloseTo(PercentileReference(data, 75f) - PercentileReference(data, 25f), 1e-5f);
        vector.MedianX().ShouldBeCloseTo(vector.Median(), 1e-5f);
    }

    [Theory]
    [InlineData(-1f)]
    [InlineData(101f)]
    [InlineData(float.NaN)]
    public void Percentile_OutOfRange_ThrowsFixedRange(float percentile)
    {
        var vector = CreateVector([3f, 1f, 2f]);
        var act = () => vector.Percentile(percentile);
        act.Should().Throw<FixedRangeException>().Where(ex => ex.ParamName == "percentile");
    }

    [Theory]
    [InlineData(16)]
    [InlineData(1024)]
    public void VectorInt_Reduces_MatchGpu(int length)
    {
        int[] data = RandomIntData(length, includeZero: true);
        var vector = CreateVectorInt(data);

        vector.Sum().ShouldBeCloseTo(vector.SumX(), 1e-4f);
        vector.Min().Should().Be(vector.MinX());
        vector.Max().Should().Be(vector.MaxX());
        vector.MinMax().Should().Be(vector.MinMaxX());
        vector.Range().Should().Be(vector.RangeX());
        vector.Mean().ShouldBeCloseTo(vector.MeanX(), 1e-4f);
        vector.Var().ShouldBeCloseTo(vector.VarX(), 1e-5f);
        vector.Std().ShouldBeCloseTo(vector.StdX(), 1e-5f);
        vector.All().Should().Be(vector.AllX());
    }

    [Fact]
    public void Var_LargeN_DoesNotRequireArithmeticKernels()
    {
        float[] data = Enumerable.Repeat(0.1f, 12_000).ToArray();
        var vector = CreateVector(data);

        vector.Var().ShouldBeCloseTo(0f, 1e-5f);
        vector.VarX().ShouldBeCloseTo(0f, 1e-5f);
    }

    [Fact]
    public void Var_LargeMeanOffset_WithinSixDecimalPlaces_CpuMatchesGpu()
    {
        // Values 1..5 offset by 1e5 — within float32 precision; two-pass would still be fragile at larger offsets.
        float[] data = [100_001f, 100_002f, 100_003f, 100_004f, 100_005f];
        var vector = CreateVector(data);

        vector.Var().ShouldBeCloseTo(2f, 1e-5f);
        vector.VarX().ShouldBeCloseTo(2f, 1e-5f);
        vector.VarX().ShouldBeCloseTo(vector.Var(), 1e-5f);
    }

    [Fact]
    public void Sum_Empty_ReturnsZero()
    {
        var vector = new Vector(Gpu, 0);
        vector.Sum().Should().Be(0f);
        vector.SumX().Should().Be(0f);
    }

    [Fact]
    public void All_Empty_ReturnsTrue()
    {
        var vector = new Vector(Gpu, 0);
        vector.All().Should().BeTrue();
        vector.AllX().Should().BeTrue();
    }

    [Fact]
    public void Mean_Empty_ThrowsDivideByZero()
    {
        var vector = new Vector(Gpu, 0);
        var actCpu = () => vector.Mean();
        var actGpu = () => vector.MeanX();
        actCpu.Should().Throw<DivideByZeroException>();
        actGpu.Should().Throw<DivideByZeroException>();
    }

    [Fact]
    public void Min_Empty_FloatCpuThrows_InvalidOperation_GpuReturnsNaN()
    {
        var vector = new Vector(Gpu, 0);
        var actCpu = () => vector.Min();
        actCpu.Should().Throw<InvalidOperationException>();
        vector.MinX().Should().Be(float.NaN);
    }

    [Fact]
    public void Min_Empty_VectorInt_ThrowsInvalidOperation()
    {
        var vector = new VectorInt(Gpu, 0);
        var actCpu = () => vector.Min();
        var actGpu = () => vector.MinX();
        actCpu.Should().Throw<InvalidOperationException>();
        actGpu.Should().Throw<InvalidOperationException>();
    }

    static float[] RandomData(int length, bool includeZero = false)
    {
        float[] data = new float[length];
        for (int i = 0; i < length; i++)
            data[i] = includeZero && i % 17 == 0 ? 0f : (float)(Rng.NextDouble() * 20 - 10);
        return data;
    }

    static int[] RandomIntData(int length, bool includeZero = false)
    {
        int[] data = new int[length];
        for (int i = 0; i < length; i++)
            data[i] = includeZero && i % 19 == 0 ? 0 : Rng.Next(-50, 50);
        return data;
    }

    static float PercentileReference(float[] sorted, float percentile)
    {
        if (sorted.Length == 1)
            return sorted[0];

        float rank = (sorted.Length - 1) * percentile / 100f;
        int lower = (int)MathF.Floor(rank);
        int upper = (int)MathF.Ceiling(rank);
        float weight = rank - lower;
        return sorted[lower] + weight * (sorted[upper] - sorted[lower]);
    }
}
