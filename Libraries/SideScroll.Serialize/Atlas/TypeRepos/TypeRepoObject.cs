using SideScroll.Extensions;
using SideScroll.Logs;
using SideScroll.Serialize.Atlas.Schema;
using System.Reflection;

namespace SideScroll.Serialize.Atlas.TypeRepos;

public class TypeRepoObject : TypeRepo
{
	public class Creator : IRepoCreator
	{
		public TypeRepo? TryCreateRepo(Serializer serializer, TypeSchema typeSchema)
		{
			// todo: support matching constructors with name params & types to fields/properties
			if (typeSchema.HasConstructor || typeSchema.IsSerialized)
			{
				return new TypeRepoObject(serializer, typeSchema);
			}
			return null;
		}
	}

	public interface IMemberRepo
	{
		public void Load(object obj);
		public object? Get();
	}

	public List<IMemberRepo> MemberRepos { get; protected set; } = [];

	public List<FieldRepo> FieldRepos { get; protected set; } = [];
	public List<PropertyRepo> PropertyRepos { get; protected set; } = [];

	public LazyClass? LazyClass { get; protected set; }

	public class FieldRepo : IMemberRepo
	{
		public FieldSchema FieldSchema { get; }
		public TypeRepo? TypeRepo { get; }

		public override string ToString() => "Field Repo: " + FieldSchema.Name;

		public FieldRepo(FieldSchema fieldSchema, TypeRepo? typeRepo = null)
		{
			FieldSchema = fieldSchema;
			TypeRepo = typeRepo;

			if (typeRepo?.Serializer.PublicOnly == true && FieldSchema.IsPrivate)
			{
				FieldSchema.IsReadable = false;
			}
		}

		public void Load(object obj)
		{
			if (FieldSchema.IsReadable)
			{
				object? valueObject = TypeRepo!.LoadObjectRef();
				// todo: 36% of current cpu usage, break into explicit operators? (is that even possible?)
				FieldSchema.FieldInfo!.SetValue(obj, valueObject); // else set to null?
			}
			else
			{
				TypeRepo!.SkipObjectRef();
			}
		}

		public object? Get()
		{
			if (FieldSchema.IsReadable)
			{
				return TypeRepo!.LoadObjectRef();
			}
			else
			{
				TypeRepo!.SkipObjectRef();
				return null;
			}
		}
	}

	public class PropertyRepo : IMemberRepo
	{
		public PropertySchema PropertySchema { get; }
		public TypeRepo? TypeRepo { get; }
		public LazyProperty? LazyProperty { get; set; }

		public override string ToString() => $"{PropertySchema} ({TypeRepo})";

		public PropertyRepo(PropertySchema propertySchema, TypeRepo? typeRepo = null)
		{
			PropertySchema = propertySchema;
			TypeRepo = typeRepo;

			if (typeRepo?.Serializer.PublicOnly == true && PropertySchema.IsPrivate)
			{
				PropertySchema.IsWriteable = false;
				PropertySchema.IsReadable = false;
			}
		}

		// Load serialized data into object
		public void Load(object obj)
		{
			if (LazyProperty != null)
			{
				TypeRef? typeRef = TypeRepo!.LoadLazyObjectRef();
				if (!PropertySchema.IsWriteable)
				{
					typeRef = null;
				}

				LazyProperty.SetTypeRef(obj, typeRef);
			}
			else if (!PropertySchema.IsWriteable)
			{
				TypeRepo!.SkipObjectRef();
			}
			else
			{
				object? valueObject = TypeRepo!.LoadObjectRef();
				// can throw System.ArgumentException, set to null if not Loadable?
				// Should we add exception handling or detect this earlier when we load the schema?

				// Don't set the property if it's already set to the default, some objects track property assignments
				if (TypeRepo.TypeSchema.IsPrimitive)
				{
					// todo: construct temp object and store default instead for speed?
					dynamic? currentValue = PropertySchema.PropertyInfo!.GetValue(obj);
					if ((dynamic?)valueObject == currentValue)
						return;
				}
				PropertySchema.PropertyInfo!.SetValue(obj, valueObject);
			}
		}

