using SideScroll.Serialize.Atlas.TypeRepos;
using System.Text;

namespace SideScroll.Serialize.Atlas;

public class SerializerMemoryAtlas : SerializerMemory
{
	//private MemoryStream stream = new();

	protected TypeRepoString? TypeRepoString { get; set; } // Reuse string instances to reduce memory use when deep cloning

	private new Serializer Create()
	{
		return new Serializer
		{
			PublicOnly = PublicOnly,
		};
	}

	public override void Save(Call call, object obj)
	{
		using CallTimer callTimer = call.Timer();

		using var writer = new BinaryWriter(Stream, Encoding.Default, true);

		var serializer = Create();
		serializer.AddObject(callTimer, obj);
		serializer.Save(callTimer, writer);

		if (serializer.IdxTypeToRepo.TryGetValue(typeof(string), out TypeRepo? typeRepo))
		{
			TypeRepoString = (TypeRepoString)typeRepo;
		}
	}

	public override T Load<T>(Call? call = null)
	{
		return (T)Load(call)!;
	}

	public override bool TryLoad<T>(out T? obj, Call? call = null) where T : class
	{
		obj = (T?)Load(call);
		return obj != null;
	}

	public override object? Load(Call? call = null)
	{
		call ??= new();
		using CallTimer callTimer = call.Timer();

		Stream.Seek(0, SeekOrigin.Begin);
		using var reader = new BinaryReader(Stream);

		var serializer = Create();
		serializer.TypeRepoString = TypeRepoString;
		serializer.Load(callTimer, reader);
		return serializer.BaseObject(call);
	}

	public override void Validate(Call? call = null)
	{
		call ??= new();
		using CallTimer callTimer = call.Timer();

		Stream.Seek(0, SeekOrigin.Begin);
		using var reader = new BinaryReader(Stream);

		var serializer = Create();
		serializer.Load(callTimer, reader, loadData: false);
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
