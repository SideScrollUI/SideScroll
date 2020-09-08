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
		/*public enum TypeClass
		{
			primitive,
			list,
			dictionary,
			hashset
		}*/
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
		public bool isPrimitive;
		public bool HasConstructor = true;
		public bool Secure = false; // Secure types do not get saved if the SaveSecure flag is set

		public bool IsStatic;
		public bool HasSubType;

		public TypeSchema(Type type)
		{
			Type = type;
			Name = type.ToString(); // better than FullName (don't remember why)
			
			AssemblyQualifiedName = type.AssemblyQualifiedName; // todo: strip out unused version?
			IsCollection = (typeof(ICollection).IsAssignableFrom(type));
			HasSubType = !type.IsSealed; // set for all non derived classes?
			CanReference = !(type.IsPrimitive || type.IsEnum || type == typeof(string));
			NonNullableType = type.GetNonNullableType();
			isPrimitive = NonNullableType.IsPrimitive;

			IsStatic = (type.GetCustomAttribute<StaticAttribute>() != null);
			Secure = (type.GetCustomAttribute<SecureAttribute>() != null);

			if (!IsCollection)
			{
				// FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance); // For Atlas only?
				foreach (FieldInfo fieldInfo in type.GetFields())
				{
					var fieldSchema = new FieldSchema(fieldInfo);
					if (!fieldSchema.Serialized)
						continue;

					FieldSchemas.Add(fieldSchema);
				}
				
				foreach (PropertyInfo propertyInfo in type.GetProperties())
				{
					var propertySchema = new PropertySchema(propertyInfo);
					if (!propertySchema.Serialized)
						continue;

					PropertySchemas.Add(propertySchema);
				}
			}
		}

		public TypeSchema(Log log, BinaryReader reader)
		{
			Load(log, reader);
		}

		public override string ToString() => Name;

		// not completely safe since anyone can name their Assemblies whatever, but someone would have to include those libraries
		// BinaryFormatter uses[Serializable], should we allow that?
		public static bool IsAllowed(Type type)
		{
			return true;
			/*if (type == null)
				return false;
			if (type.IsPrimitive)
				return true;
			if (type == typeof(string))
				return true;
			if (type == typeof(object))
				return true;
			if (type == typeof(DateTime))
				return true;
			if (typeof(Type).IsAssignableFrom(type))
				return true;
			if (type.IsArray)
				return IsWhitelisted(type.GetElementType());
			if (type.Assembly.FullName.StartsWith("Atlas."))
				return true;
			if (type.Namespace.StartsWith("System."))
			{
				//type.Assembly.GetName().GetPublicKeyToken(); // todo: add support for this
				if (!type.Assembly.IsFullyTrusted)
					return false;
				foreach (Type argType in type.GenericTypeArguments)
				{
					if (!IsWhitelisted(argType))
						return false;
				}
				return true;
			}
			return false;*/
		}

		// loading a random type from a derived type such as an object can be dangerous, disallow by default
		// otherwise it could be dangerous loading a random file from the internet
		public bool Allowed
		{
			get
			{
				return IsAllowed(Type);
			}
		}

		public bool IsPrimitive
		{
			get
			{
				if (NonNullableType == null)
					return false;

				return NonNullableType.IsPrimitive;
			}
		}

		public void Save(BinaryWriter writer)
		{
			writer.Write(Name);
			writer.Write(AssemblyQualifiedName);
			writer.Write(CanReference);
			writer.Write(IsCollection);
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
			CanReference = reader.ReadBoolean();
			IsCollection = reader.ReadBoolean();
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
				NonNullableType = Type.GetNonNullableType();
				isPrimitive = NonNullableType.IsPrimitive;
			}

			LoadFields(reader);
			LoadProperties(reader);
		}

		// Type lookup can take a long time, especially when there's missing types
		private static Dictionary<string, Type> typeCache = new Dictionary<string, Type>();

		private void LoadType(Log log)
		{
			lock (typeCache)
			{
				if (typeCache.TryGetValue(AssemblyQualifiedName, out Type type))
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

			lock (typeCache)
			{
				typeCache.Add(AssemblyQualifiedName, Type);
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
			//ConstructorInfo constructorInfo = type.GetConstructor(new Type[] { });
			ConstructorInfo constructorInfo = Type.GetConstructor(Type.EmptyTypes); // doesn't find constructor if none declared
			var constructors = Type.GetConstructors();
			HasConstructor = (constructorInfo != null || constructors.Length == 0);
		}
	}
}
