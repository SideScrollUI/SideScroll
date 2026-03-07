using SideScroll.Serialize.Atlas.TypeRepos;
using System.Text;

namespace SideScroll.Serialize.Atlas;

/// <summary>
/// In-memory serializer implementation using the Atlas format
/// </summary>
public class SerializerMemoryAtlas : SerializerMemory
{
	/// <summary>
	/// Gets or sets the string type repository used to reuse string instances during deep cloning
	/// </summary>
	protected TypeRepoString? TypeRepoString { get; set; }

	private new Serializer Create()
	{
		return new Serializer
		{
			PublicOnly = PublicOnly,
		};
	}

	/// <summary>
	/// Saves an object to the memory stream using Atlas serialization
	/// </summary>
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

	/// <summary>
	/// Loads an object of the specified type from the memory stream
	/// </summary>
	public override T Load<T>(Call? call = null)
	{
		return (T)Load(call)!;
	}

	/// <summary>
	/// Attempts to load an object from the memory stream
	/// </summary>
	public override bool TryLoad<T>(out T? obj, Call? call = null) where T : class
	{
		obj = (T?)Load(call);
		return obj != null;
	}

	/// <summary>
	/// Loads an object from the memory stream
	/// </summary>
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

	/// <summary>
	/// Validates the serialized data without fully loading the object
	/// </summary>
	public override void Validate(Call? call = null)
	{
		call ??= new();
		using CallTimer callTimer = call.Timer();

		Stream.Seek(0, SeekOrigin.Begin);
		using var reader = new BinaryReader(Stream);

		var serializer = Create();
		serializer.Load(callTimer, reader, loadData: false);
	}

	/// <summary>
	/// Internal implementation for deep cloning a typed object
	/// </summary>
	protected override T DeepCloneInternal<T>(Call call, T obj) where T : class
	{
		Save(call, obj);
		T copy = Load<T>(call);
		return copy;
	}

	/// <summary>
	/// Internal implementation for deep cloning an object
	/// </summary>
	protected override object? DeepCloneInternal(Call call, object obj)
	{
		Save(call, obj);
		object? copy = Load(call);
		return copy;
	}
}
