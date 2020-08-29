﻿using Atlas.Core;
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

		public List<FieldRepo> FieldRepos = new List<FieldRepo>();
		public List<PropertyRepo> PropertyRepos = new List<PropertyRepo>();

		public LazyClass LazyClass;

		public class FieldRepo
		{
			public FieldSchema FieldSchema;
			public TypeRepo TypeRepo;

			public FieldRepo(FieldSchema fieldSchema, TypeRepo typeRepo = null)
			{
				FieldSchema = fieldSchema;
				TypeRepo = typeRepo;
			}

			public override string ToString() => "Field Repo: " + FieldSchema.FieldName;

			public void Load(object obj)
			{
				if (FieldSchema.Loadable)
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

			public PropertyRepo(PropertySchema propertySchema, TypeRepo typeRepo = null)
			{
				PropertySchema = propertySchema;
				TypeRepo = typeRepo;
			}

			public override string ToString()
			{
				return PropertySchema.ToString() + " (" + TypeRepo.ToString() + ")";
			}

			// Load serialized data into object
			public void Load(object obj)
			{
				if (!PropertySchema.Loadable)
				{
					TypeRepo.LoadLazyObjectRef(); // skip reference
					if (LazyProperty != null)
						LazyProperty.fieldInfoLoaded.SetValue(obj, true);
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
				if (!fieldSchema.Serialized)
					continue;

				Type fieldType = fieldSchema.FieldInfo.FieldType.GetNonNullableType();
				//TypeRepo typeRepo = serializer.GetOrCreateRepo(fieldType);
				//fieldSchema.fileDataOffset = writer.BaseStream.Position;
				//if (fieldType != null && serializer.idxTypeToRepo.ContainsKey(fieldType))
				//	fieldSchema.typeIndex = serializer.idxTypeToRepo[fieldType].typeIndex;

				FieldRepos.Add(new FieldRepo(fieldSchema));
			}

			foreach (PropertySchema propertySchema in TypeSchema.PropertySchemas)
			{
				if (!propertySchema.Serialized)
					continue;

				Type propertyType = propertySchema.PropertyInfo.PropertyType.GetNonNullableType();
				//if (propertyType != null && serializer.idxTypeToRepo.ContainsKey(propertyType))
				//	propertySchema.typeIndex = serializer.idxTypeToRepo[propertyType].typeIndex;

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
					if (propertySchema.Loadable == false)
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
						fieldSchema.Loadable = false;
						//continue;
					}
				}
				else
				{
					Type fieldType = fieldSchema.FieldInfo.FieldType.GetNonNullableType();
					typeRepo = Serializer.GetOrCreateRepo(log, fieldType);
				}
				fieldSchema.TypeSchema = typeRepo.TypeSchema;
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
						propertySchema.Loadable = false;
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

					if (propertySchema.Loadable && !propertySchema.Type.IsPrimitive)
						lazyPropertyRepos.Add(propertyRepo);
				}
			}

			// should we add an attribute for this instead?
			if (Serializer.Lazy && HasVirtualProperty)
			{
				LazyClass = new LazyClass(Type, lazyPropertyRepos);
				LoadableType = LazyClass.newType;
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
			object obj = Activator.CreateInstance(Type, true);
			Objects[objectIndex] = obj; // must assign before loading any more refs

			LoadFields(bytes, ref byteOffset, obj);
			LoadProperties(bytes, ref byteOffset, obj);
			return obj;
		}

		private void AddFields(object obj)
		{
			foreach (FieldSchema fieldSchema in TypeSchema.FieldSchemas)
			{
				if (!fieldSchema.Serialized)
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

		private void LoadFields(byte[] bytes, ref int byteOffset, object obj)
		{
			foreach (FieldRepo fieldRepo in FieldRepos)
			{
				object valueObject = fieldRepo.TypeRepo.LoadObjectRef(bytes, ref byteOffset);
				// todo: 36% of current cpu usage, break into explicit operators? (is that even possible?)
				fieldRepo.FieldSchema.FieldInfo.SetValue(obj, valueObject); // else set to null?
			}
		}

		private void AddProperties(object value)
		{
			foreach (PropertySchema propertySchema in TypeSchema.PropertySchemas)
			{
				if (!propertySchema.Serialized)
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

		private void LoadProperties(byte[] bytes, ref int byteOffset, object obj)
		{
			foreach (PropertyRepo propertyRepo in PropertyRepos)
			{
				object valueObject = propertyRepo.TypeRepo.LoadObjectRef(bytes, ref byteOffset);
				propertyRepo.PropertySchema.PropertyInfo.SetValue(obj, valueObject); // set to null if not Loadable?
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
				if (!fieldSchema.Serialized)
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
				if (!propertySchema.Serialized)
					continue;

				object propertyValue = propertySchema.PropertyInfo.GetValue(source);
				Serializer.AddObjectRef(propertyValue);
				object clone = Serializer.Clone(propertyValue);
				propertySchema.PropertyInfo.SetValue(dest, clone); // else set to null?
			}
		}
	}
}
