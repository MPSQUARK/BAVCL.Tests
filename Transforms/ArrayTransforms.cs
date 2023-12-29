namespace BAVCL.Tests;

[Binding]
public class ArrayTransforms
{
	[StepArgumentTransformation]
	public float[] TransformToFloatArray(Table table)
	{
		return table.Rows.Select(row =>
		{
			var value = row[0];
			if (value == "NaN")
				return float.NaN;
			if (value == "Inf")
				return float.PositiveInfinity;
			if (value == "-Inf")
				return float.NegativeInfinity;
			return Convert.ToSingle(value);
		}).ToArray();
	}
	
	
}