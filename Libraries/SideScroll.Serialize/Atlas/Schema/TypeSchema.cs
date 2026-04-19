using SideScroll.Attributes;
using SideScroll.Extensions;
using SideScroll.Logs;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace SideScroll.Serialize.Atlas.Schema;

/// <summary>
/// Represents schema information for a serialized type including its fields, properties, and metadata
/// </summary>
public class TypeSchema
{
	/// <summary>
	/// Gets or sets the maximum number of objects allowed for public types during import
	/// </summary>
	public static int PublicMaxObjects { get; set; } = 100_000;

	/// <summary>
	/// Gets or sets the set of types that are considered public by default
	/// Only these types and the PublicGenericTypes will be imported or exported if the Serializer is set to PublicOnly
	/// </summary>
	public static HashSet<Type> PublicTypes { get; set; } =
	[
		typeof(string),
		typeof(DateTime),
		typeof(DateTimeOffset),
		typeof(TimeSpan),
		typeof(TimeZoneInfo),
		typeof(Type),
		typeof(Version),
		typeof(object),
	];

	/// <summary>
	/// Gets or sets the set of types that are considered private by default
	/// </summary>
	public static HashSet<Type> PrivateTypes { get; set; } =
	[
		typeof(MemoryStream),
	];

	/// <summary>
	/// Gets or sets the set of generic type definitions that are considered public
	/// Only these types and the PublicTypes will be imported or exported if the Serializer is set to PublicOnly
	/// </summary>
	public static HashSet<Type> PublicGenericTypes { get; set; } =
	[
		typeof(List<>),
		typeof(Dictionary<,>),
		typeof(SortedDictionary<,>),
		typeof(HashSet<>),
	];

	/// <summary>
	/// Gets or sets the type name
	/// </summary>
	[WordWrap]
	public string Name { get; set; }

	/// <summary>
	/// Gets or sets the assembly qualified name for the type
	/// </summary>
	public string AssemblyQualifiedName { get; set; }

	/// <summary>
	/// Gets or sets whether the type can reference other types
	/// </summary>
	public bool CanReference { get; set; }

	/// <summary>
	/// Gets whether the type is a collection
	/// </summary>
	public bool IsCollection { get; protected set; }

	/// <summary>
	/// Gets the list of all member schemas (fields and properties)
	/// </summary>
	public List<MemberSchema> MemberSchemas { get; } = [];

	/// <summary>
	/// Gets the list of field schemas
	/// </summary>
	public List<FieldSchema> FieldSchemas { get; } = [];

	/// <summary>
	/// Gets the list of writable property schemas
	/// </summary>
	public List<PropertySchema> PropertySchemas { get; } = [];

	/// <summary>
	/// Gets the list of all property schemas including read-only ones
	/// </summary>
	[HiddenColumn]
	public List<PropertySchema> ReadOnlyPropertySchemas { get; } = [];

	/// <summary>
	/// Gets or sets the type index in the serializer's type list
	/// </summary>
	public int TypeIndex { get; set; }

	/// <summary>
	/// Gets or sets the number of serialized objects of this type
	/// </summary>
	public int NumObjects { get; set; }

	/// <summary>
	/// Gets or sets the size of the serialized data for this type in bytes
	/// </summary>
	public long DataSize { get; set; }

	/// <summary>
	/// Gets or sets the starting offset of the data in the file
	/// </summary>
	public long StartDataOffset { get; set; }

	/// <summary>
	/// Gets the ending offset of the data in the file
	/// </summary>
	public long EndDataOffset => StartDataOffset + DataSize;

	/// <summary>
	/// Gets the actual runtime type (not serialized)
	/// </summary>
	[WordWrap]
	public Type? Type { get; protected set; }

	/// <summary>
	/// Gets the non-nullable version of the type (not serialized)
	/// </summary>
	[WordWrap, HiddenColumn]
	public Type? NonNullableType { get; protected set; }

	/// <summary>
	/// Gets whether the type is a primitive type
	/// </summary>
	public bool IsPrimitive { get; protected set; }

	/// <summary>
	/// Gets whether the type is marked as private data
	/// </summary>
	public bool IsPrivate { get; protected set; }

	/// <summary>
	/// Gets whether the type is marked as protected data
	/// </summary>
	public bool IsProtected { get; protected set; }

	/// <summary>
	/// Gets whether the type is marked as public data (will be exported if Serializer PublicOnly is set)
	/// </summary>
	public bool IsPublic { get; protected set; }

	/// <summary>
	/// Gets whether the type is either public or protected
	/// </summary>
	public bool IsPublicOnly => IsPublic || IsProtected;

