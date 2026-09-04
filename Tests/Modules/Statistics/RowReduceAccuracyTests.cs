using BAVCL.Core;
using BAVCL.GpuAlgorithms;
using BAVCL.Modules.Statistics;
using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Modules.Statistics;

[Collection("GpuSerial")]
public class RowReduceAccuracyTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
    [Theory]
    [InlineData(Operations.multiply)]
    [InlineData(Operations.add)]
    public void WideCols_MatchCpuErrorCorrected(Operations op)
    {
        const int rows = 64;
        const int cols = 10_000;
        float[] coeff = Enumerable.Repeat(0.1f, cols).ToArray();
        float[] matrix = Enumerable.Repeat(0.1f, rows * cols).ToArray();
        var vector = CreateVector(coeff);
        var mat = CreateVector(matrix, cols);

        float[] expected = CpuSimdReduce.RowReduceFloatCompensated(coeff, matrix, rows, cols, op);
        float[] actual = SyncValues(RowReduceAlgorithms.Reduce(vector, mat, op));

        actual.ShouldBeCloseTo(expected, 0.1f);
    }

    [Theory]
    [InlineData(Operations.multiply)]
    [InlineData(Operations.add)]
    [InlineData(Operations.subtract)]
    [InlineData(Operations.flipSubtract)]
    [InlineData(Operations.divide)]
    [InlineData(Operations.flipDivide)]
    [InlineData(Operations.differenceSquared)]
    public void WideCols_AllOps_MatchCpuErrorCorrected(Operations op)
    {
        const int rows = 8;
        const int cols = 512;
        float[] coeff = BuildSequential(cols, 1.05f, 0.01f);
        float[] matrix = BuildSequential(rows * cols, 0.5f, 0.03f);
        var vector = CreateVector(coeff);
        var mat = CreateVector(matrix, cols);

        float[] expected = CpuSimdReduce.RowReduceFloatCompensated(coeff, matrix, rows, cols, op);
        float[] actual = SyncValues(RowReduceAlgorithms.Reduce(vector, mat, op));

        actual.ShouldBeCloseTo(expected, 1e-3f);
    }

    [Fact]
    public void Distance_WideCols_MatchesCpuErrorCorrected()
    {
        const int rows = 8;
        const int cols = 512;
        float[] coeff = BuildSequential(cols, 1.05f, 0.01f);
        float[] matrix = BuildSequential(rows * cols, 0.5f, 0.03f);
        var vector = CreateVector(coeff);
        var mat = CreateVector(matrix, cols);

        float[] expected = CpuSimdReduce.RowReduceFloatCompensated(coeff, matrix, rows, cols, Operations.distance);
        float[] actual = SyncValues(RowReduceAlgorithms.Reduce(vector, mat, Operations.distance));

        actual.ShouldBeCloseTo(expected, 1e-3f);
    }

    [Fact]
    public void CrossX_1Dx2D_MatchesRowReduceMultiply()
    {
        int rows = 4;
        int cols = 16;
        float[] coeff = BuildSequential(cols, 1f);
        float[] matrix = BuildSequential(rows * cols, 2f, 0.1f);
        var vector = CreateVector(coeff);
        var mat = CreateVector(matrix, cols);

        float[] cross = SyncValues(vector.CrossX(mat));
        float[] reduce = SyncValues(RowReduceAlgorithms.Reduce(vector, mat, Operations.multiply));

        cross.ShouldBeCloseTo(reduce, 1e-4f);
    }

    static float[] BuildSequential(int length, float start, float step = 1f)
    {
        float[] data = new float[length];
        for (int i = 0; i < length; i++)
            data[i] = start + i * step;
        return data;
    }
}
