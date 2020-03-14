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
				return new TypeRepoObject(serializer, typeSchema);
			}
		}

		public List<FieldRepo> fieldRepos = new List<FieldRepo>();
		public List<PropertyRepo> propertyRepos = new List<PropertyRepo>();

		public LazyClass lazyClass;

		public class FieldRepo
		{
			public FieldSchema fieldSchema;
			public TypeRepo typeRepo;

			public FieldRepo(FieldSchema fieldSchema, TypeRepo typeRepo = null)
			{
				this.fieldSchema = fieldSchema;
				this.typeRepo = typeRepo;
			}

			public override string ToString() => "Field Repo: " + fieldSchema.fieldName;

			public void Load(object obj)
			{
				if (fieldSchema.Loadable)
				{
					object valueObject = typeRepo.LoadObjectRef();
					// todo: 36% of current cpu usage, break into explicit operators? (is that even possible?)
					fieldSchema.fieldInfo.SetValue(obj, valueObject); // else set to null?
				}
				else
				{
					typeRepo.SkipObjectRef();
				}
			}
		}

		public class PropertyRepo
		{
			public PropertySchema propertySchema;
			public TypeRepo typeRepo;
			public LazyProperty lazyProperty;

			public PropertyRepo(PropertySchema propertySchema, TypeRepo typeRepo = null)
			{
				this.propertySchema = propertySchema;
				this.typeRepo = typeRepo;
			}

			public override string ToString()
			{
				return propertySchema.ToString() + " (" + typeRepo.ToString() + ")";
			}

			// Load serialized data into object
			public void Load(object obj)
			{
				if (!propertySchema.Loadable)
				{
					typeRepo.LoadLazyObjectRef(); // skip reference
					if (lazyProperty != null)
						lazyProperty.fieldInfoLoaded.SetValue(obj, true);
				}
				else if (lazyProperty != null)
				{
					TypeRef typeRef = typeRepo.LoadLazyObjectRef();
					lazyProperty.SetTypeRef(obj, typeRef);
				}
				else
				{
					object valueObject = typeRepo.LoadObjectRef();
					// can throw System.ArgumentException, set to null if not Loadable?
					// Should we add exception handling or detect this earlier when we load the schema?
					propertySchema.propertyInfo.SetValue(obj, valueObject);
				}
			}
		}

		public TypeRepoObject(Serializer serializer, TypeSchema typeSchema) : 
			base(serializer, typeSchema)
		{
		}

		public override void InitializeSaving()
		{
			foreach (FieldSchema fieldSchema in typeSchema.FieldSchemas)
			{
				if (!fieldSchema.Serialized)
					continue;

				Type fieldType = fieldSchema.fieldInfo.FieldType.GetNonNullableType();
				//TypeRepo typeRepo = serializer.GetOrCreateRepo(fieldType);
				//fieldSchema.fileDataOffset = writer.BaseStream.Position;
				//if (fieldType != null && serializer.idxTypeToRepo.ContainsKey(fieldType))
				//	fieldSchema.typeIndex = serializer.idxTypeToRepo[fieldType].typeIndex;

				fieldRepos.Add(new FieldRepo(fieldSchema));
			}

			foreach (PropertySchema propertySchema in typeSchema.PropertySchemas)
			{
				if (!propertySchema.Serialized)
					continue;

				Type propertyType = propertySchema.propertyInfo.PropertyType.GetNonNullableType();
				//if (propertyType != null && serializer.idxTypeToRepo.ContainsKey(propertyType))
				//	propertySchema.typeIndex = serializer.idxTypeToRepo[propertyType].typeIndex;

				propertyRepos.Add(new PropertyRepo(propertySchema));
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
				foreach (PropertySchema propertySchema in typeSchema.PropertySchemas)
				{
					if (propertySchema.Loadable == false)
						continue;

					MethodInfo getMethod = propertySchema.propertyInfo.GetGetMethod(false);
					if (getMethod.IsVirtual)
						return true;
				}
				return false;
			}
		}

		public void InitializeFields(Log log)
		{
			foreach (FieldSchema fieldSchema in typeSchema.FieldSchemas)
			{
				//if (fieldSchema.Loadable == false)
				//	continue;

				TypeRepo typeRepo;
				if (fieldSchema.typeIndex >= 0)
				{
					typeRepo = serializer.typeRepos[fieldSchema.typeIndex];
					if (typeRepo.type != fieldSchema.nonNullableType)
					{
						log.Add("Can't load field, type has changed", new Tag("Field", fieldSchema));
						fieldSchema.Loadable = false;
						//continue;
					}
				}
				else
				{
					Type fieldType = fieldSchema.fieldInfo.FieldType.GetNonNullableType();
					typeRepo = serializer.GetOrCreateRepo(log, fieldType);
				}
				fieldSchema.typeSchema = typeRepo.typeSchema;
				//TypeRepo typeRepo = serializer.typeRepos[fieldSchema.typeIndex];
				//if (typeRepo == null)
				//	continue;

				fieldRepos.Add(new FieldRepo(fieldSchema, typeRepo));
			}
		}

		public void InitializeProperties(Log log)
		{
			List<PropertyRepo> lazyPropertyRepos = new List<PropertyRepo>();
			foreach (PropertySchema propertySchema in typeSchema.PropertySchemas)
			{
				//if (propertySchema.Loadable == false)
				//	continue;

				TypeRepo typeRepo;
				if (propertySchema.typeIndex >= 0 || propertySchema.propertyInfo == null)
				{
					typeRepo = serializer.typeRepos[propertySchema.typeIndex];
					if (typeRepo.type != propertySchema.nonNullableType)
					{
						// should we add type conversion here?
						log.Add("Can't load field, type has changed", new Tag("Property", propertySchema));
						propertySchema.Loadable = false;
						//continue;
					}
				}
				else
				{
					// Base Type might not have been serialized
					Type propertyType = propertySchema.propertyInfo.PropertyType.GetNonNullableType();
					typeRepo = serializer.GetOrCreateRepo(log, propertyType);
				}
				propertySchema.propertyTypeSchema = typeRepo.typeSchema;
				if (typeRepo != null)
				{
					PropertyRepo propertyRepo = new PropertyRepo(propertySchema, typeRepo);
					propertyRepos.Add(propertyRepo);

					if (propertySchema.Loadable && !propertySchema.type.IsPrimitive)
						lazyPropertyRepos.Add(propertyRepo);
				}
			}

			// should we add an attribute for this instead?
			if (serializer.lazy && HasVirtualProperty)
			{
				lazyClass = new LazyClass(type, lazyPropertyRepos);
				loadableType = lazyClass.newType;
			}

			/*if (lazyClass != null)
			{
				//if (propertySchema.propertyInfo != null)
					lazyClass.lazyProperties.TryGetValue(propertySchema.propertyInfo, out propertyRepo.lazyProperty);
			}*/
		}

		public override void AddChildObjects( object obj)
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

		protected override object LoadObjectData(byte[] bytes, ref int byteOffset, int objectIndex)
		{
			object obj = Activator.CreateInstance(type, true);
			objects[objectIndex] = obj; // must assign before loading any more refs

			LoadFields(bytes, ref byteOffset, obj);
			LoadProperties(bytes, ref byteOffset, obj);
			return obj;
		}

		private void AddFields(object obj)
		{
			foreach (FieldSchema fieldSchema in typeSchema.FieldSchemas)
			{
				if (!fieldSchema.Serialized)
					continue;

				object fieldValue = fieldSchema.fieldInfo.GetValue(obj);
				serializer.AddObjectRef(fieldValue);
			}
		}

		private void SaveFields(BinaryWriter writer, object obj)
		{
			foreach (FieldRepo fieldRepo in fieldRepos)
			{
				FieldInfo fieldInfo = fieldRepo.fieldSchema.fieldInfo;
				object fieldValue = fieldInfo.GetValue(obj);
				serializer.WriteObjectRef(fieldRepo.fieldSchema.nonNullableType, fieldValue, writer);
			}
		}

		private void LoadFields(object obj)
		{
			foreach (FieldRepo fieldRepo in fieldRepos)
			{
				fieldRepo.Load(obj);
			}
		}

		private void LoadFields(byte[] bytes, ref int byteOffset, object obj)
		{
			foreach (FieldRepo fieldRepo in fieldRepos)
			{
				object valueObject = fieldRepo.typeRepo.LoadObjectRef(bytes, ref byteOffset);
				// todo: 36% of current cpu usage, break into explicit operators? (is that even possible?)
				fieldRepo.fieldSchema.fieldInfo.SetValue(obj, valueObject); // else set to null?
			}
		}

		private void AddProperties(object value)
		{
			foreach (PropertySchema propertySchema in typeSchema.PropertySchemas)
			{
				if (!propertySchema.Serialized)
					continue;

				object propertyValue = propertySchema.propertyInfo.GetValue(value);
				serializer.AddObjectRef(propertyValue);
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
			foreach (PropertyRepo propertyRepo in propertyRepos)
			{
				PropertyInfo propertyInfo = propertyRepo.propertySchema.propertyInfo;
				object propertyValue = propertyInfo.GetValue(obj);
				serializer.WriteObjectRef(propertyRepo.propertySchema.nonNullableType, propertyValue, writer);
			}
		}

		private void LoadProperties(object obj)
		{
			foreach (PropertyRepo propertyRepo in propertyRepos)
			{
				propertyRepo.Load(obj);
			}
		}

		private void LoadProperties(byte[] bytes, ref int byteOffset, object obj)
		{
			foreach (PropertyRepo propertyRepo in propertyRepos)
			{
				object valueObject = propertyRepo.typeRepo.LoadObjectRef(bytes, ref byteOffset);
				propertyRepo.propertySchema.propertyInfo.SetValue(obj, valueObject); // set to null if not Loadable?
			}
		}

		public override void Clone(object source, object dest)
		{
			CloneFields(source, dest);
			CloneProperties(source, dest);
		}

		private void CloneFields(object source, object dest)
		{
			foreach (FieldSchema fieldSchema in typeSchema.FieldSchemas)
			{
				if (!fieldSchema.Serialized)
					continue;

				object propertyValue = fieldSchema.fieldInfo.GetValue(source);
				serializer.AddObjectRef(propertyValue);
				object clone = serializer.Clone(propertyValue);
				fieldSchema.fieldInfo.SetValue(dest, clone);
			}
		}

		private void CloneProperties(object source, object dest)
		{
			foreach (PropertySchema propertySchema in typeSchema.PropertySchemas)
			{
				if (!propertySchema.Serialized)
					continue;

				object propertyValue = propertySchema.propertyInfo.GetValue(source);
				serializer.AddObjectRef(propertyValue);
				object clone = serializer.Clone(propertyValue);
				propertySchema.propertyInfo.SetValue(dest, clone); // else set to null?
			}
		}
	}
}