		public object? Get()
		{
			if (!PropertySchema.IsReadable)
			{
				return TypeRepo!.SkipObjectRef(); // Custom constuctors can load non-writeable properties
			}
			else if (LazyProperty != null)
			{
				throw new SerializerException("Get() doesn't support Lazy Properties",
					new Tag("Property", PropertySchema.Name));
			}
			else
			{
				return TypeRepo!.LoadObjectRef();
			}
		}
	}

	// Skips loading all data
	public class MemberRepo : IMemberRepo
	{
		public MemberSchema MemberSchema { get; }
		public TypeRepo? TypeRepo { get; }

		public override string ToString() => "Member Repo: " + MemberSchema.Name;

		public MemberRepo(MemberSchema memberSchema, TypeRepo? typeRepo = null)
		{
			MemberSchema = memberSchema;
			TypeRepo = typeRepo;
			MemberSchema.IsReadable = false;
		}

		public void Load(object obj)
		{
			TypeRepo!.SkipObjectRef();
		}

		public object? Get()
		{
			TypeRepo!.SkipObjectRef();
			return null;
		}
	}

	public TypeRepoObject(Serializer serializer, TypeSchema typeSchema) :
		base(serializer, typeSchema)
	{
		TypeSchema.AddCustomConstructorProperties();
	}

	public override void InitializeSaving()
	{
		foreach (FieldSchema fieldSchema in TypeSchema.FieldSchemas)
		{
			if (!fieldSchema.IsReadable)
				continue;

			FieldRepos.Add(new FieldRepo(fieldSchema));
		}

		foreach (PropertySchema propertySchema in TypeSchema.PropertySchemas)
		{
			if (!propertySchema.ShouldWrite)
				continue;

			PropertyRepos.Add(new PropertyRepo(propertySchema));
		}
	}

	public override void InitializeLoading(Log log)
	{
		InitializeMembers(log);
		InitializeConstructor(log);

		if (!TypeSchema.HasConstructor)
		{
			log.AddWarning("No matching constructor found", new Tag("Type", TypeSchema.ToString()));
		}
	}

	public bool HasVirtualProperty
	{
		get
		{
			// todo: add nonloadable type
			foreach (PropertySchema propertySchema in TypeSchema.PropertySchemas)
			{
				if (!propertySchema.IsWriteable) continue;

				MethodInfo getMethod = propertySchema.PropertyInfo!.GetGetMethod(false)!;
				if (getMethod.IsVirtual) return true;
			}
			return false;
		}
	}

	public void InitializeMembers(Log log)
	{
		List<PropertyRepo> lazyPropertyRepos = [];

		foreach (MemberSchema memberSchema in TypeSchema.MemberSchemas)
		{
			if (memberSchema is FieldSchema fieldSchema)
			{
				InitializeField(log, fieldSchema);
			}
			else if (memberSchema is PropertySchema propertySchema)
			{
				InitializeProperty(log, propertySchema, lazyPropertyRepos);
			}
			else
			{
				InitializeMember(log, memberSchema);
			}
		}

		if (Serializer.Lazy && HasVirtualProperty)
		{
			LazyClass = new LazyClass(LoadableType!, lazyPropertyRepos);
			LoadableType = LazyClass.LazyType;
		}
	}

	public void InitializeField(Log log, FieldSchema fieldSchema)
	{
		TypeRepo typeRepo;
		if (fieldSchema.TypeIndex >= 0)
		{
			typeRepo = Serializer.TypeRepos[fieldSchema.TypeIndex];
			if (typeRepo.Type != fieldSchema.NonNullableType)
			{
				log.Add("Can't load field, type has changed", new Tag("Field", fieldSchema));
				fieldSchema.IsReadable = false;
			}
		}
		else
		{
			Type fieldType = fieldSchema.FieldInfo!.FieldType.GetNonNullableType();
			typeRepo = Serializer.GetOrCreateRepo(log, fieldType);
		}
		fieldSchema.FieldTypeSchema = typeRepo.TypeSchema;

		var fieldRepo = new FieldRepo(fieldSchema, typeRepo);
		FieldRepos.Add(fieldRepo);
		MemberRepos.Add(fieldRepo);
	}