	/// <summary>
	/// Gets whether the type should be cloned by reference instead of deep cloning
	/// </summary>
	public bool IsCloneReference { get; protected set; }

	/// <summary>
	/// Gets whether the type is marked as unserialized
	/// </summary>
	public bool IsUnserialized { get; protected set; }

	/// <summary>
	/// Gets whether the type has an empty constructor
	/// </summary>
	public bool HasEmptyConstructor { get; protected set; }

	/// <summary>
	/// Gets the custom constructor used for deserialization
	/// </summary>
	[HiddenColumn]
	public ConstructorInfo? CustomConstructor { get; protected set; }

	/// <summary>
	/// Gets whether the type has a constructor (empty or custom)
	/// </summary>
	public bool HasConstructor => HasEmptyConstructor || CustomConstructor != null;

	/// <summary>
	/// Gets whether the type has subtypes (is not sealed)
	/// </summary>
	public bool HasSubType { get; protected set; }

	// Type lookup can take a long time, especially when there's missing types
	private static readonly Dictionary<string, Type?> TypeCache = [];

	/// <summary>
	/// Binding flags used for reflection when accessing members
	/// </summary>
	public const BindingFlags BindingAttributes = BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

	public override string ToString() => Name;

	public TypeSchema(Type type, Serializer serializer)
	{
		Type = type;
		Name = type.ToString(); // better than FullName (don't remember why)

		AssemblyQualifiedName = type.AssemblyQualifiedName!; // todo: strip out unused version?
		InitializeType();

		if (!IsCollection)
		{
			InitializeFields(serializer);
			InitializeProperties(serializer);

			CustomConstructor = GetCustomConstructor();
		}
	}

	public TypeSchema(Log log, Serializer serializer, BinaryReader reader)
	{
		Load(log, serializer, reader);
	}

	private void InitializeType()
	{
		IsCollection = typeof(ICollection).IsAssignableFrom(Type);
		HasSubType = !Type!.IsSealed; // set for all non derived classes?
		CanReference = !(Type.IsPrimitive || Type.IsEnum || Type == typeof(string));
		NonNullableType = Type.GetNonNullableType();
		IsPrimitive = NonNullableType.IsPrimitive;
		HasEmptyConstructor = TypeHasEmptyConstructor(Type);

		IsUnserialized = Type.GetCustomAttribute<UnserializedAttribute>() != null;
		IsCloneReference = Type.GetCustomAttribute<CloneReferenceAttribute>() != null;
		IsPrivate = GetIsPrivate();
		IsProtected = Type.GetCustomAttribute<ProtectedDataAttribute>() != null;
		IsPublic = GetIsPublic();
	}

	private void InitializeFields(Serializer serializer)
	{
		foreach (FieldInfo fieldInfo in Type!.GetFields(BindingAttributes))
		{
			var fieldSchema = new FieldSchema(this, fieldInfo);
			if (!fieldSchema.IsReadable)
				continue;

			if (serializer.PublicOnly)
			{
				if (fieldSchema.IsPrivate)
					continue;

				if (!fieldSchema.IsPublic)
					continue;
			}

			FieldSchemas.Add(fieldSchema);
		}
	}

	private void InitializeProperties(Serializer serializer)
	{
		foreach (PropertyInfo propertyInfo in Type!.GetProperties(BindingAttributes))
		{
			var propertySchema = new PropertySchema(this, propertyInfo);
			if (!propertySchema.IsReadable)
				continue;

			if (HasEmptyConstructor && !propertySchema.IsWriteable)
				continue;

			if (propertySchema.IsPrivate && serializer.PublicOnly)
				continue;

			if (serializer.PublicOnly)
			{
				if (propertySchema.IsPrivate)
					continue;

				if (!propertySchema.IsPublic)
					continue;
			}

			if (propertySchema.IsWriteable)
			{
				PropertySchemas.Add(propertySchema);
			}

			ReadOnlyPropertySchemas.Add(propertySchema);
		}
	}

	/// <summary>
	/// Checks whether a type has any usable constructor (empty or custom)
	/// </summary>
	public static bool TypeHasConstructor(Type type) => TypeHasEmptyConstructor(type) || TypeGetCustomConstructor(type) != null;

	/// <summary>
	/// Checks whether a type has an empty parameterless constructor
	/// </summary>
	public static bool TypeHasEmptyConstructor(Type type)
	{
		ConstructorInfo? constructorInfo = type.GetConstructor(Type.EmptyTypes); // doesn't find constructor if none declared
		var constructors = type.GetConstructors();
		return (constructorInfo != null || constructors.Length == 0);
	}

