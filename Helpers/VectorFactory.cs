namespace BAVCL.Tests.Helpers;

public static class VectorFactory
{
    public static readonly float[] Small1D = [1f, 2f, 3f, 4f, 5f];

    public static readonly float[] MixedSigns = [1f, -2f, 3f, -4f, 5f];

    public static readonly float[] Matrix3x5 =
    [
        1, 2, 3, 4, 5,
        6, 7, 8, 9, 10,
        11, 12, 13, 14, 15
    ];

    public static readonly float[] Vector3Data = [1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f];

    public static readonly float[] EdgeNaNInf = [float.NaN, float.PositiveInfinity, float.NegativeInfinity, 0f];

    public static readonly float[] PositiveOnly = [1f, 2f, 3f, 4f, 5f];

    public static float[] Sequential(int count, float start = 0f, float step = 1f)
    {
        var result = new float[count];
        for (int i = 0; i < count; i++)
            result[i] = start + i * step;
        return result;
    }

    public static IEnumerable<object[]> Small1DData() =>
        [[Small1D]];

    public static IEnumerable<object[]> Matrix3x5Data() =>
        [[Matrix3x5]];
}