	public void InitializeProperty(Log log, PropertySchema propertySchema, List<PropertyRepo> lazyPropertyRepos)
	{
		TypeRepo typeRepo;
		if (propertySchema.TypeIndex >= 0 || propertySchema.PropertyInfo == null)
		{
			typeRepo = Serializer.TypeRepos[propertySchema.TypeIndex];
			if (typeRepo.Type != propertySchema.NonNullableType)
			{
				// Should we add type conversion here?
				log.Add("Can't load field, type has changed", new Tag("Property", propertySchema));
				propertySchema.IsWriteable = false;
			}
		}
		else
		{
			// Base Type might not have been serialized
			Type propertyType = propertySchema.PropertyInfo.PropertyType.GetNonNullableType();
			typeRepo = Serializer.GetOrCreateRepo(log, propertyType);
		}

		propertySchema.PropertyTypeSchema = typeRepo.TypeSchema;

		var propertyRepo = new PropertyRepo(propertySchema, typeRepo);
		PropertyRepos.Add(propertyRepo);
		MemberRepos.Add(propertyRepo);

		if (propertySchema.IsWriteable && !propertySchema.Type!.IsPrimitive)
		{
			lazyPropertyRepos.Add(propertyRepo);
		}
	}

	public void InitializeMember(Log log, MemberSchema memberSchema)
	{
		TypeRepo typeRepo;
		if (memberSchema.TypeIndex >= 0)
		{
			typeRepo = Serializer.TypeRepos[memberSchema.TypeIndex];
		}
		else
		{
			throw new SerializerException("Member Type Index is invalid", new Tag("Member", memberSchema));
		}

		var memberRepo = new MemberRepo(memberSchema, typeRepo);
		MemberRepos.Add(memberRepo);
	}

	private readonly List<object> _constructorRepos = [];

	public void InitializeConstructor(Log log)
	{
		if (TypeSchema.CustomConstructor == null) return;

		var fields = FieldRepos.ToDictionary(f => f.FieldSchema.Name.ToLower());
		var properties = PropertyRepos.ToDictionary(f => f.PropertySchema.Name.ToLower());

		var parameters = TypeSchema.CustomConstructor.GetParameters();
		foreach (var param in parameters)
		{
			string name = param.Name!.ToLower();
			if (fields.TryGetValue(name, out var field))
			{
				_constructorRepos.Add(field);
			}
			else if (properties.TryGetValue(name, out var property))
			{
				_constructorRepos.Add(property);
			}
			else
			{
				log.Throw(new SerializerException("Constructor param not found", new Tag("Param", name)));
			}
		}
	}

	// Reads over all the field and properties since they're ordered without offsets
	private List<object?> GetConstructorParams(int objectIndex)
	{
		if (TypeSchema.CustomConstructor == null)
		{
			throw new SerializerException("No default or matching constructor found", new Tag("TypeSchema", TypeSchema.ToString()));
		}

		long position = Reader!.BaseStream.Position;
		Reader!.BaseStream.Position = ObjectOffsets![objectIndex];

		Dictionary<FieldRepo, object?> fieldValues = FieldRepos.ToDictionary(f => f, f => f.Get());
		Dictionary<PropertyRepo, object?> propertyValues = PropertyRepos.ToDictionary(p => p, p => p.Get());

		List<object?> parameters = [];
		foreach (var repo in _constructorRepos)
		{
			if (repo is FieldRepo fieldRepo)
			{
				if (fieldValues.TryGetValue(fieldRepo, out object? value))
				{
					parameters.Add(value);
				}
				else
				{
					throw new SerializerException("Missing FieldRepo: " + fieldRepo);
				}
			}
			else if (repo is PropertyRepo propertyRepo)
			{
				if (propertyValues.TryGetValue(propertyRepo, out object? value))
				{
					parameters.Add(value);
				}
				else
				{
					throw new SerializerException("Missing PropertyRepo: " + propertyRepo);
				}
			}
			else
			{
				throw new SerializerException("Unhandled repo type: " + repo);
			}
		}

		Reader.BaseStream.Position = position;
		return parameters;
	}

