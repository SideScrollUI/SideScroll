using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Highlighting.Xshd;
using SideScroll.Resources;
using System.Xml;

namespace SideScroll.Avalonia.Controls.TextEditor;

/// <summary>
/// A custom syntax highlighting format that can be registered with <see cref="TabAvaloniaEdit.Highlighters"/>,
/// matched either by file extension or by probing the text contents
/// </summary>
/// <remarks>
/// Register formats once at startup. Every <see cref="TabAvaloniaEdit"/> checks the registered
/// highlighters, so text is highlighted wherever it's shown, including generic string and file tabs
/// </remarks>
public class TextHighlighter
{
	/// <summary>Gets the file extensions this format applies to, including the leading dot.</summary>
	public string[] Extensions { get; init; } = [];

	/// <summary>Gets an optional test that detects this format from the text contents.</summary>
	public Func<string, bool>? Probe { get; init; }

	/// <summary>Gets the action that assigns the highlighting definition and its theme colors to an editor.</summary>
	/// <remarks>Called again whenever the theme changes, so the colors have to be reassigned each time</remarks>
	public required Action<TabAvaloniaEdit> Apply { get; init; }

	/// <summary>Returns whether the file extension or the text contents match this format.</summary>
	public bool Matches(string? extension, string text)
	{
		if (extension != null && Extensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
			return true;

		return Probe?.Invoke(text) == true;
	}
}

/// <summary>Loads AvaloniaEdit .xshd syntax highlighting definitions from embedded resources.</summary>
public static class TextHighlighting
{
	/// <summary>Loads an .xshd definition and registers it with the <see cref="HighlightingManager"/>.</summary>
	/// <remarks>Registering the same name again replaces the previous definition</remarks>
	public static IHighlightingDefinition Register(string name, IResourceView resourceView, params string[] extensions)
	{
		HighlightingManager.Instance.RegisterHighlighting(name, extensions, () => Load(resourceView));

		return HighlightingManager.Instance.GetDefinition(name)!;
	}

	/// <summary>Loads an .xshd definition from an embedded resource without registering it.</summary>
	public static IHighlightingDefinition Load(IResourceView resourceView)
	{
		XshdSyntaxDefinition xshd;
		using (Stream stream = resourceView.Stream)
		using (XmlReader reader = XmlReader.Create(stream))
		{
			xshd = HighlightingLoader.LoadXshd(reader);
		}
		return HighlightingLoader.Load(xshd, HighlightingManager.Instance);
	}
}
