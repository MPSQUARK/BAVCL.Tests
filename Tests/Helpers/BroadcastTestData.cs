namespace BAVCL.Tests.Helpers;

public static class BroadcastTestData
{
    public static IEnumerable<object[]> CompatibleBroadcastPairs()
    {
        foreach (var m in new[] { 2, 3 })
        foreach (var n in new[] { 2, 3, 4 })
        {
            yield return Pack(
                [m, n], BroadcastReference.SequentialData(m, n, 10f, 1f),
                [1, n], BroadcastReference.SequentialData(1, n, 1f, 1f));

            yield return Pack(
                [m, n], BroadcastReference.SequentialData(m, n, 10f, 1f),
                m == n ? [1, n] : [n], BroadcastReference.SequentialData(n, 1f, 1f));

            yield return Pack(
                [1, n], BroadcastReference.SequentialData(1, n, 1f, 1f),
                [m, n], BroadcastReference.SequentialData(m, n, 10f, 1f));

            yield return Pack(
                [m, n], BroadcastReference.SequentialData(m, n, 10f, 1f),
                [m, 1], BroadcastReference.SequentialData(m, 1, 1f, 1f));

            yield return Pack(
                [m, 1], BroadcastReference.SequentialData(m, 1, 1f, 1f),
                [m, n], BroadcastReference.SequentialData(m, n, 10f, 1f));
        }

        yield return Pack(
            [2, 3], BroadcastReference.SequentialData(2, 3, 5f, 1f),
            [2, 3], BroadcastReference.SequentialData(2, 3, 1f, 1f));

        yield return Pack(
            [3], BroadcastReference.SequentialData(3, 1f, 1f),
            [2, 3], BroadcastReference.SequentialData(2, 3, 10f, 1f));
    }

    public static IEnumerable<object[]> CompatibleBroadcastOps() =>
        from pair in CompatibleBroadcastPairs()
        from op in new[] { "add", "sub", "mul", "div", "pow" }
        select new object[] { pair[0], pair[1], pair[2], pair[3], op };

    public static IEnumerable<object[]> IncompatibleBroadcastPairs()
    {
        yield return Pack(
            [3, 5], BroadcastReference.SequentialData(3, 5),
            [2, 5], BroadcastReference.SequentialData(2, 5));

        yield return Pack(
            [3, 5], BroadcastReference.SequentialData(3, 5),
            [3, 4], BroadcastReference.SequentialData(3, 4));

        yield return Pack(
            [2, 3], BroadcastReference.SequentialData(2, 3),
            [6], BroadcastReference.SequentialData(6));

        yield return Pack(
            [4], BroadcastReference.SequentialData(4),
            [2, 3], BroadcastReference.SequentialData(2, 3));

        yield return Pack(
            [2, 2], BroadcastReference.SequentialData(2, 2),
            [3, 3], BroadcastReference.SequentialData(3, 3));
    }

    static object[] Pack(int[] shapeA, float[] dataA, int[] shapeB, float[] dataB) =>
        [shapeA, dataA, shapeB, dataB];

    public static float[] ExpectedBroadcast(
        string op, float[] dataA, int[] shapeA, float[] dataB, int[] shapeB) =>
        op switch
        {
            "add" => BroadcastReference.Add(dataA, shapeA, dataB, shapeB),
            "sub" => BroadcastReference.Subtract(dataA, shapeA, dataB, shapeB),
            "mul" => BroadcastReference.Multiply(dataA, shapeA, dataB, shapeB),
            "div" => BroadcastReference.Divide(dataA, shapeA, dataB, shapeB),
            "pow" => BroadcastReference.Pow(dataA, shapeA, dataB, shapeB),
            _ => throw new ArgumentException(op)
        };

    public static Vector ApplyOp(Vector a, Vector b, string op) =>
        op switch
        {
            "add" => a + b,
            "sub" => a - b,
            "mul" => a * b,
            "div" => a / b,
            "pow" => a ^ b,
            _ => throw new ArgumentException(op)
        };

    public static Vector ApplyOp(Vector a, Vector b, string op, bool reverse) =>
        reverse ? ApplyOp(b, a, ReverseOp(op)) : ApplyOp(a, b, op);

    static string ReverseOp(string op) =>
        op switch
        {
            "add" => "add",
            "sub" => "revsub",
            "mul" => "mul",
            "div" => "revdiv",
            "pow" => "revpow",
            _ => throw new ArgumentException(op)
        };

    public static float[] ExpectedReverseBroadcast(
        string op, float[] dataA, int[] shapeA, float[] dataB, int[] shapeB) =>
        op switch
        {
            "revsub" => BroadcastReference.Subtract(dataB, shapeB, dataA, shapeA),
            "revdiv" => BroadcastReference.Divide(dataB, shapeB, dataA, shapeA),
            "revpow" => BroadcastReference.Pow(dataB, shapeB, dataA, shapeA),
            _ => ExpectedBroadcast(op, dataA, shapeA, dataB, shapeB)
        };
}
