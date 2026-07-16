namespace BAVCL.Tests.Helpers;

/// <summary>
/// NumPy-style broadcasting and matmul references for row-major flat arrays.
/// </summary>
public static class BroadcastReference
{
    public static int[] BroadcastShapes(int[] shapeA, int[] shapeB)
    {
        int rank = Math.Max(shapeA.Length, shapeB.Length);
        var paddedA = PadShape(shapeA, rank);
        var paddedB = PadShape(shapeB, rank);
        var result = new int[rank];

        for (int i = 0; i < rank; i++)
        {
            int a = paddedA[i];
            int b = paddedB[i];
            if (a == b)
                result[i] = a;
            else if (a == 1)
                result[i] = b;
            else if (b == 1)
                result[i] = a;
            else
                throw new ArgumentException($"Incompatible broadcast shapes: [{string.Join(", ", shapeA)}] vs [{string.Join(", ", shapeB)}]");
        }

        return result;
    }

    public static bool CanBroadcast(int[] shapeA, int[] shapeB)
    {
        try
        {
            BroadcastShapes(shapeA, shapeB);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    public static float[] Binary(
        float[] a, int[] shapeA,
        float[] b, int[] shapeB,
        Func<float, float, float> op)
    {
        var outShape = BroadcastShapes(shapeA, shapeB);
        int outLength = outShape.Aggregate(1, (x, y) => x * y);
        var result = new float[outLength];

        ForEachIndex(outShape, indices =>
        {
            int outFlat = ToFlat(outShape, indices);
            int aFlat = BroadcastFlatIndex(indices, shapeA, outShape);
            int bFlat = BroadcastFlatIndex(indices, shapeB, outShape);
            result[outFlat] = op(a[aFlat], b[bFlat]);
        });

        return result;
    }

    public static float[] Add(float[] a, int[] shapeA, float[] b, int[] shapeB) =>
        Binary(a, shapeA, b, shapeB, (x, y) => x + y);

    public static float[] Subtract(float[] a, int[] shapeA, float[] b, int[] shapeB) =>
        Binary(a, shapeA, b, shapeB, (x, y) => x - y);

    public static float[] Multiply(float[] a, int[] shapeA, float[] b, int[] shapeB) =>
        Binary(a, shapeA, b, shapeB, (x, y) => x * y);

    public static float[] Divide(float[] a, int[] shapeA, float[] b, int[] shapeB) =>
        Binary(a, shapeA, b, shapeB, (x, y) => x / y);

    public static float[] Pow(float[] a, int[] shapeA, float[] b, int[] shapeB) =>
        Binary(a, shapeA, b, shapeB, (x, y) => MathF.Pow(x, y));

    public static float[] Scale(float[] a, int[] shape, float scalar) =>
        Binary(a, shape, [scalar], [], (x, y) => x * y);

    public static float[] AddScalar(float[] a, int[] shape, float scalar) =>
        Binary(a, shape, [scalar], [], (x, y) => x + y);

    public static float[] SubtractScalar(float[] a, int[] shape, float scalar) =>
        Binary(a, shape, [scalar], [], (x, y) => x - y);

    public static float[] ScalarSubtract(float scalar, float[] a, int[] shape) =>
        Binary([scalar], [], a, shape, (x, y) => x - y);

    public static float[] ScalarDivide(float scalar, float[] a, int[] shape) =>
        Binary([scalar], [], a, shape, (x, y) => x / y);

    public static float[] ScalarPow(float scalar, float[] a, int[] shape) =>
        Binary([scalar], [], a, shape, (x, y) => MathF.Pow(x, y));

    public static float[] PowScalar(float[] a, int[] shape, float scalar) =>
        Binary(a, shape, [scalar], [], (x, y) => MathF.Pow(x, y));

    /// <summary>NumPy dot / matmul for 2D × 2D row-major arrays.</summary>
    public static float[] MatMul(float[] a, int rowsA, int colsA, float[] b, int rowsB, int colsB)
    {
        if (colsA != rowsB)
            throw new ArgumentException($"Inner dimensions must match: ({rowsA},{colsA}) · ({rowsB},{colsB})");

        var result = new float[rowsA * colsB];
        for (int r = 0; r < rowsA; r++)
        {
            for (int c = 0; c < colsB; c++)
            {
                float sum = 0;
                for (int k = 0; k < colsA; k++)
                    sum += a[r * colsA + k] * b[k * colsB + c];
                result[r * colsB + c] = sum;
            }
        }

        return result;
    }

    /// <summary>(M×K) · (K,) → (M,)</summary>
    public static float[] MatMulVector(float[] a, int rowsA, int colsA, float[] b)
    {
        if (colsA != b.Length)
            throw new ArgumentException("Inner dimension must match vector length");

        var result = new float[rowsA];
        for (int r = 0; r < rowsA; r++)
        {
            float sum = 0;
            for (int k = 0; k < colsA; k++)
                sum += a[r * colsA + k] * b[k];
            result[r] = sum;
        }

        return result;
    }

    /// <summary>(K,) · (K×N) → (N,)</summary>
    public static float[] VectorMatMul(float[] a, float[] b, int rowsB, int colsB)
    {
        if (a.Length != rowsB)
            throw new ArgumentException("Vector length must match matrix rows");

        var result = new float[colsB];
        for (int c = 0; c < colsB; c++)
        {
            float sum = 0;
            for (int k = 0; k < rowsB; k++)
                sum += a[k] * b[k * colsB + c];
            result[c] = sum;
        }

        return result;
    }

    public static float InnerProduct(float[] a, float[] b)
    {
        if (a.Length != b.Length)
            throw new ArgumentException("Inner product requires equal lengths");

        float sum = 0;
        for (int i = 0; i < a.Length; i++)
            sum += a[i] * b[i];
        return sum;
    }

    public static float[] SequentialData(int length, float start = 1f, float step = 1f)
    {
        var data = new float[length];
        for (int i = 0; i < length; i++)
            data[i] = start + i * step;
        return data;
    }

    public static float[] SequentialData(int rows, int cols, float start = 1f, float step = 1f) =>
        SequentialData(rows * cols, start, step);

    static int[] PadShape(int[] shape, int rank)
    {
        var padded = new int[rank];
        int offset = rank - shape.Length;
        for (int i = 0; i < offset; i++)
            padded[i] = 1;
        for (int i = 0; i < shape.Length; i++)
            padded[i + offset] = shape[i];
        return padded;
    }

    static void ForEachIndex(int[] shape, Action<int[]> callback)
    {
        var indices = new int[shape.Length];
        Recurse(0);

        void Recurse(int dim)
        {
            if (dim == shape.Length)
            {
                callback((int[])indices.Clone());
                return;
            }

            for (int i = 0; i < shape[dim]; i++)
            {
                indices[dim] = i;
                Recurse(dim + 1);
            }
        }
    }

    static int ToFlat(int[] shape, int[] indices)
    {
        int flat = 0;
        for (int i = 0; i < shape.Length; i++)
            flat = flat * shape[i] + indices[i];
        return flat;
    }

    static int BroadcastFlatIndex(int[] outIndices, int[] shape, int[] outShape)
    {
        int rank = outShape.Length;
        var padded = PadShape(shape, rank);
        int flat = 0;
        for (int i = 0; i < rank; i++)
        {
            int dim = padded[i];
            int idx = dim == 1 ? 0 : outIndices[i];
            flat = flat * dim + idx;
        }

        return flat;
    }
}
