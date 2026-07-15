namespace BAVCL.Tests.Helpers;

public static class CpuReference
{
    public static float[] Add(float[] a, float[] b)
    {
        var result = new float[a.Length];
        for (int i = 0; i < a.Length; i++)
            result[i] = a[i] + b[i];
        return result;
    }

    public static float[] Subtract(float[] a, float[] b)
    {
        var result = new float[a.Length];
        for (int i = 0; i < a.Length; i++)
            result[i] = a[i] - b[i];
        return result;
    }

    public static float[] Multiply(float[] a, float[] b)
    {
        var result = new float[a.Length];
        for (int i = 0; i < a.Length; i++)
            result[i] = a[i] * b[i];
        return result;
    }

    public static float[] Divide(float[] a, float[] b)
    {
        var result = new float[a.Length];
        for (int i = 0; i < a.Length; i++)
            result[i] = a[i] / b[i];
        return result;
    }

    public static float[] Pow(float[] a, float[] b)
    {
        var result = new float[a.Length];
        for (int i = 0; i < a.Length; i++)
            result[i] = MathF.Pow(a[i], b[i]);
        return result;
    }

    public static float[] Scale(float[] a, float scalar)
    {
        var result = new float[a.Length];
        for (int i = 0; i < a.Length; i++)
            result[i] = a[i] * scalar;
        return result;
    }

    public static float[] Abs(float[] a)
    {
        var result = new float[a.Length];
        for (int i = 0; i < a.Length; i++)
            result[i] = MathF.Abs(a[i]);
        return result;
    }

    public static float[] Reverse(float[] a)
    {
        var result = new float[a.Length];
        Array.Copy(a, result, a.Length);
        Array.Reverse(result);
        return result;
    }

    public static float Sum(float[] a)
    {
        float sum = 0;
        foreach (var v in a)
            sum += v;
        return sum;
    }

    public static float Mean(float[] a) => Sum(a) / a.Length;

    public static float[] Transpose(float[] values, int columns)
    {
        int rows = values.Length / columns;
        var result = new float[values.Length];
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < columns; c++)
                result[c * rows + r] = values[r * columns + c];
        return result;
    }

    public static float Dot(float[] a, float[] b)
    {
        float sum = 0;
        for (int i = 0; i < a.Length; i++)
            sum += a[i] * b[i];
        return sum;
    }
}
