using Atlas.Core;
using Atlas.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Atlas.Serialize
{
	public class TypeSchema
	{
		public static HashSet<Type> AllowedTypes = new HashSet<Type>()
		{
			typeof(string),
			typeof(DateTime),
			typeof(DateTimeOffset),
			typeof(TimeSpan),
			typeof(TimeZoneInfo),
			typeof(Type),
			typeof(object),
		};

		public static HashSet<Type> AllowedGenericTypes = new HashSet<Type>()
		{
			typeof(List<>),
			typeof(Dictionary<,>),
			typeof(HashSet<>),
		};

		public string Name { get; set; }
		public string AssemblyQualifiedName { get; set; }
		public bool CanReference { get; set; } // whether the object can reference other types
		public bool IsCollection;

		public List<FieldSchema> FieldSchemas { get; set; } = new List<FieldSchema>();
		public List<PropertySchema> PropertySchemas { get; set; } = new List<PropertySchema>();

		// not really schema, could break out into a records class
		public int TypeIndex; // -1 if null
		public int NumObjects { get; set; }
		public long FileDataOffset { get; set; }
		public long DataSize { get; set; }

		// not written out
		public Type Type; // might be null
		public Type NonNullableType; // might be null

		public bool IsPrimitive;
		public bool IsPrivate;
		public bool IsPublic; // [PublicData], will get exported if PublicOnly set
		public bool IsStatic;
		public bool IsSerialized;
		public bool HasConstructor;
		public bool HasSubType;

		// Type lookup can take a long time, especially when there's missing types
		private static Dictionary<string, Type> _typeCache = new Dictionary<string, Type>();

		public TypeSchema(Type type)
		{
			Type = type;
			Name = type.ToString(); // better than FullName (don't remember why)

			AssemblyQualifiedName = type.AssemblyQualifiedName; // todo: strip out unused version?
			InitializeType();

			if (!IsCollection)
			{
				// FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance); // For Atlas only?
				foreach (FieldInfo fieldInfo in type.GetFields())
				{
					var fieldSchema = new FieldSchema(fieldInfo);
					if (!fieldSchema.IsSerialized)
						continue;

					FieldSchemas.Add(fieldSchema);
				}

				foreach (PropertyInfo propertyInfo in type.GetProperties())
				{
					var propertySchema = new PropertySchema(propertyInfo);
					if (!propertySchema.IsSerialized)
						continue;

					PropertySchemas.Add(propertySchema);
				}
			}
		}

		private void InitializeType()
		{
			IsCollection = (typeof(ICollection).IsAssignableFrom(Type));
			HasSubType = !Type.IsSealed; // set for all non derived classes?
			CanReference = !(Type.IsPrimitive || Type.IsEnum || Type == typeof(string));
			NonNullableType = Type.GetNonNullableType();
			IsPrimitive = NonNullableType.IsPrimitive;

			//ConstructorInfo constructorInfo = type.GetConstructor(new Type[] { });
			ConstructorInfo constructorInfo = Type.GetConstructor(Type.EmptyTypes); // doesn't find constructor if none declared
			var constructors = Type.GetConstructors();
			HasConstructor = (constructorInfo != null || constructors.Length == 0);

			IsSerialized = (Type.GetCustomAttribute<UnserializedAttribute>() == null);
			IsStatic = (Type.GetCustomAttribute<StaticAttribute>() != null);
			IsPrivate = (Type.GetCustomAttribute<PrivateDataAttribute>() != null);
			IsPublic = GetIsPublic();
		}

		public TypeSchema(Log log, BinaryReader reader)
		{
			Load(log, reader);
		}

		public override string ToString() => Name;

		// BinaryFormatter uses [Serializable], should we allow that?
		private bool GetIsPublic()
		{
			if (IsPrivate)
				return false;
			if (Type.IsPrimitive || Type.IsEnum || Type.IsInterface)
				return true;
			// Might need to modify this later if we ever add dynamic loading
			if (Type.GetCustomAttribute<PublicDataAttribute>() != null)
				return true;
			if (AllowedTypes.Contains(Type))
				return true;
			if (typeof(Type).IsAssignableFrom(Type))
				return true;
			//if (Type.IsSecurityCritical) // useful?
			//	return true;
			if (Type.IsGenericType)
			{
				Type genericType = Type.GetGenericTypeDefinition();
				if (AllowedGenericTypes.Contains(genericType))
					return true;
			}

			return false;
			/*if (Type == null)
				return false;
			if (Type.IsArray)
				return IsAllowed(type.GetElementType());*/
		}

		public void Save(BinaryWriter writer)
		{
			writer.Write(Name);
			writer.Write(AssemblyQualifiedName);
			writer.Write(HasSubType);
			writer.Write(NumObjects);
			writer.Write(FileDataOffset);
			writer.Write(DataSize);

			SaveFields(writer);
			SaveProperties(writer);
		}

		public void Load(Log log, BinaryReader reader)
		{
			Name = reader.ReadString();
			AssemblyQualifiedName = reader.ReadString();
			HasSubType = reader.ReadBoolean();
			NumObjects = reader.ReadInt32();
			FileDataOffset = reader.ReadInt64();
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

			LoadFields(reader);
			LoadProperties(reader);
		}

		private void LoadType(Log log)
		{
			lock (_typeCache)
			{
				if (_typeCache.TryGetValue(AssemblyQualifiedName, out Type type))
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
				log.AddWarning("Missing Versioned Type", new Tag("TypeSchema", this), new Tag("Message", e.Message));
			}

			// Get Type without version
			try
			{
				if (Type == null)
					Type = Type.GetType(AssemblyQualifiedName, AssemblyResolver, null);
			}
			catch (Exception e)
			{
				// why doesn't the false flag work above?
				log.AddWarning("Missing Unversioned Type", new Tag("TypeSchema", this), new Tag("Message", e.Message));
			}

			lock (_typeCache)
			{
				if (!_typeCache.ContainsKey(AssemblyQualifiedName))
					_typeCache.Add(AssemblyQualifiedName, Type);
			}
		}

		// ignore Assembly version to allow loading shared 
		private static Assembly AssemblyResolver(AssemblyName assemblyName)
		{
			assemblyName.Version = null;
			return Assembly.Load(assemblyName);
		}

		public void SaveFields(BinaryWriter writer)
		{
			writer.Write(FieldSchemas.Count);
			foreach (FieldSchema fieldSchema in FieldSchemas)
			{
				fieldSchema.Save(writer);
			}
		}

		public void LoadFields(BinaryReader reader)
		{
			int count = reader.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				FieldSchemas.Add(new FieldSchema(this, reader));
			}
		}

		public void SaveProperties(BinaryWriter writer)
		{
			writer.Write(PropertySchemas.Count);
			foreach (PropertySchema propertySchema in PropertySchemas)
			{
				propertySchema.Save(writer);
			}
		}

		public void LoadProperties(BinaryReader reader)
		{
			int count = reader.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				PropertySchemas.Add(new PropertySchema(this, reader));
			}
		}

		// todo: this isn't getting called for types not serialized
		public void Validate(List<TypeSchema> typeSchemas)
		{
			foreach (FieldSchema fieldSchema in FieldSchemas)
			{
				fieldSchema.Validate(typeSchemas);
			}
			foreach (PropertySchema propertySchema in PropertySchemas)
			{
				propertySchema.Validate(typeSchemas);
			}
		}
	}
}