	/// <summary>
	/// Gets the custom constructor for a type if available
	/// </summary>
	public static ConstructorInfo? TypeGetCustomConstructor(Type type)
	{
		return new TypeSchema(type, new Serializer()).GetCustomConstructor();
	}

	/// <summary>
	/// Gets a custom constructor that matches the type's serializable members
	/// </summary>
	public ConstructorInfo? GetCustomConstructor()
	{
		if (HasEmptyConstructor || Type == null) return null;

		var members = ReadOnlyPropertySchemas
			.Where(p => p.IsReadable)
			.Select(p => p.Name.ToLower())
			.ToHashSet();

		var fieldMembers = FieldSchemas
			.Where(f => f.IsReadable)
			.Select(f => f.Name.ToLower())
			.ToHashSet();

		foreach (var member in fieldMembers)
		{
			members.Add(member);
		}

		if (members.Count == 0) return null;

		ConstructorInfo[] constructors = Type!.GetConstructors();
		foreach (ConstructorInfo constructor in constructors)
		{
			ParameterInfo[] parameters = constructor.GetParameters();
			// Skip optional parameters (with default values) when checking for matching constructor
			if (parameters.All(p => p.HasDefaultValue || members.Contains(p.Name?.ToLower() ?? "- invalid -")))
			{
				return constructor;
			}
		}
		return null;
	}

	/// <summary>
	/// Adds properties required by the custom constructor to the serialization list
	/// </summary>
	public void AddCustomConstructorProperties()
	{
		if (CustomConstructor == null) return;

		foreach (var param in CustomConstructor.GetParameters())
		{
			var prop = ReadOnlyPropertySchemas.FirstOrDefault(p => p.Name.Equals(param.Name, StringComparison.CurrentCultureIgnoreCase));
			if (prop != null)
			{
				prop.IsRequired = true;
				if (!PropertySchemas.Contains(prop))
				{
					PropertySchemas.Add(prop);
				}
			}
		}
	}

	private bool GetIsPrivate()
	{
		if (Type!.GetCustomAttribute<PrivateDataAttribute>() != null)
			return true;

		return PrivateTypes.Contains(Type!);
	}

	// BinaryFormatter uses [Serializable], should we allow that?
	private bool GetIsPublic()
	{
		if (IsPrivate)
			return false;

		if (Type!.IsPrimitive || Type.IsEnum || Type.IsInterface)
			return true;

		// Might need to modify this later if we ever add dynamic loading
		if (Type.GetCustomAttribute<PublicDataAttribute>() != null)
			return true;

		if (PublicTypes.Contains(Type))
			return true;

		if (typeof(Type).IsAssignableFrom(Type))
			return true;

		if (Type.IsGenericType)
		{
			Type genericType = Type.GetGenericTypeDefinition();
			if (PublicGenericTypes.Contains(genericType))
				return true;
		}

		return false;
	}

	/// <summary>
	/// Saves the type schema to the binary writer
	/// </summary>
	public void Save(BinaryWriter writer)
	{
		writer.Write(Name);
		writer.Write(AssemblyQualifiedName);
		writer.Write(HasSubType);
		writer.Write(NumObjects);
		writer.Write(StartDataOffset);
		writer.Write(DataSize);

		SaveFields(writer);
		SaveProperties(writer);
	}

	/// <summary>
	/// Loads the type schema from the binary reader
	/// </summary>
	[MemberNotNull(nameof(Name), nameof(AssemblyQualifiedName))]
	public void Load(Log log, Serializer serializer, BinaryReader reader)
	{
		Name = reader.ReadString();
		AssemblyQualifiedName = reader.ReadString();
		HasSubType = reader.ReadBoolean();
		NumObjects = reader.ReadInt32();
		StartDataOffset = reader.ReadInt64();
		DataSize = reader.ReadInt64();

		LoadType(log);
		if (Type == null)
		{
			log.AddWarning("Missing Type", new Tag("TypeSchema", this));
		}
		else
		{
			InitializeType();
		}

		LoadMembers<FieldInfo>(serializer, reader);
		LoadMembers<PropertyInfo>(serializer, reader);

		CustomConstructor = GetCustomConstructor();
	}

