using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Core.VectorTests;

public class VectorScalarBroadcastTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
    [Theory]
    [InlineData(2f)]
    [InlineData(-1.5f)]
    [InlineData(0.5f)]
    public void ScalarOp1D_MatchesBroadcastReference(float scalar)
    {
        var data = BroadcastReference.SequentialData(4, 2f, 1f);
        var shape = new[] { 4 };
        var vector = BavclShape.Create(Gpu, shape, data);

        BavclShape.ShouldMatchNumpyShape(vector + scalar, shape, BroadcastReference.AddScalar(data, shape, scalar));
        BavclShape.ShouldMatchNumpyShape(vector - scalar, shape, BroadcastReference.SubtractScalar(data, shape, scalar));
        BavclShape.ShouldMatchNumpyShape(vector * scalar, shape, BroadcastReference.Scale(data, shape, scalar));
        BavclShape.ShouldMatchNumpyShape(vector / scalar, shape, BroadcastReference.Divide(data, shape, [scalar], []));
        BavclShape.ShouldMatchNumpyShape(vector ^ scalar, shape, BroadcastReference.PowScalar(data, shape, scalar));

        BavclShape.ShouldMatchNumpyShape(scalar - vector, shape, BroadcastReference.ScalarSubtract(scalar, data, shape));
        BavclShape.ShouldMatchNumpyShape(scalar / vector, shape, BroadcastReference.ScalarDivide(scalar, data, shape));
        BavclShape.ShouldMatchNumpyShape(scalar ^ vector, shape, BroadcastReference.ScalarPow(scalar, data, shape));
    }

    [Theory]
    [InlineData(2f)]
    [InlineData(3f)]
    public void ScalarOp2D_MatchesBroadcastReference(float scalar)
    {
        var data = BroadcastReference.SequentialData(2, 3, 10f, 1f);
        var shape = new[] { 2, 3 };
        var matrix = BavclShape.Create(Gpu, shape, data);

        BavclShape.ShouldMatchNumpyShape(matrix + scalar, shape, BroadcastReference.AddScalar(data, shape, scalar));
        BavclShape.ShouldMatchNumpyShape(matrix * scalar, shape, BroadcastReference.Scale(data, shape, scalar));
        BavclShape.ShouldMatchNumpyShape(matrix / scalar, shape, BroadcastReference.Divide(data, shape, [scalar], []));
        BavclShape.ShouldMatchNumpyShape(scalar - matrix, shape, BroadcastReference.ScalarSubtract(scalar, data, shape));
    }

    [Fact]
    public void IPOP_ScalarOn2D_MutatesAllElements()
    {
        var data = BroadcastReference.SequentialData(2, 3, 1f, 1f);
        var shape = new[] { 2, 3 };
        var matrix = BavclShape.Create(Gpu, shape, data);

        matrix.IPOP(2f, Operations.add);
        matrix.SyncCPU();

        matrix.Value.ShouldBeCloseTo(BroadcastReference.AddScalar(data, shape, 2f));
    }

    [Fact]
    public void IPOP_ScalarMultiplyOn1D_MutatesInPlace()
    {
        var data = BroadcastReference.SequentialData(3, 2f, 1f);
        var vector = BavclShape.Create(Gpu, [3], data);

        vector.IPOP(2f, Operations.multiply);
        vector.SyncCPU();

        vector.Value.ShouldBeCloseTo(BroadcastReference.Scale(data, [3], 2f));
    }
}
