namespace BAVCL.Tests.Models;

[Binding]
public class VectorProperties
{
	public int? Columns { get; set; }
	public int? Rows { get; set; }
	public int? Length { get; set; }
	public uint? LiveCount { get; set; }
	public long? MemorySize { get; set; }

	public void Matches(Vector vector)
	{
		if (Columns.HasValue)
			vector.Columns.Should().Be(Columns);
		if (Rows.HasValue)
			vector.Rows.Should().Be(Rows);
		if (Length.HasValue)
			vector.Length.Should().Be(Length);
		if (LiveCount.HasValue)
			vector.LiveCount.Should().Be(LiveCount);
		if (MemorySize.HasValue)
			vector.MemorySize.Should().Be(MemorySize);
	}

	[StepArgumentTransformation]
	public VectorProperties TableToVectorProperties(Table table) => table.CreateInstance<VectorProperties>();
}
