using TechTalk.SpecFlow.Assist;

namespace BAVCL.Tests.Models;
public class VectorProperties
{
    public int Columns { get; set; }
    public int Rows { get; set; }
    public int Length { get; set; }
    public uint LiveCount { get; set; }
    public long MemorySize { get; set; }

    public void Matches(Vector vector)
    {
        vector.Columns.Should().Be(Columns);
        vector.RowCount().Should().Be(Rows);
        vector.Length.Should().Be(Length);
        vector.LiveCount.Should().Be(LiveCount);
        vector.MemorySize.Should().Be(MemorySize);
    }

    [StepArgumentTransformation]
    public VectorProperties TableToVectorProperties(Table table) => table.CreateInstance<VectorProperties>();
}
