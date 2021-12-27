using Atlas.Core;

namespace Atlas.Tabs.Test
{
	public class TabTestJson : ITab
	{
		public readonly static string Json1 =
@"{
""id"":""abc"",
""value"": 123,
""map"":
  {
    ""list"":
    [
	  {""type"": ""Cat"", ""Age"": 3},
	  {""type"": ""Dog"", ""Age"": 5},
  	  {""type"": ""Frog"", ""Age"": 7},
	  {""type"": ""Turtle"", ""Age"": 11}
    ]
  }
}";
		public TabInstance Create() => new Instance();

		public class Instance : TabInstance
		{
			public override void Load(Call call, TabModel model)
			{
				model.Items = new ItemCollection<ListItem>()
				{
					new ListItem("Sample Text", LazyJsonNode.Parse(Json1)),
				};
			}
		}
	}
}
