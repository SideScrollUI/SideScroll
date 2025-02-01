using SideScroll.Attributes;

namespace SideScroll;

public class TaggedException(string text, params Tag[] tags) : Exception()
{
	public string Text => text;
	public Tag[] Tags => tags;

	private string TagText => Tags == null ? "" : string.Join<Tag>(' ', Tags);

	[WordWrap, MinWidth(300)]
	public override string Message => Text + TagText;
}
