using Atlas.Core;
using Newtonsoft.Json;
using System;
using System.IO;

namespace Atlas.Serialize;

public class SerializerFileJson : SerializerFile
{
	private const string HeaderName = "Header.atlas";
	private const string DataName = "Data.json";

	public SerializerFileJson(string basePath, string name = "") : base(basePath, name)
	{
		HeaderPath = Paths.Combine(basePath, HeaderName);
		DataPath = Paths.Combine(basePath, DataName);
	}

	public override void SaveInternal(Call call, object obj, string name = null)
	{
		string json = JsonConvert.SerializeObject(obj); // pass obj.GetType()?
		File.WriteAllText(DataPath, json);
		SaveHeader(call, name);
	}

	protected override object LoadInternal(Call call, bool lazy, TaskInstance taskInstance)
	{
		// This doesn't work for the System.Text.Json since it doesn't support dynamic types
		string json = File.ReadAllText(DataPath);
		object obj = JsonConvert.DeserializeObject(json);
		return obj;
	}

	public void SaveHeader(Call call, string name = null)
	{
		using var stream = new FileStream(HeaderPath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
		using var writer = new BinaryWriter(stream);

		var header = new Header()
		{
			Name = name,
		};
		header.Save(writer);
	}
}
