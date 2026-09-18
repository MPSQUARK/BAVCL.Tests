using System.Text;
using System.Text.RegularExpressions;

namespace BAVCL.Benchmarks.Viewer.Services;

public static partial class DisplayFormatting
{
	public static string ReadableName(string name)
	{
		if (string.IsNullOrWhiteSpace(name))
			return name;

		if (name.Contains('_'))
			return name.Replace('_', ' ');

		if (MixedCaseWordRe().IsMatch(name))
			return name;

		return SplitPascalCase(name);
	}

	static string SplitPascalCase(string text)
	{
		var sb = new StringBuilder(text.Length + 8);
		for (int i = 0; i < text.Length; i++)
		{
			char c = text[i];
			if (i > 0 && char.IsUpper(c) && char.IsLower(text[i - 1]))
				sb.Append(' ');
			sb.Append(c);
		}

		return sb.ToString();
	}

	[GeneratedRegex("[a-z]")]
	private static partial Regex MixedCaseWordRe();
}
