using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Atlas.Serialize
{
	public class TypeRef
	{
		public TypeRepo typeRepo;
		public int index;

		public object Load()
		{
			return typeRepo.LoadFullObject(index);
		}
	}

	public class LazyProperty
	{
		//public TypeRepo typeRepo;
		public PropertyBuilder propertyBuilder;
		public PropertyInfo propertyInfoOriginal;
		public PropertyInfo propertyInfoOverride;
		public FieldBuilder fieldBuilderLoaded;
		public FieldBuilder fieldBuilderTypeRef;
		public FieldInfo fieldInfoLoaded;
		public FieldInfo fieldInfoTypeRef;

		public override string ToString() => propertyBuilder.Name;

		public void SetTypeRef(object obj, TypeRef typeRef)
		{
			if (typeRef == null)
			{
				fieldInfoLoaded.SetValue(obj, true);
			}
			else
			{
				fieldInfoTypeRef.SetValue(obj, typeRef);
			}
		}
	}

	public class LazyClass
	{
		public Type originalType;
		public Type newType;
		public Dictionary<PropertyInfo, LazyProperty> lazyProperties = new Dictionary<PropertyInfo, LazyProperty>();

		public LazyClass(Type type, List<TypeRepoObject.PropertyRepo> propertyRepos)
		{
			originalType = type;

			newType = CreateLazyType(propertyRepos);

			foreach (LazyProperty lazyProperty in lazyProperties.Values)
			{
				lazyProperty.propertyInfoOverride = newType.GetProperty(lazyProperty.propertyInfoOriginal.Name);
				lazyProperty.fieldInfoLoaded = newType.GetField(lazyProperty.fieldBuilderLoaded.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				lazyProperty.fieldInfoTypeRef = newType.GetField(lazyProperty.fieldBuilderTypeRef.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				Debug.Assert(lazyProperty.fieldInfoLoaded != null);
				Debug.Assert(lazyProperty.fieldBuilderTypeRef != null);
			}
		}

		public TypeInfo CreateLazyType(List<TypeRepoObject.PropertyRepo> propertyRepos)
		{
			TypeBuilder typeBuilder = GetTypeBuilder();
			ConstructorBuilder constructor = typeBuilder.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);

			PropertyInfo[] propertyInfos = originalType.GetProperties().OrderBy(x => x.MetadataToken).ToArray();

			foreach (TypeRepoObject.PropertyRepo propertyRepo in propertyRepos)
			{
				PropertyInfo propertyInfo = propertyRepo.propertySchema.propertyInfo;
				if (propertyInfo.CanRead == false || propertyInfo.CanWrite == false)
					continue;

				MethodInfo getMethod = propertyInfo.GetGetMethod(false);
				if (getMethod.IsVirtual)
					propertyRepo.lazyProperty = CreateLazyProperty(typeBuilder, propertyInfo);
			}

			TypeInfo objectType = typeBuilder.CreateTypeInfo();
			return objectType;
		}

		private TypeBuilder GetTypeBuilder()
		{
			string typeSignature = "Lazy." + originalType.FullName;
			AssemblyName assemblyName = new AssemblyName(typeSignature);
			AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
			ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("Lazy");
			TypeBuilder typeBuilder = moduleBuilder.DefineType(typeSignature,
					TypeAttributes.Public |
					TypeAttributes.Class |
					TypeAttributes.AutoClass |
					TypeAttributes.AnsiClass |
					TypeAttributes.BeforeFieldInit |
					TypeAttributes.AutoLayout,
					originalType);
			return typeBuilder;
		}

		private LazyProperty CreateLazyProperty(TypeBuilder typeBuilder, PropertyInfo propertyInfo)
		{
			string propertyName = propertyInfo.Name;
			Type propertyType = propertyInfo.PropertyType;

			MethodInfo setMethod = propertyInfo.GetSetMethod(true);
			MethodInfo getMethod = propertyInfo.GetGetMethod(true);
			FieldBuilder fieldBuilderLoaded = typeBuilder.DefineField("_" + propertyName + "Loaded", typeof(bool), FieldAttributes.Family);
			FieldBuilder fieldBuilderTypeRef = typeBuilder.DefineField("_" + propertyName + "TypeRef", typeof(TypeRef), FieldAttributes.Family);
			MethodInfo methodInfoLoad = typeof(TypeRef).GetMethod(nameof(TypeRef.Load));

			PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);

			// GET

			MethodBuilder getPropertyMethodBuilder = typeBuilder.DefineMethod("get_" + propertyName,
				MethodAttributes.Public |
				MethodAttributes.Virtual |
				MethodAttributes.SpecialName |
				MethodAttributes.HideBySig,
				propertyType, Type.EmptyTypes);
			ILGenerator getIl = getPropertyMethodBuilder.GetILGenerator();

			Label returnValue = getIl.DefineLabel();

			// check result and jump to Ret if are equals
			getIl.Emit(OpCodes.Ldarg_0); // load this
			getIl.Emit(OpCodes.Ldfld, fieldBuilderLoaded);
			getIl.Emit(OpCodes.Brtrue_S, returnValue);

			// set IsModified to true
			getIl.Emit(OpCodes.Ldarg_0); // load this
			getIl.Emit(OpCodes.Ldc_I4_1); // load 1 (true)
			getIl.Emit(OpCodes.Stfld, fieldBuilderLoaded);

			// save value to inner property

			// load value into field
			getIl.Emit(OpCodes.Ldarg_0); // load this

			getIl.Emit(OpCodes.Ldarg_0); // load this
			getIl.Emit(OpCodes.Ldfld, fieldBuilderTypeRef);
			getIl.Emit(OpCodes.Call, methodInfoLoad);

			getIl.Emit(OpCodes.Call, setMethod);

			// set TypeRef to null to free memory
			getIl.Emit(OpCodes.Ldarg_0); // load this
			getIl.Emit(OpCodes.Ldnull);
			getIl.Emit(OpCodes.Stfld, fieldBuilderTypeRef);

			// return loaded field
			getIl.MarkLabel(returnValue);

			// return value from inner property

			getIl.Emit(OpCodes.Ldarg_0); // load this
			getIl.Emit(OpCodes.Call, getMethod);
			getIl.Emit(OpCodes.Ret);


			// SET

			MethodBuilder setPropertyMethodBuilder =
				typeBuilder.DefineMethod("set_" + propertyName,
				  MethodAttributes.Public |
				  MethodAttributes.Virtual |
				  MethodAttributes.SpecialName |
				  MethodAttributes.HideBySig,
				  null, new[] { propertyType });

			ILGenerator setIl = setPropertyMethodBuilder.GetILGenerator();

			// set IsModified to true
			setIl.Emit(OpCodes.Ldarg_0); // load this
			setIl.Emit(OpCodes.Ldc_I4_1); // load 1 (true)
			setIl.Emit(OpCodes.Stfld, fieldBuilderLoaded);

			// save value to inner property

			// set value
			setIl.Emit(OpCodes.Ldarg_0); // load this
			setIl.Emit(OpCodes.Ldarg_1); // load value
			setIl.Emit(OpCodes.Call, setMethod);

			setIl.Emit(OpCodes.Ret);

			propertyBuilder.SetGetMethod(getPropertyMethodBuilder);
			propertyBuilder.SetSetMethod(setPropertyMethodBuilder);

			var lazyProperty = new LazyProperty()
			{
				propertyInfoOriginal = propertyInfo,
				propertyBuilder = propertyBuilder,
				fieldBuilderTypeRef = fieldBuilderTypeRef,
				fieldBuilderLoaded = fieldBuilderLoaded,
			};
			lazyProperties[propertyInfo] = lazyProperty;

			return lazyProperty;
		}
	}
}

/*

Load everything at once?
	What if they call set first?
		Also load during a set?
Load individually
	each property requires a 
		set
		index
		type repo
	don't want to load 25k objects automatically

[LazySerialization]
public class MyClass
{
	public virtual int Age { get; set; }
}

public class Wrapper : MyClass
{
	//private TypeRepo typeRepo;
	//private int objectIndex;


	//private bool set = false;
	private TypeRef typeRef;
	//private int _Age = 0;
	public override int Age
	{
		get
		{
			if (!set)
			{
				set = true;
				_Age = typeRef.Load();
			}
			return _Age;
		}
		set
		{
			_Age = value;
			set = true;
		}
	}
}
*/
