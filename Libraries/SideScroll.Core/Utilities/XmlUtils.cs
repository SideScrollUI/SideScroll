using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Xml;

namespace SideScroll.Core.Utilities;

public static class XmlUtils
{
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
