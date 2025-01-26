using SideScroll.Logs;
using SideScroll.Serialize.Atlas.Schema;
using System.Collections;
using System.Diagnostics;
using System.Reflection;

namespace SideScroll.Serialize.Atlas.TypeRepos;

public class TypeRepoDictionary : TypeRepo
{
	public class Creator : IRepoCreator
	{
		public TypeRepo? TryCreateRepo(Serializer serializer, TypeSchema typeSchema)
		{
			if (CanAssign(typeSchema.Type!))
			{
				return new TypeRepoDictionary(serializer, typeSchema);
			}
			return null;
		}
	}

	private readonly Type? _typeKey;
	private readonly Type? _typeValue;

	private readonly MethodInfo _addMethod;

	private TypeRepo? _list1TypeRepo;
	private TypeRepo? _list2TypeRepo;

	public TypeRepoDictionary(Serializer serializer, TypeSchema typeSchema) :
		base(serializer, typeSchema)
	{
		Type[] types = LoadableType!
			.GetInterfaces()
			.FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>))?
			.GetGenericArguments() ?? LoadableType.GetGenericArguments();

		if (types.Length > 1)
		{
			_typeKey = types[0];
			_typeValue = types[1];
		}
		else
		{
			Debug.WriteLine($"Failed to find generic arguments for {LoadableType}");
		}

		_addMethod = LoadableType.GetMethods()
			.FirstOrDefault(m => m.Name == "Add" && m.GetParameters().Length == 2)!;
	}

	public static bool CanAssign(Type type)
	{
		return typeof(IDictionary).IsAssignableFrom(type);
	}

	public override void InitializeLoading(Log log)
	{
		// these base types might not be serialized
		if (_typeKey != null)
		{
			_list1TypeRepo = Serializer.GetOrCreateRepo(log, _typeKey);
		}

		if (_typeValue != null)
		{
			_list2TypeRepo = Serializer.GetOrCreateRepo(log, _typeValue);
		}
	}

	public override void AddChildObjects(object obj)
	{
		var dictionary = (IDictionary)obj;
		foreach (DictionaryEntry item in dictionary)
		{
			Serializer.AddObjectRef(item.Key);
			Serializer.AddObjectRef(item.Value);
		}
	}

	public override void SaveObject(BinaryWriter writer, object obj)
	{
		var dictionary = (IDictionary)obj;

		writer.Write(dictionary.Count);
		foreach (DictionaryEntry item in dictionary)
		{
			Serializer.WriteObjectRef(_typeKey!, item.Key, writer);
			Serializer.WriteObjectRef(_typeValue!, item.Value, writer);
		}
	}

	public override void LoadObjectData(object obj)
	{
		var dictionary = (IDictionary)obj;
		int count = Reader!.ReadInt32();

		for (int j = 0; j < count; j++)
		{
			object? key = _list1TypeRepo!.LoadObjectRef();
			object? value = _list2TypeRepo!.LoadObjectRef();

			if (key != null)
			{
				_addMethod.Invoke(dictionary, [key, value]);
			}
		}
	}

	public override void Clone(object source, object dest)
	{
		var iSource = (IDictionary)source;
		var iDest = (IDictionary)dest;
		foreach (DictionaryEntry item in iSource)
		{
			object? key = Serializer.Clone(item.Key);
			object? value = Serializer.Clone(item.Value);
			if (key != null)
			{
				_addMethod.Invoke(iDest, [key, value]);
			}
		}
	}
}
