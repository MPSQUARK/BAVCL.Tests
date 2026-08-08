using BAVCL.Core;
using BAVCL.Core.Exceptions;
using BAVCL.Modules.Arithmetic;
using BAVCL.Modules.GpuOps;
using BAVCL.Modules.Masking;
using BAVCL.Modules.Statistics;
using BAVCL.Modules.Structural;
using BAVCL.Tests.Helpers;
using BAVCL.Types;

namespace BAVCL.Tests.Core.VectorIntTests;

public class VectorIntTests(GpuTestFixture fixture) : GpuTestBase(fixture)
{
    [Fact]
    public void Constructor_WithValues_SetsDimensions()
    {
        var vector = CreateVectorInt(VectorIntFactory.Small1D);

        vector.Length.Should().Be(5);
        vector.Columns.Should().Be(0);
        SyncValues(vector).Should().Equal(VectorIntFactory.Small1D);
    }

    [Fact]
    public void Copy_CreatesIndependentVector()
    {
        var original = CreateVectorInt(VectorIntFactory.Small1D);
        var copy = original.Copy();

        copy.Equals(original).Should().BeTrue();
        copy.Should().NotBeSameAs(original);
    }

    [Fact]
    public void Equals_ReturnsFalseForDifferentValues()
    {
        var a = CreateVectorInt([1, 2, 3]);
        var b = CreateVectorInt([1, 2, 4]);

        a.Equals(b).Should().BeFalse();
    }

    [Theory]
    [InlineData("add", 1, 2, 3, 10, 20, 30, 11, 22, 33)]
    [InlineData("subtract", 10, 20, 30, 1, 2, 3, 9, 18, 27)]
    [InlineData("multiply", 2, 4, 6, 3, 3, 3, 6, 12, 18)]
    public void Operators_VectorVector_MatchExpected(
        string op, int a0, int a1, int a2, int b0, int b1, int b2, int e0, int e1, int e2)
    {
        var a = CreateVectorInt([a0, a1, a2]);
        var b = CreateVectorInt([b0, b1, b2]);

        VectorInt result = op switch
        {
            "add" => a + b,
            "subtract" => a - b,
            "multiply" => a * b,
            _ => throw new ArgumentOutOfRangeException(nameof(op)),
        };

        SyncValues(result).Should().Equal([e0, e1, e2]);
    }

    [Fact]
    public void Divide_TruncatesTowardZero()
    {
        var a = CreateVectorInt([7, -7, 8]);
        var b = CreateVectorInt([2, 2, -3]);

        SyncValues(a / b).Should().Equal([3, -3, -2]);
    }

    [Fact]
    public void UnsupportedOperation_ThrowsForInvalidReduceOp()
    {
        var coeffs = CreateVectorInt([1, 1]);
        var matrix = CreateVectorInt([1, 2, 3, 4, 5, 6], columns: 2);

        Action reduce = () => coeffs.ReduceOPX(matrix, Operations.bitwiseXor);
        reduce.Should().Throw<UnsupportedOperationException>();
    }

    [Fact]
    public void UnsupportedOperation_ThrowsForUnknownElementWiseOp()
    {
        var a = CreateVectorInt([1, 2, 3]);
        var b = CreateVectorInt([4, 5, 6]);

        Action op = () => a.OP(b, Operations.pow);
        op.Should().Throw<InvalidOperationOnTypeException>();
    }

    [Fact]
    public void Modulo_MatchesCSharpSemantics()
    {
        var a = CreateVectorInt([7, -7, 8]);
        var b = CreateVectorInt([3, 3, -3]);

        SyncValues(a % b).Should().Equal([7 % 3, -7 % 3, 8 % -3]);
        SyncValues(a % 3).Should().Equal([7 % 3, -7 % 3, 8 % 3]);
        SyncValues(10 % a).Should().Equal([10 % 7, 10 % -7, 10 % 8]);
    }

    [Fact]
    public void Xor_ScalarAndVector()
    {
        var a = CreateVectorInt([1, 2, 3]);
        var b = CreateVectorInt([3, 1, 2]);

        SyncValues(a ^ b).Should().Equal([2, 3, 1]);
        SyncValues(a ^ 1).Should().Equal([0, 3, 2]);
    }

    [Fact]
    public void BitwiseAnd_ScalarAndVector()
    {
        var a = CreateVectorInt([5, 6, 7]);
        var b = CreateVectorInt([3, 3, 3]);

        SyncValues(a & b).Should().Equal([1, 2, 3]);
        SyncValues(a & 2).Should().Equal([0, 2, 2]);
    }

