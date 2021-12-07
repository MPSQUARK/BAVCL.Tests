using Xunit;

namespace BAVCL.Tests.Core
{
    public class VectorTests
    {
        public VectorTests()
        {
            this.gpu = new GPU();
        }

        readonly GPU gpu;

        [Fact]
        public void CachedVectorCreation()
        {
            float[] vals = new float[6] { float.NaN, float.PositiveInfinity, float.NegativeInfinity, 5f, 0.1234f, -0.2434f };
            Vector vector = new(this.gpu, vals, 1, true);

            // Check the properties of the created vector
            Assert.Equal(1, vector.Columns);
            Assert.Equal(1, vector.RowCount());
            Assert.Equal(6, vector.Length);
            Assert.Equal(1, (int)vector.ID);
            Assert.Equal(0, (int)vector.LiveCount);
            Assert.Equal(24, vector.MemorySize);

            // Check immidiate access to 
            Assert.Null(vector.Value);

        }

        [Fact]
        public void UnCachedVectorCreation()
        {
            float[] vals = new float[6] { float.NaN, float.PositiveInfinity, float.NegativeInfinity, 5f, 0.1234f, -0.2434f };
            Vector vector = new(this.gpu, vals, 1, false);

            // Check the properties of the created vector
            Assert.Equal(1, vector.Columns);
            Assert.Equal(1, vector.RowCount());
            Assert.Equal(6, vector.Length);
            Assert.Equal(0, (int)vector.ID);
            Assert.Equal(0, (int)vector.LiveCount);
            Assert.Equal(24, vector.MemorySize);

            // Check immidiate access to 
            Assert.NotNull(vector.Value);

        }



    }

}