	protected override object? CreateObject(int objectIndex)
	{
		if (TypeSchema.HasEmptyConstructor)
		{
			return base.CreateObject(objectIndex);
		}

		List<object?> constructorParams = GetConstructorParams(objectIndex);

		object obj = TypeSchema.CustomConstructor!.Invoke(constructorParams.ToArray());

		ObjectsLoaded[objectIndex] = obj; // must assign before loading any more refs
		Serializer.QueueLoading(this, objectIndex);
		return obj;
	}

	public override void AddChildObjects(object obj)
	{
		AddFields(obj);
		AddProperties(obj);
	}

	public override void SaveObject(BinaryWriter writer, object obj)
	{
		SaveFields(writer, obj);
		SaveProperties(writer, obj);
	}

	public override void LoadObjectData(object obj)
	{
		foreach (IMemberRepo memberRepo in MemberRepos)
		{
			memberRepo.Load(obj);
		}
	}

	private void AddFields(object obj)
	{
		foreach (FieldSchema fieldSchema in TypeSchema.FieldSchemas)
		{
			if (!fieldSchema.IsReadable)
				continue;

			object? fieldValue = fieldSchema.FieldInfo!.GetValue(obj);
			Serializer.AddObjectRef(fieldValue);
		}
	}

	private void SaveFields(BinaryWriter writer, object obj)
	{
		foreach (FieldRepo fieldRepo in FieldRepos)
		{
			FieldInfo fieldInfo = fieldRepo.FieldSchema.FieldInfo!;
			object? fieldValue = fieldInfo.GetValue(obj);
			Serializer.WriteObjectRef(fieldRepo.FieldSchema.NonNullableType!, fieldValue, writer);
		}
	}

	private void AddProperties(object obj)
	{
		foreach (PropertySchema propertySchema in TypeSchema.PropertySchemas)
		{
			if (!propertySchema.ShouldWrite) continue;

			object? propertyValue = propertySchema.PropertyInfo!.GetValue(obj);
			Serializer.AddObjectRef(propertyValue);
		}

		// Groups values by property instead of object
		/*foreach (PropertySchema propertySchema in typeSchema.propertySchemas)
		{
			Type propertyType = propertySchema.propertyInfo.PropertyType.GetNonNullableType();

			TypeRepo typeRepo;
			if (!serializer.idxTypeToInstances.TryGetValue(propertyType, out typeRepo))
				continue;

			propertySchema.typeIndex = typeRepo.typeIndex;
			propertySchema.fileDataOffset = writer.BaseStream.Position;
			foreach (object obj in objects)
			{
				object propertyValue = propertySchema.propertyInfo.GetValue(obj);
				AddObjectRef(propertyValue, writer);
			}
		}*/
	}

	private void SaveProperties(BinaryWriter writer, object obj)
	{
		foreach (PropertyRepo propertyRepo in PropertyRepos)
		{
			PropertyInfo propertyInfo = propertyRepo.PropertySchema.PropertyInfo!;
			object? propertyValue = propertyInfo.GetValue(obj);
			Serializer.WriteObjectRef(propertyRepo.PropertySchema.NonNullableType!, propertyValue, writer);
		}
	}

	public override void Clone(object source, object dest)
	{
		CloneFields(source, dest);
		CloneProperties(source, dest);
	}

	private void CloneFields(object source, object dest)
	{
		foreach (FieldSchema fieldSchema in TypeSchema.FieldSchemas)
		{
			if (!fieldSchema.IsReadable) continue;

			object? fieldValue = fieldSchema.FieldInfo!.GetValue(source);
			Serializer.AddObjectRef(fieldValue);
			object? clone = Serializer.Clone(fieldValue);
			fieldSchema.FieldInfo.SetValue(dest, clone);
		}
	}

	private void CloneProperties(object source, object dest)
	{
		foreach (PropertySchema propertySchema in TypeSchema.PropertySchemas)
		{
			if (!propertySchema.ShouldWrite) continue;

			object? propertyValue = propertySchema.PropertyInfo!.GetValue(source);
			Serializer.AddObjectRef(propertyValue);
			object? clone = Serializer.Clone(propertyValue);
			propertySchema.PropertyInfo.SetValue(dest, clone); // else set to null?
		}
	}
}