    [Fact]
    public void IPOP_Add_MutatesInPlace()
    {
        var vector = CreateVectorInt([1, 2, 3]);
        var other = CreateVectorInt([10, 20, 30]);

        vector += other;
        vector.SyncCPU();

        vector.Value.Should().Equal([11, 22, 33]);
    }

    [Fact]
    public void IPOP_ScalarMultiply_MutatesInPlace()
    {
        var vector = CreateVectorInt([2, 4, 6]);

        vector *= 3;
        vector.SyncCPU();

        vector.Value.Should().Equal([6, 12, 18]);
    }

    [Fact]
    public void Broadcast_RowVectorPlusScalar_MatchesExpected()
    {
        var vector = CreateVectorInt([1, 2, 3, 4], columns: 2);

        SyncValues(vector + 10).Should().Equal([11, 12, 13, 14]);
    }

    [Fact]
    public void LeftShift_Scalar_ShiftsEveryElement()
    {
        var vector = CreateVectorInt([1, 2, 3]);

        SyncValues(vector << 2).Should().Equal([4, 8, 12]);
    }

    [Fact]
    public void RightShift_Scalar_IsArithmeticSignExtending()
    {
        var vector = CreateVectorInt([8, -8]);

        SyncValues(vector >> 1).Should().Equal([4, -4]);
    }

    [Fact]
    public void LeftShift_VectorCounts_Broadcasts()
    {
        var vector = CreateVectorInt([1, 1, 1]);
        var counts = CreateVectorInt([1, 2, 3]);

        SyncValues(vector << counts).Should().Equal([2, 4, 8]);
    }

    [Fact]
    public void Shift_EdgeCounts_MatchCSharpMasking()
    {
        var vector = CreateVectorInt([1, 1, 1]);

        SyncValues(vector << 0).Should().Equal([1 << 0, 1 << 0, 1 << 0]);
        SyncValues(vector << 31).Should().Equal([1 << 31, 1 << 31, 1 << 31]);
        SyncValues(vector << 32).Should().Equal([1 << 32, 1 << 32, 1 << 32]);
    }

    [Fact]
    public void InPlaceShift_MutatesInPlace()
    {
        var vector = CreateVectorInt([1, 2, 3]);

        vector <<= 1;
        vector.SyncCPU();

        vector.Value.Should().Equal([2, 4, 6]);

        vector >>= 1;
        vector.SyncCPU();

        vector.Value.Should().Equal([1, 2, 3]);
    }

    [Fact]
    public void Compare_ProducesExpectedMask()
    {
        var vector = CreateVectorInt([1, 2, 3, 4]);
        var other = CreateVectorInt([0, 2, 5, 4]);

        SyncMaskBits(vector > other).Should().Equal([true, false, false, false]);
        SyncMaskBits(vector.CompareEqualsX(other)).Should().Equal([false, true, false, true]);
    }

    [Fact]
    public void Mask_AndOperator_AppliesFillValue()
    {
        var vector = CreateVectorInt([1, 2, 3, 4]);
        var mask = CreateMask([true, false, true, false]);

        SyncValues(vector & mask).Should().Equal([1, 0, 3, 0]);
        SyncValues(vector & (mask, -1)).Should().Equal([1, -1, 3, -1]);
        SyncValues(vector.MaskX(mask, -1)).Should().Equal([1, -1, 3, -1]);
    }

    [Fact]
    public void Filter_CompactsToSurvivingLanes()
    {
        var vector = CreateVectorInt([10, 20, 30, 40]);
        var mask = CreateMask([true, false, true, false]);

        SyncValues(vector | mask).Should().Equal([10, 30]);
        SyncValues(vector.FilterX(mask)).Should().Equal([10, 30]);
        SyncValues(vector[mask]).Should().Equal([10, 30]);
    }

    [Fact]
    public void Partition_SplitsTrueAndFalseLanes()
    {
        var vector = CreateVectorInt([1, 2, 3, 4]);
        var mask = CreateMask([true, false, true, false]);

        (VectorInt trueLanes, VectorInt falseLanes) = vector / mask;

        SyncValues(trueLanes).Should().Equal([1, 3]);
        SyncValues(falseLanes).Should().Equal([2, 4]);
    }

    [Fact]
    public void ExplicitCast_ToVector_ConvertsElementsToFloat()
    {
        var vector = CreateVectorInt([1, -2, 3]);

        Vector asFloat = (Vector)vector;

        SyncValues(asFloat).ShouldBeCloseTo([1f, -2f, 3f]);
    }

