namespace BAVCL.Tests.Models;

[Binding]
public class VectorCreator<T> where T : unmanaged
{
	public required string Alias { get; set; }
	public bool IsCached { get; set; }
	public int Columns { get; set; }
	public required T[] Values { get; set; }

	[StepArgumentTransformation]
	public VectorCreator<T> TableToVectorCreator(Table table) =>
		TableRowToVectorCreator(table.Rows.First());

	[StepArgumentTransformation]
	public IEnumerable<VectorCreator<T>> TableToVectorCreatorEnumerable(Table table) =>
		table.Rows.Select(TableRowToVectorCreator);

	public VectorCreator<T> TableRowToVectorCreator(TableRow row)
	{
		var alias = row["Alias"];
		var values = ConvertValues(row["Values"].Split(','));
		var columns = int.Parse(row["Columns"]);
		var isCached = bool.Parse(row["IsCached"]);
		return new VectorCreator<T>
		{
			Alias = alias,
			Values = values,
			Columns = columns,
			IsCached = isCached
		};
	}

	private T[] ConvertValues(string[] values)
	{
		T[] convertedValues = new T[values.Length];
		for (int i = 0; i < values.Length; i++)
		{
			convertedValues[i] = (T)Convert.ChangeType(values[i], typeof(T));
		}
		return convertedValues;
	}
}
