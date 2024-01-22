using Atlas.Core;

namespace Atlas.Tabs.Test;

public class TabTestJson : ITab
{
	public static readonly string Json1 =
@"{
""id"":""abc"",
""value"": 123,
""map"":
  {
    ""list"":
    [
	  {""type"": ""Cat"", ""Age"": 3, ""Indoor"": true},
	  {""type"": ""Dog"", ""Age"": 5, ""Indoor"": true},
  	  {""type"": ""Frog"", ""Age"": 7, ""Indoor"": false},
	  {""type"": ""Turtle"", ""Age"": null, ""Indoor"": true}
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
				new("Sample Text", LazyJsonNode.Parse(Json1)),
			};
		}
	}
}
