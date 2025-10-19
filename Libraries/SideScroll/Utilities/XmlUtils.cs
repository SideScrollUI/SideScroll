using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Xml;

namespace SideScroll.Utilities;

/// <summary>
/// Provides utilities for working with XML text
/// </summary>
public static class XmlUtils
{
	/// <summary>
	/// Attempts to format XML text with proper indentation
	/// </summary>
	/// <param name="text">The XML text to format</param>
	/// <param name="formatted">The formatted XML text if successful</param>
	/// <returns>True if the text was successfully formatted; otherwise, false</returns>
	public static bool TryFormat(string text, [NotNullWhen(true)] out string? formatted)
	{
		formatted = default;
		text = text.TrimStart();
		if (!text.StartsWith('<')) return false;

		using var memoryStream = new MemoryStream();
		using var writer = new XmlTextWriter(memoryStream, Encoding.Unicode)
		{
			Formatting = Formatting.Indented,
		};
		var document = new XmlDocument();

		try
		{
			document.LoadXml(text);
			document.WriteContentTo(writer);

			writer.Flush();
			memoryStream.Flush();

			memoryStream.Position = 0;

			var streamReader = new StreamReader(memoryStream);

			formatted = streamReader.ReadToEnd();

			return true;
		}
		catch (Exception e)
		{
			Debug.WriteLine(e);
			return false;
		}
	}
}
