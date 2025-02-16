using SideScroll.Serialize.Atlas;
using System.Text.Json;

namespace SideScroll.Serialize.Json;

public class SerializerMemoryJson : SerializerMemory
{
	//private MemoryStream stream = new();

	protected Type? Type { get; set; }
	protected string? Value { get; set; }

	private JsonSerializerOptions Options => PublicOnly ? JsonConverters.PublicJsonSerializerOptions : JsonConverters.PrivateJsonSerializerOptions;

	public override void Save(Call call, object obj)
	{
		using CallTimer callTimer = call.Timer("Save");

		//using var writer = new BinaryWriter(Stream, Encoding.Default, true);

		Type = obj.GetType();
		Value = JsonSerializer.Serialize(obj, Options);

		//JsonSerializer.SerializeAsync(
	}

	public override T Load<T>(Call? call = null)
	{
		T? result = JsonSerializer.Deserialize<T>(Value!, Options)!;
		if (result == null)
		{
			throw new SerializerException("Deserialize failed");
		}
		return result;
	}

	public override bool TryLoad<T>(out T? obj, Call? call = null) where T : class
	{
		obj = (T?)Load(call);
		return obj != null;
	}

	public override object? Load(Call? call = null)
	{
		call ??= new();
		using CallTimer callTimer = call.Timer("Load");

		//Stream.Seek(0, SeekOrigin.Begin);
		//using var reader = new BinaryReader(Stream);

		return JsonSerializer.Deserialize(Value!, Type!, Options)!;
	}

	public override void Validate(Call? call = null)
	{
		call ??= new();
		using CallTimer callTimer = call.Timer("Validate");

		JsonDocument.Parse(Value!);

		/*Stream.Seek(0, SeekOrigin.Begin);
		using var reader = new BinaryReader(Stream);

		var serializer = Create();
		serializer.Load(callTimer, reader, loadData: false);*/
	}

	//public static T Clone<T>(Call call, T obj)
	protected override T DeepCloneInternal<T>(Call call, T obj) where T : class
	{
		Save(call, obj);
		T copy = Load<T>(call);
		return copy;
	}

	protected override object? DeepCloneInternal(Call call, object obj)
	{
		Save(call, obj);
		object? copy = Load(call);
		return copy;
	}
}
