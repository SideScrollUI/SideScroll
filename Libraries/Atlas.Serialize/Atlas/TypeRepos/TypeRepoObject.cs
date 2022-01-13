using Atlas.Core;
using Atlas.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Atlas.Serialize
{
	public class TypeRepoObject : TypeRepo
	{
		public class Creator : IRepoCreator
		{
			public TypeRepo TryCreateRepo(Serializer serializer, TypeSchema typeSchema)
			{
				// todo: support matching constructors with name params & types to fields/properties
				if (typeSchema.HasConstructor || typeSchema.IsSerialized)
					return new TypeRepoObject(serializer, typeSchema);
				return null;
			}
		}

		public List<FieldRepo> FieldRepos = new();
		public List<PropertyRepo> PropertyRepos = new();

		public LazyClass LazyClass;

		public class FieldRepo
		{
			public FieldSchema FieldSchema;
			public TypeRepo TypeRepo;

			public override string ToString() => "Field Repo: " + FieldSchema.FieldName;

			public FieldRepo(FieldSchema fieldSchema, TypeRepo typeRepo = null)
			{
				FieldSchema = fieldSchema;
				TypeRepo = typeRepo;

				if (typeRepo?.Serializer.PublicOnly == true && FieldSchema.IsPrivate)
					FieldSchema.IsLoadable = false;
			}

			public void Load(object obj)
			{
				if (FieldSchema.IsLoadable)
				{
					object valueObject = TypeRepo.LoadObjectRef();
					// todo: 36% of current cpu usage, break into explicit operators? (is that even possible?)
					FieldSchema.FieldInfo.SetValue(obj, valueObject); // else set to null?
				}
				else
				{
					TypeRepo.SkipObjectRef();
				}
			}
		}

		public class PropertyRepo
		{
			public PropertySchema PropertySchema;
			public TypeRepo TypeRepo;
			public LazyProperty LazyProperty;

			public override string ToString() => PropertySchema.ToString() + " (" + TypeRepo.ToString() + ")";

			public PropertyRepo(PropertySchema propertySchema, TypeRepo typeRepo = null)
			{
				PropertySchema = propertySchema;
				TypeRepo = typeRepo;

				if (typeRepo?.Serializer.PublicOnly == true && PropertySchema.IsPrivate)
					PropertySchema.IsLoadable = false;
			}

			// Load serialized data into object
			public void Load(object obj)
			{
				if (!PropertySchema.IsLoadable)
				{
					TypeRepo.LoadLazyObjectRef(); // skip reference
					if (LazyProperty != null)
						LazyProperty.FieldInfoLoaded.SetValue(obj, true);
				}
				else if (LazyProperty != null)
				{
					TypeRef typeRef = TypeRepo.LoadLazyObjectRef();
					LazyProperty.SetTypeRef(obj, typeRef);
				}
				else
				{
					object valueObject = TypeRepo.LoadObjectRef();
					// can throw System.ArgumentException, set to null if not Loadable?
					// Should we add exception handling or detect this earlier when we load the schema?

					// Don't set the property if it's already set to the default, some objects track property assignments
					if (TypeRepo.TypeSchema.IsPrimitive)
					{
						// todo: construct temp object and store default instead for speed?
						dynamic currentValue = PropertySchema.PropertyInfo.GetValue(obj);
						if ((dynamic)valueObject == currentValue)
							return;
					}
					PropertySchema.PropertyInfo.SetValue(obj, valueObject);
				}
			}
		}

		public TypeRepoObject(Serializer serializer, TypeSchema typeSchema) :
			base(serializer, typeSchema)
		{
		}

		public override void InitializeSaving()
		{
			foreach (FieldSchema fieldSchema in TypeSchema.FieldSchemas)
			{
				if (!fieldSchema.IsSerialized)
					continue;

				FieldRepos.Add(new FieldRepo(fieldSchema));
			}

			foreach (PropertySchema propertySchema in TypeSchema.PropertySchemas)
			{
				if (!propertySchema.IsSerialized)
					continue;

				PropertyRepos.Add(new PropertyRepo(propertySchema));
			}
		}

		public override void InitializeLoading(Log log)
		{
			InitializeFields(log);
			InitializeProperties(log);
			//InitializeSaving();
		}

		public bool HasVirtualProperty
		{
			get
			{
				// todo: add nonloadable type
				foreach (PropertySchema propertySchema in TypeSchema.PropertySchemas)
				{
					if (propertySchema.IsLoadable == false)
						continue;

					MethodInfo getMethod = propertySchema.PropertyInfo.GetGetMethod(false);
					if (getMethod.IsVirtual)
						return true;
				}
				return false;
			}
		}

		public void InitializeFields(Log log)
		{
			foreach (FieldSchema fieldSchema in TypeSchema.FieldSchemas)
			{
				//if (fieldSchema.Loadable == false)
				//	continue;

				TypeRepo typeRepo;
				if (fieldSchema.TypeIndex >= 0)
				{
					typeRepo = Serializer.TypeRepos[fieldSchema.TypeIndex];
					if (typeRepo.Type != fieldSchema.NonNullableType)
					{
						log.Add("Can't load field, type has changed", new Tag("Field", fieldSchema));
						fieldSchema.IsLoadable = false;
						//continue;
					}
				}
				else
				{
					Type fieldType = fieldSchema.FieldInfo.FieldType.GetNonNullableType();
					typeRepo = Serializer.GetOrCreateRepo(log, fieldType);
				}
				fieldSchema.FieldTypeSchema = typeRepo.TypeSchema;
				//TypeRepo typeRepo = serializer.typeRepos[fieldSchema.typeIndex];
				//if (typeRepo == null)
				//	continue;

				FieldRepos.Add(new FieldRepo(fieldSchema, typeRepo));
			}
		}

		public void InitializeProperties(Log log)
		{
			var lazyPropertyRepos = new List<PropertyRepo>();
			foreach (PropertySchema propertySchema in TypeSchema.PropertySchemas)
			{
				//if (propertySchema.Loadable == false)
				//	continue;

				TypeRepo typeRepo;
				if (propertySchema.TypeIndex >= 0 || propertySchema.PropertyInfo == null)
				{
					typeRepo = Serializer.TypeRepos[propertySchema.TypeIndex];
					if (typeRepo.Type != propertySchema.NonNullableType)
					{
						// should we add type conversion here?
						log.Add("Can't load field, type has changed", new Tag("Property", propertySchema));
						propertySchema.IsLoadable = false;
						//continue;
					}
				}
				else
				{
					// Base Type might not have been serialized
					Type propertyType = propertySchema.PropertyInfo.PropertyType.GetNonNullableType();
					typeRepo = Serializer.GetOrCreateRepo(log, propertyType);
				}

				propertySchema.PropertyTypeSchema = typeRepo.TypeSchema;

				if (typeRepo != null)
				{
					var propertyRepo = new PropertyRepo(propertySchema, typeRepo);
					PropertyRepos.Add(propertyRepo);

					if (propertySchema.IsLoadable && !propertySchema.Type.IsPrimitive)
						lazyPropertyRepos.Add(propertyRepo);
				}
			}

			// should we add an attribute for this instead?
			if (Serializer.Lazy && HasVirtualProperty)
			{
				LazyClass = new LazyClass(LoadableType, lazyPropertyRepos);
				LoadableType = LazyClass.NewType;
			}

			/*if (lazyClass != null)
			{
				//if (propertySchema.propertyInfo != null)
					lazyClass.lazyProperties.TryGetValue(propertySchema.propertyInfo, out propertyRepo.lazyProperty);
			}*/
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
			LoadFields(obj);
			LoadProperties(obj);
		}

		private void AddFields(object obj)
		{
			foreach (FieldSchema fieldSchema in TypeSchema.FieldSchemas)
			{
				if (!fieldSchema.IsSerialized)
					continue;

				object fieldValue = fieldSchema.FieldInfo.GetValue(obj);
				Serializer.AddObjectRef(fieldValue);
			}
		}

		private void SaveFields(BinaryWriter writer, object obj)
		{
			foreach (FieldRepo fieldRepo in FieldRepos)
			{
				FieldInfo fieldInfo = fieldRepo.FieldSchema.FieldInfo;
				object fieldValue = fieldInfo.GetValue(obj);
				Serializer.WriteObjectRef(fieldRepo.FieldSchema.NonNullableType, fieldValue, writer);
			}
		}

		private void LoadFields(object obj)
		{
			foreach (FieldRepo fieldRepo in FieldRepos)
			{
				fieldRepo.Load(obj);
			}
		}

		private void AddProperties(object value)
		{
			foreach (PropertySchema propertySchema in TypeSchema.PropertySchemas)
			{
				if (!propertySchema.IsSerialized)
					continue;

				object propertyValue = propertySchema.PropertyInfo.GetValue(value);
				Serializer.AddObjectRef(propertyValue);
			}

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
				PropertyInfo propertyInfo = propertyRepo.PropertySchema.PropertyInfo;
				object propertyValue = propertyInfo.GetValue(obj);
				Serializer.WriteObjectRef(propertyRepo.PropertySchema.NonNullableType, propertyValue, writer);
			}
		}

		private void LoadProperties(object obj)
		{
			foreach (PropertyRepo propertyRepo in PropertyRepos)
			{
				propertyRepo.Load(obj);
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
				if (!fieldSchema.IsSerialized)
					continue;

				object propertyValue = fieldSchema.FieldInfo.GetValue(source);
				Serializer.AddObjectRef(propertyValue);
				object clone = Serializer.Clone(propertyValue);
				fieldSchema.FieldInfo.SetValue(dest, clone);
			}
		}

		private void CloneProperties(object source, object dest)
		{
			foreach (PropertySchema propertySchema in TypeSchema.PropertySchemas)
			{
				if (!propertySchema.IsSerialized)
					continue;

				object propertyValue = propertySchema.PropertyInfo.GetValue(source);
				Serializer.AddObjectRef(propertyValue);
				object clone = Serializer.Clone(propertyValue);
				propertySchema.PropertyInfo.SetValue(dest, clone); // else set to null?
			}
		}
	}
}
