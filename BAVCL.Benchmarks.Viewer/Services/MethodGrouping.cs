namespace BAVCL.Benchmarks.Viewer.Services;

public sealed record MethodFamily(string Name, IReadOnlyList<string> Methods);

public static class MethodGrouping
{
	public static string FamilyName(string method)
	{
		if (method.EndsWith("_Eager", StringComparison.Ordinal))
			return method[..^"_Eager".Length];

		int graphIdx = method.IndexOf("_Graph_", StringComparison.Ordinal);
		if (graphIdx > 0)
			return method[..graphIdx];

		int gpuIdx = method.IndexOf("_Gpu_", StringComparison.OrdinalIgnoreCase);
		if (gpuIdx > 0)
			return method[..gpuIdx];

		return method;
	}

	public static IReadOnlyList<MethodFamily> GroupMethods(IEnumerable<string> methods) =>
		methods
			.Distinct(StringComparer.OrdinalIgnoreCase)
			.GroupBy(FamilyName, StringComparer.OrdinalIgnoreCase)
			.OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase)
			.Select(g => new MethodFamily(g.Key, g.OrderBy(m => m, StringComparer.OrdinalIgnoreCase).ToList()))
			.ToList();

	public static string ShortLabel(string method)
	{
		string family = FamilyName(method);
		if (method.Equals(family, StringComparison.OrdinalIgnoreCase))
			return method;

		return method[family.Length..].TrimStart('_');
	}
}