    [Fact]
    public void ExplicitCast_ToVectorInt_TruncatesTowardZero()
    {
        var vector = CreateVector([1.9f, -1.9f, 2.1f]);

        VectorInt asInt = (VectorInt)vector;

        SyncValues(asInt).Should().Equal([1, -1, 2]);
    }

    [Fact]
    public void ExplicitCast_ToVectorInt_RejectsNonFinite()
    {
        var vector = CreateVector([1f, float.NaN]);

        Action cast = () => _ = (VectorInt)vector;

        cast.Should().Throw<InvalidCastException>();
    }

    [Fact]
    public void Sum_And_Mean_ComputeExpectedValues()
    {
        var vector = CreateVectorInt([1, 2, 3, 4]);

        vector.Sum().Should().Be(10f);
        vector.Mean().Should().Be(2.5f);
    }

    [Fact]
    public void MinMaxRange_All_ComputeExpectedValues()
    {
        var vector = CreateVectorInt([5, -3, 10, 2]);

        vector.Min().Should().Be(-3);
        vector.Max().Should().Be(10);
        vector.Range().Should().Be(13);
        vector.All().Should().BeTrue();

        var zeros = CreateVectorInt([0, 0]);
        zeros.All().Should().BeFalse();
    }

    [Fact]
    public void Abs_ComputesBitwiseAbsoluteValue()
    {
        var vector = CreateVectorInt([int.MinValue, -2, 3]);

        SyncValues(vector.Abs()).Should().Equal([0, -2 & int.MaxValue, 3]);
        SyncValues(+vector).Should().Equal([0, -2 & int.MaxValue, 3]);
    }

    [Fact]
    public void Negate_HandlesMinValueWithoutException()
    {
        var vector = CreateVectorInt([int.MinValue, 5, -3]);

        SyncValues(-vector).Should().Equal([int.MinValue, -5, 3]);
    }

    [Fact]
    public void AbsX_GpuKernel_MatchesCpuAbs()
    {
        var vector = CreateVectorInt([-2, -3, -4, -5]);

        SyncValues(vector.AbsX()).Should().Equal([-2 & int.MaxValue, -3 & int.MaxValue, -4 & int.MaxValue, -5 & int.MaxValue]);
    }

    [Fact]
    public void Diff_ComputesConsecutiveDifferences()
    {
        var vector = CreateVectorInt([1, 3, 6, 10]);

        SyncValues(vector.DiffX()).Should().Equal([2, 3, 4]);
    }

    [Fact]
    public void Dot_ComputesSumOfProducts()
    {
        var a = CreateVectorInt([1, 2, 3]);
        var b = CreateVectorInt([4, 5, 6]);

        a.Dot(b).Should().Be(32f);
    }

    [Fact]
    public void InvalidOperationOnType_ThrowsForFloatOnlyOp()
    {
        var a = CreateVectorInt([1, 2, 3]);
        var b = CreateVectorInt([4, 5, 6]);

        Action act = () => a.OP(b, Operations.distance);

        act.Should().Throw<InvalidOperationOnTypeException>();
    }

    [Fact]
    public void Factories_ZerosOnesFillArange_ProduceExpectedValues()
    {
        SyncValues(VectorInt.Zeros(Gpu, 3)).Should().Equal([0, 0, 0]);
        SyncValues(VectorInt.Ones(Gpu, 3)).Should().Equal([1, 1, 1]);
        SyncValues(VectorInt.Fill(Gpu, 7, 3)).Should().Equal([7, 7, 7]);
        SyncValues(VectorInt.Arange(Gpu, 0, 5, 1)).Should().Equal([0, 1, 2, 3, 4]);
    }

    [Fact]
    public void MatrixMultiply_ComputesExpectedProduct()
    {
        var a = CreateVectorInt([1, 2, 3, 4], columns: 2);
        var b = CreateVectorInt([5, 6, 7, 8], columns: 2);

        SyncValues(a.MatrixMultiplyX(b)).Should().Equal([19, 22, 43, 50]);
    }

    [Fact]
    public void Reverse_ReversesElementOrder()
    {
        var vector = CreateVectorInt([1, 2, 3, 4]);

        SyncValues(vector.Reverse()).Should().Equal([4, 3, 2, 1]);
    }

    [Fact]
    public void ReverseX_GpuKernel_ReversesElementOrder()
    {
        var vector = CreateVectorInt([1, 2, 3, 4]);

        SyncValues(vector.ReverseX()).Should().Equal([4, 3, 2, 1]);
    }
}
