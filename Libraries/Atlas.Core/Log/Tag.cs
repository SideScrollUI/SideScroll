using Atlas.Extensions;

namespace Atlas.Core
{
	public class Tag
	{
		public string Name { get; set; }

		[InnerValue]
		public object Value { get; set; }

		public bool Unique;

		public override string ToString() => "[ " + Name + " = " + Value.Formatted() + " ]";

		public Tag()
		{
		}

		public Tag(object value)
		{
			Name = value.ToString();
			Value = value;
		}

		public Tag(Tag tag)
		{
			Name = tag.Name;
			Value = tag.Value;
			Unique = tag.Unique;
		}

		public Tag(string name, object value, bool unique = false)
		{
			Name = name;
			Value = value;
			Unique = unique;
		}
	}
}