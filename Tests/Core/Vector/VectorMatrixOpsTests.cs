using BAVCL.Core.Exceptions;
using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Core.VectorTests;

public class VectorMatrixOpsTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
    [Theory]
    [InlineData(Operations.add)]
    [InlineData(Operations.subtract)]
    [InlineData(Operations.divide)]
    [InlineData(Operations.pow)]
    public void MatrixBinary_SameShape_MatchesElementWise(Operations op)
    {
        var shape = new[] { 2, 3 };
        var aData = BroadcastReference.SequentialData(2, 3, 10f, 1f);
        var bData = BroadcastReference.SequentialData(2, 3, 1f, 1f);
        var a = BavclShape.Create(Gpu, shape, aData);
        var b = BavclShape.Create(Gpu, shape, bData);

        var expected = op switch
        {
            Operations.add => BroadcastReference.Add(aData, shape, bData, shape),
            Operations.subtract => BroadcastReference.Subtract(aData, shape, bData, shape),
            Operations.divide => BroadcastReference.Divide(aData, shape, bData, shape),
            Operations.pow => BroadcastReference.Pow(aData, shape, bData, shape),
            _ => throw new ArgumentOutOfRangeException(nameof(op))
        };

        Vector result = op switch
        {
            Operations.add => a.MatrixAdd(b),
            Operations.subtract => a.MatrixSubtract(b),
            Operations.divide => a.MatrixDivide(b),
            Operations.pow => a.MatrixPow(b),
            _ => throw new ArgumentOutOfRangeException(nameof(op))
        };

        BavclShape.ShouldMatchNumpyShape(result, shape, expected);
    }

    [Fact]
    public void MatrixAdd_DifferentShapes_ThrowsShapeMismatch()
    {
        var a = BavclShape.Create(Gpu, [2, 3], BroadcastReference.SequentialData(2, 3));
        var b = BavclShape.Create(Gpu, [3, 2], BroadcastReference.SequentialData(3, 2));

        Action act = () => a.MatrixAdd(b);

        act.Should().Throw<ShapeMismatchException>();
    }

    [Fact]
    public void MatrixMultiply_AliasMatchesCross()
    {
        var a = BavclShape.Create(Gpu, [2, 3], BroadcastReference.SequentialData(2, 3, 1f, 1f));
        var b = BavclShape.Create(Gpu, [3, 2], BroadcastReference.SequentialData(3, 2, 2f, 0.5f));
        var expected = BroadcastReference.MatMul(
            BroadcastReference.SequentialData(2, 3, 1f, 1f), 2, 3,
            BroadcastReference.SequentialData(3, 2, 2f, 0.5f), 3, 2);

        var cross = a.Cross(b);
        var matmul = a.MatrixMultiply(b);

        BavclShape.ShouldMatchNumpyShape(cross, [2, 2], expected);
        BavclShape.ShouldMatchNumpyShape(matmul, [2, 2], expected);
    }

    [Fact]
    public void MatrixAdd_On1DVector_ThrowsShapeMismatch()
    {
        var vector = BavclShape.Create(Gpu, [3], BroadcastReference.SequentialData(3));
        var matrix = BavclShape.Create(Gpu, [2, 3], BroadcastReference.SequentialData(2, 3));

        Action act = () => vector.MatrixAdd(matrix);

        act.Should().Throw<ShapeMismatchException>();
    }
}
