using BAVCL.Modules.Statistics;
using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Modules.Statistics;

[Collection("GpuSerial")]
public class VarianceDefinitionTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
    const float Tolerance = 1e-5f;
    static readonly Random Rng = new(31);

    [Fact]
    public void Var_IsPopulationVariance_SampleVar_IsSampleVariance()
    {
        float[] data = [1f, 2f, 3f, 4f, 5f];
        var vector = CreateVector(data);

        vector.Var().ShouldBeCloseTo(2f, Tolerance);
        vector.SampleVar().ShouldBeCloseTo(2.5f, Tolerance);
        vector.SampleVar().ShouldBeCloseTo(vector.Var() * data.Length / (data.Length - 1), Tolerance);
        vector.SampleStd().ShouldBeCloseTo(MathF.Sqrt(2.5f), Tolerance);
    }

    [Fact]
    public void VectorInt_VarAndSampleVar_MatchKnownDataset()
    {
        int[] data = [2, 4, 4, 4, 5, 5, 7, 9];
        var vector = CreateVectorInt(data);

        vector.Var().ShouldBeCloseTo(4f, 1e-3f);
        vector.SampleVar().ShouldBeCloseTo(32f / 7f, 1e-3f);
        vector.SampleStd().ShouldBeCloseTo(MathF.Sqrt(32f / 7f), 1e-3f);
    }

    [Fact]
    public void SampleVar_Empty_ReturnsZero()
    {
        var vector = new Vector(Gpu, 0);
        vector.SampleVar().Should().Be(0f);
        vector.SampleVarX().Should().Be(0f);
    }

    [Fact]
    public void SampleVar_ConstantVector_IsZero()
    {
        var vector = CreateVector(Enumerable.Repeat(3.5f, 8).ToArray());
        vector.SampleVar().ShouldBeCloseTo(0f, Tolerance);
        vector.SampleVarX().ShouldBeCloseTo(0f, Tolerance);
    }

    [Fact]
    public void SampleVar_TwoElements_MatchesHandCalculation()
    {
        var vector = CreateVector([10f, 20f]);
        vector.SampleVar().ShouldBeCloseTo(50f, Tolerance);
        vector.Var().ShouldBeCloseTo(25f, Tolerance);
    }

    [Fact]
    public void SampleVar_SingleElement_ThrowsDivideByZero()
    {
        var vector = CreateVector([42f]);
        vector.Invoking(v => v.SampleVar()).Should().Throw<DivideByZeroException>();
        vector.Invoking(v => v.SampleVarX()).Should().Throw<DivideByZeroException>();
    }

    [Fact]
    public void Mean_HasNoSamplePopulationSplit()
    {
        float[] data = [1f, 2f, 3f, 4f, 5f];
        var vector = CreateVector(data);

        float expected = data.Sum() / data.Length;
        vector.Mean().Should().Be(expected);
        vector.MeanX().ShouldBeCloseTo(expected, Tolerance);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(16)]
    [InlineData(1024)]
    public void SampleVar_MatchesSampleVarX(int length)
    {
        float[] data = RandomData(length);
        var vector = CreateVector(data);

        vector.SampleVar().ShouldBeCloseTo(vector.SampleVarX(), Tolerance);
        vector.SampleStd().ShouldBeCloseTo(vector.SampleStdX(), Tolerance);
        vector.SampleVar().ShouldBeCloseTo(
            vector.Var() * length / (length - 1),
            Tolerance);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(16)]
    [InlineData(256)]
    public void VectorInt_SampleVar_MatchesSampleVarX(int length)
    {
        int[] data = RandomIntData(length);
        var vector = CreateVectorInt(data);

        vector.SampleVar().ShouldBeCloseTo(vector.SampleVarX(), Tolerance);
        vector.SampleStd().ShouldBeCloseTo(vector.SampleStdX(), Tolerance);
    }

    [Fact]
    public void Var_LargeSpread_CpuAndGpuMayDifferWithinFloat32()
    {
        float[] data = [1f, 324f, 312f, 5645f, 756f, 890f, 123f, 456f, 789f, 12f, 198f];
        var vector = CreateVector(data);

        float cpu = vector.Var();
        float gpu = vector.VarX();
        MathF.Abs(cpu - gpu).Should().BeLessThan(1f);
        cpu.ShouldBeCloseTo(2_372_909.25f, 0.5f);

        vector.SampleVar().ShouldBeCloseTo(cpu * data.Length / (data.Length - 1), 1f);
        vector.SampleVarX().ShouldBeCloseTo(vector.SampleVar(), 1f);
    }

    static float[] RandomData(int length)
    {
        float[] data = new float[length];
        for (int i = 0; i < length; i++)
            data[i] = (float)(Rng.NextDouble() * 20 - 10);
        return data;
    }

    static int[] RandomIntData(int length)
    {
        int[] data = new int[length];
        for (int i = 0; i < length; i++)
            data[i] = Rng.Next(-50, 50);
        return data;
    }
}