	private void LoadType(Log log)
	{
		lock (TypeCache)
		{
			if (TypeCache.TryGetValue(AssemblyQualifiedName, out Type? type))
			{
				Type = type;
				return;
			}
		}

		// Get Type with version
		try
		{
			Type = Type.GetType(AssemblyQualifiedName); // .Net Framework (WPF) requires this?
		}
		catch (Exception e)
		{
			log.AddWarning("Missing Versioned Type",
				new Tag("TypeSchema", this),
				new Tag("Message", e.Message));
		}

		// Get Type without version
		try
		{
			Type ??= Type.GetType(AssemblyQualifiedName, AssemblyResolver, null);
		}
		catch (Exception e)
		{
			log.AddWarning("Missing Unversioned Type",
				new Tag("TypeSchema", this),
				new Tag("Message", e.Message));
		}

		// Get Type with just Namespace, but without assembly
		if (Type == null)
		{
			string typeName = AssemblyQualifiedName.Split(',').First();
			try
			{
				Type = Type.GetType(
					AssemblyQualifiedName,
					(_) =>
					{
						return AppDomain.CurrentDomain.GetAssemblies()
							.FirstOrDefault(a => a.GetType(typeName) != null);
					},
					null,
					true);
			}
			catch (Exception e)
			{
				log.AddWarning("Missing Namespaced Type",
					new Tag("TypeSchema", this),
					new Tag("Message", e.Message));
			}
		}

		lock (TypeCache)
		{
			TypeCache.TryAdd(AssemblyQualifiedName, Type);
		}
	}

	// ignore Assembly version to allow loading shared 
	private static Assembly AssemblyResolver(AssemblyName assemblyName)
	{
		assemblyName.Version = null;
		return Assembly.Load(assemblyName);
	}

	/// <summary>
	/// Saves field schemas to the binary writer
	/// </summary>
	public void SaveFields(BinaryWriter writer)
	{
		writer.Write(FieldSchemas.Count);
		foreach (FieldSchema fieldSchema in FieldSchemas)
		{
			fieldSchema.Save(writer);
		}
	}

	/// <summary>
	/// Saves property schemas to the binary writer
	/// </summary>
	public void SaveProperties(BinaryWriter writer)
	{
		writer.Write(PropertySchemas.Count);
		foreach (PropertySchema propertySchema in PropertySchemas)
		{
			propertySchema.Save(writer);
		}
	}

	/// <summary>
	/// Loads member schemas from the binary reader
	/// </summary>
	public void LoadMembers<T>(Serializer serializer, BinaryReader reader) where T : MemberInfo
	{
		int count = reader.ReadInt32();
		for (int i = 0; i < count; i++)
		{
			var memberSchema = MemberSchema.Load<T>(this, serializer, reader);
			MemberSchemas.Add(memberSchema);

			if (memberSchema is FieldSchema fieldSchema)
			{
				FieldSchemas.Add(fieldSchema);
			}
			else if (memberSchema is PropertySchema propertySchema)
			{
				PropertySchemas.Add(propertySchema);
				ReadOnlyPropertySchemas.Add(propertySchema);
			}
		}
	}

	/// <summary>
	/// Validates the type schema against the serializer and list of type schemas
	/// </summary>
	public void Validate(Serializer serializer, List<TypeSchema> typeSchemas)
	{
		long totalBytes = serializer.Reader!.BaseStream.Length;
		if (NumObjects > totalBytes || DataSize > totalBytes)
		{
			throw new SerializerException("Invalid object count or data size",
				new Tag("Type", Type),
				new Tag("Num Objects", NumObjects),
				new Tag("Data Size", DataSize),
				new Tag("Total Bytes", totalBytes));
		}

		if (serializer.PublicOnly && IsPublic && NumObjects > PublicMaxObjects)
		{
			throw new SerializerException("Too many objects for public import",
				new Tag("Type", Type),
				new Tag("Objects", NumObjects),
				new Tag("Max", PublicMaxObjects));
		}

		foreach (FieldSchema fieldSchema in FieldSchemas)
		{
			fieldSchema.Validate(typeSchemas);
		}

		foreach (PropertySchema propertySchema in PropertySchemas)
		{
			propertySchema.Validate(typeSchemas);
		}
	}

	/// <summary>
	/// Gets the member info for the specified member name, checking deprecated names if not found
	/// </summary>
	public MemberInfo? GetMemberInfo(string name)
	{
		var members = Type!.GetMember(name, BindingAttributes);
		if (members.Length > 0)
			return members[0];

		// Check deprecated names
		foreach (MemberInfo memberInfo in Type!.GetMembers(BindingAttributes))
		{
			if (memberInfo.GetCustomAttribute<DeprecatedNameAttribute>() is DeprecatedNameAttribute attribute &&
				attribute.Names.Contains(name))
			{
				return memberInfo;
			}
		}
		return null;
	}
}
