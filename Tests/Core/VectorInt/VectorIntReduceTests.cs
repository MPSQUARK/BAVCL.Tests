using BAVCL.Core.Exceptions;
using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Core.VectorIntTests;

public class VectorIntReduceTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
    [Theory]
    [InlineData(Operations.add)]
    [InlineData(Operations.multiply)]
    public void ReduceOP_1DWithMatrix_MatchesRowReduction(Operations op)
    {
        int rows = 2;
        int cols = 3;
        int[] coeff = [1, 1, 1];
        int[] matrix = [10, 11, 12, 13, 14, 15];
        var vector = BavclShapeInt.Create(Gpu, [cols], coeff);
        var mat = BavclShapeInt.Create(Gpu, [rows, cols], matrix);

        var expected = RowReduceReference(coeff, matrix, rows, cols, op);

        var result = vector.ReduceOP(mat, op);

        result.Length.Should().Be(rows);
        result.Columns.Should().Be(1);
        SyncValues(result).Should().Equal(expected);
    }

    [Fact]
    public void ReduceOP_UnequalCoefficientLength_ThrowsLengthMismatch()
    {
        var vector = BavclShapeInt.Create(Gpu, [3], [1, 2, 3]);
        var matrix = BavclShapeInt.Create(Gpu, [2, 4], [1, 2, 3, 4, 5, 6, 7, 8]);

        Action act = () => vector.ReduceOP(matrix, Operations.add);

        act.Should().Throw<LengthMismatchException>();
    }

    [Fact]
    public void ReduceOP_MatrixOnLeft_ThrowsShapeMismatch()
    {
        var vector = BavclShapeInt.Create(Gpu, [3], [1, 2, 3]);
        var matrix = BavclShapeInt.Create(Gpu, [2, 3], [1, 2, 3, 4, 5, 6]);

        Action act = () => matrix.ReduceOP(vector, Operations.add);

        act.Should().Throw<ShapeMismatchException>();
    }

    static int[] RowReduceReference(int[] coeff, int[] matrix, int rows, int cols, Operations op)
    {
        var output = new int[rows];
        for (int r = 0; r < rows; r++)
        {
            int acc = 0;
            for (int c = 0; c < cols; c++)
            {
                int a = coeff[c];
                int b = matrix[r * cols + c];
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
