using Atlas.Extensions;

namespace Atlas.Core
{
	public class Tag
	{
		public string Name { get; set; }
		[InnerValue]
		public object Value { get; set; }

		public override string ToString() => "[ " + Name + " = " + Value.Formatted() + " ]";

		public Tag()
		{
		}

		public Tag(object value)
		{
			Name = value.ToString();
			Value = value;
		}

		public Tag(string name, object value, bool verbose = true)
		{
			Name = name;

			if (verbose)
				Value = value;
			else
				Value = value.ToString();
		}

		public static Tag Add(object value, bool verbose = true)
		{
			return new Tag(value.ToString(), value, verbose);
		}
	}
}