using BAVCL.Core.Exceptions;
using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Core.VectorTests;

/// <summary>
/// ReduceOP uses reduceRowOpKernel: one output per matrix row, not NumPy broadcast.
/// </summary>
public class VectorReduceTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
    [Theory]
    [InlineData(Operations.add)]
    [InlineData(Operations.multiply)]
    public void ReduceOP_1DWithMatrix_MatchesRowReduction(Operations op)
    {
        int rows = 2;
        int cols = 3;
        var coeff = BroadcastReference.SequentialData(cols, 1f, 1f);
        var matrix = BroadcastReference.SequentialData(rows, cols, 10f, 1f);
        var vector = BavclShape.Create(Gpu, [cols], coeff);
        var mat = BavclShape.Create(Gpu, [rows, cols], matrix);

        var expected = RowReduceReference(coeff, matrix, rows, cols, op);

        var result = vector.ReduceOP(mat, op);

        result.Length.Should().Be(rows);
        result.Columns.Should().Be(1);
        SyncValues(result).ShouldBeCloseTo(expected);
    }

    [Fact]
    public void ReduceOP_UnequalCoefficientLength_ThrowsLengthMismatch()
    {
        var vector = BavclShape.Create(Gpu, [3], BroadcastReference.SequentialData(3));
        var matrix = BavclShape.Create(Gpu, [2, 4], BroadcastReference.SequentialData(2, 4));

        Action act = () => vector.ReduceOP(matrix, Operations.add);

        act.Should().Throw<LengthMismatchException>();
    }

    [Fact]
    public void ReduceOP_MatrixOnLeft_ThrowsShapeMismatch()
    {
        var vector = BavclShape.Create(Gpu, [3], BroadcastReference.SequentialData(3));
        var matrix = BavclShape.Create(Gpu, [2, 3], BroadcastReference.SequentialData(2, 3));

        Action act = () => matrix.ReduceOP(vector, Operations.add);

        act.Should().Throw<ShapeMismatchException>();
    }

    [Fact]
    public void Operator_NoLongerUsesRowReduction()
    {
        var coeff = BavclShape.Create(Gpu, [3], BroadcastReference.SequentialData(3, 1f, 1f));
        var matrix = BavclShape.Create(Gpu, [2, 3], BroadcastReference.SequentialData(2, 3, 10f, 1f));

        var broadcast = coeff + matrix;
        broadcast.Length.Should().Be(6);

        var reduced = coeff.ReduceOP(matrix, Operations.add);
        reduced.Length.Should().Be(2);
    }

    static float[] RowReduceReference(float[] coeff, float[] matrix, int rows, int cols, Operations op)
    {
        var output = new float[rows];
        for (int r = 0; r < rows; r++)
        {
            float acc = 0f;
            for (int c = 0; c < cols; c++)
            {
                float a = coeff[c];
                float b = matrix[r * cols + c];
                acc += op switch
                {
                    Operations.add => a + b,
                    Operations.multiply => a * b,
                    _ => throw new ArgumentOutOfRangeException(nameof(op))
                };
            }
            output[r] = acc;
        }
        return output;
    }
}
