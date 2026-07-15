using BAVCL.Tests.Helpers;

namespace BAVCL.Tests.Core.VectorTests;

public class VectorBroadcastTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
    [Fact]
    public void OP_VectorMatrixBroadcast_AddsRowToEachMatrixRow()
    {
        var row = CreateVector([1f, 2f, 3f]);
        var matrix = CreateVector(
        [
            10f, 20f, 30f,
            40f, 50f, 60f
        ], columns: 3);

        var result = Vector.OP(matrix, row, Operations.add);

        result.Length.Should().Be(3);
        SyncValues(result).Should().AllSatisfy(v => float.IsFinite(v));
    }
}
