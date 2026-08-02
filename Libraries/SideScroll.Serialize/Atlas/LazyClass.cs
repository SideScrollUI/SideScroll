using SideScroll.Serialize.Atlas.TypeRepos;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;

namespace SideScroll.Serialize.Atlas;

/// <summary>
/// Represents a reference to a serialized type and its index
/// </summary>
public class TypeRef
{
	/// <summary>
	/// Gets or sets the type repository containing the object
	/// </summary>
	public TypeRepo? TypeRepo;

	/// <summary>
	/// Gets or sets the index of the object within the type repository
	/// </summary>
	public int Index;

	/// <summary>
	/// Loads the full object from the type repository
	/// </summary>
	public object? Load()
	{
		return TypeRepo?.LoadFullObject(Index);
	}
}

/// <summary>
/// Represents a lazily-loaded property with metadata for dynamic type generation
/// </summary>
public class LazyProperty
{
	/// <summary>
	/// Gets or sets the property builder for the lazy property
	/// </summary>
	public PropertyBuilder? PropertyBuilder;

	/// <summary>
	/// Gets or sets the original property info from the base type
	/// </summary>
	public PropertyInfo? PropertyInfoOriginal;

	/// <summary>
	/// Gets or sets the overridden property info from the lazy type
	/// </summary>
	public PropertyInfo? PropertyInfoOverride;

	/// <summary>
	/// Gets or sets the field builder for tracking whether the property has been loaded
	/// </summary>
	public FieldBuilder? FieldBuilderLoaded;

	/// <summary>
	/// Gets or sets the field builder for storing the type reference
	/// </summary>
	public FieldBuilder? FieldBuilderTypeRef;

	/// <summary>
	/// Gets or sets the field info for the loaded flag
	/// </summary>
	public FieldInfo? FieldInfoLoaded;

	/// <summary>
	/// Gets or sets the field info for the type reference
	/// </summary>
	public FieldInfo? FieldInfoTypeRef;

	/// <summary>Returns the property's name.</summary>
	public override string? ToString() => PropertyBuilder?.Name;

	/// <summary>
	/// Sets the type reference for lazy loading or marks the property as already loaded
	/// </summary>
	public void SetTypeRef(object obj, TypeRef? typeRef)
	{
		if (typeRef == null)
		{
			FieldInfoLoaded!.SetValue(obj, true);
		}
		else
		{
			FieldInfoTypeRef!.SetValue(obj, typeRef);
		}
	}
}

/// <summary>
/// Generates and manages dynamically created lazy-loading wrapper types
/// </summary>
public class LazyClass
{
	/// <summary>
	/// Gets the original type being wrapped
	/// </summary>
	public Type OriginalType { get; }

	/// <summary>
	/// Gets the dynamically generated lazy wrapper type
	/// </summary>
	public Type LazyType { get; }

	/// <summary>
	/// Gets the dictionary mapping original properties to their lazy property metadata
	/// </summary>
	public Dictionary<PropertyInfo, LazyProperty> LazyProperties { get; } = [];

	// Each instance emits a new dynamic assembly that can never be unloaded, so reuse them.
	// The generated type only overrides the properties it was created for, so a different
	// set of properties (i.e. a file saved by another version) needs its own type
	private static readonly Dictionary<(Type Type, string Properties), LazyClass> LazyClasses = [];

	/// <summary>
	/// Returns a cached LazyClass for the type and properties, creating it if it doesn't exist yet
	/// </summary>
	public static LazyClass GetOrCreate(Type type, List<TypeRepoObject.PropertyRepo> propertyRepos)
	{
		var key = (type, string.Join(',', propertyRepos.Select(p => p.PropertySchema.Name)));

		lock (LazyClasses)
		{
			if (LazyClasses.TryGetValue(key, out LazyClass? lazyClass))
			{
				lazyClass.UpdatePropertyRepos(propertyRepos);
				return lazyClass;
			}

			lazyClass = new LazyClass(type, propertyRepos);
			LazyClasses.Add(key, lazyClass);
			return lazyClass;
		}
	}

	// The constructor assigns these while generating the type, so a reused LazyClass
	// has to assign them to each new load's PropertyRepos
	private void UpdatePropertyRepos(List<TypeRepoObject.PropertyRepo> propertyRepos)
	{
		foreach (TypeRepoObject.PropertyRepo propertyRepo in propertyRepos)
		{
			if (LazyProperties.TryGetValue(propertyRepo.PropertySchema.PropertyInfo, out LazyProperty? lazyProperty))
			{
				propertyRepo.LazyProperty = lazyProperty;
			}
		}
	}

	/// <summary>
	/// Initializes a new instance of the LazyClass for the specified type
	/// </summary>
	public LazyClass(Type type, List<TypeRepoObject.PropertyRepo> propertyRepos)
	{
		OriginalType = type;

		LazyType = CreateLazyType(propertyRepos);

		foreach (LazyProperty lazyProperty in LazyProperties.Values)
		{
			lazyProperty.PropertyInfoOverride = LazyType.GetProperty(lazyProperty.PropertyInfoOriginal!.Name);
			lazyProperty.FieldInfoLoaded = LazyType.GetField(lazyProperty.FieldBuilderLoaded!.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			lazyProperty.FieldInfoTypeRef = LazyType.GetField(lazyProperty.FieldBuilderTypeRef!.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			Debug.Assert(lazyProperty.FieldInfoLoaded != null);
			Debug.Assert(lazyProperty.FieldInfoTypeRef != null);
		}
	}

	/// <summary>Emits a dynamic subclass of the original type that adds lazy-loading backing fields for the given properties.</summary>
	/// <param name="propertyRepos">The properties to generate lazy-loading members for.</param>
	protected TypeInfo CreateLazyType(List<TypeRepoObject.PropertyRepo> propertyRepos)
	{
		TypeBuilder typeBuilder = GetTypeBuilder();
		// ConstructorBuilder constructor = typeBuilder.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);

		// PropertyInfo[] propertyInfos = OriginalType.GetProperties().OrderBy(x => x.MetadataToken).ToArray();

		foreach (TypeRepoObject.PropertyRepo propertyRepo in propertyRepos)
		{
			PropertyInfo propertyInfo = propertyRepo.PropertySchema.PropertyInfo;
			if (!propertyInfo.CanRead || !propertyInfo.CanWrite)
				continue;

			MethodInfo getMethod = propertyInfo.GetGetMethod(false)!;
			if (getMethod.IsVirtual)
			{
				propertyRepo.LazyProperty = CreateLazyProperty(typeBuilder, propertyInfo);
			}
		}

		TypeInfo objectType = typeBuilder.CreateTypeInfo();
		return objectType;
	}

	private TypeBuilder GetTypeBuilder()
	{
		string typeSignature = "Lazy." + OriginalType.FullName;
		AssemblyName assemblyName = new(typeSignature);
		AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
		ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("Lazy");
		TypeBuilder typeBuilder = moduleBuilder.DefineType(typeSignature,
			TypeAttributes.Public |
			TypeAttributes.Class |
			TypeAttributes.AutoClass |
			TypeAttributes.AnsiClass |
			TypeAttributes.BeforeFieldInit |
			TypeAttributes.AutoLayout,
			OriginalType);
		return typeBuilder;
	}

	private LazyProperty CreateLazyProperty(TypeBuilder typeBuilder, PropertyInfo propertyInfo)
	{
		string propertyName = propertyInfo.Name;
		Type propertyType = propertyInfo.PropertyType;

		MethodInfo setMethod = propertyInfo.GetSetMethod(true)!;
		MethodInfo getMethod = propertyInfo.GetGetMethod(true)!;
		FieldBuilder fieldBuilderLoaded = typeBuilder.DefineField("_" + propertyName + "Loaded", typeof(bool), FieldAttributes.Family);
		FieldBuilder fieldBuilderTypeRef = typeBuilder.DefineField("_" + propertyName + "TypeRef", typeof(TypeRef), FieldAttributes.Family);
		MethodInfo methodInfoLoad = typeof(TypeRef).GetMethod(nameof(TypeRef.Load))!;

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
		LocalBuilder localTypeRef = getIl.DeclareLocal(typeof(TypeRef));

		// check result and jump to Ret if are equals
		getIl.Emit(OpCodes.Ldarg_0); // load this
		getIl.Emit(OpCodes.Ldfld, fieldBuilderLoaded);
		getIl.Emit(OpCodes.Brtrue, returnValue);

		// Return the current value if there's no TypeRef to load, which happens when loading
		// never reached this property. Calling Load() on it would throw a NullReferenceException.
		// load TypeRef into a local variable for thread safety
		getIl.Emit(OpCodes.Ldarg_0); // load this
		getIl.Emit(OpCodes.Ldfld, fieldBuilderTypeRef);
		getIl.Emit(OpCodes.Stloc, localTypeRef);

		// check if localTypeRef is null
		getIl.Emit(OpCodes.Ldloc, localTypeRef);
		getIl.Emit(OpCodes.Brfalse, returnValue);

		// save value to inner property

		// load value into field
		getIl.Emit(OpCodes.Ldarg_0); // load this

		getIl.Emit(OpCodes.Ldloc, localTypeRef); // load localTypeRef
		getIl.Emit(OpCodes.Call, methodInfoLoad);

		// Load() returns an object, convert it before calling a setter that takes anything else.
		// Value types have to be unboxed, passing the boxed reference through is undefined behavior
		if (propertyType.IsValueType)
		{
			getIl.Emit(OpCodes.Unbox_Any, propertyType);
		}
		else if (propertyType != typeof(object))
		{
			getIl.Emit(OpCodes.Castclass, propertyType);
		}

		getIl.Emit(OpCodes.Call, setMethod);

		// set IsModified to true
		getIl.Emit(OpCodes.Ldarg_0); // load this
		getIl.Emit(OpCodes.Ldc_I4_1); // load 1 (true)
		getIl.Emit(OpCodes.Stfld, fieldBuilderLoaded);

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

		// set value
		setIl.Emit(OpCodes.Ldarg_0); // load this
		setIl.Emit(OpCodes.Ldarg_1); // load value
		setIl.Emit(OpCodes.Call, setMethod);

		// set IsModified to true
		setIl.Emit(OpCodes.Ldarg_0); // load this
		setIl.Emit(OpCodes.Ldc_I4_1); // load 1 (true)
		setIl.Emit(OpCodes.Stfld, fieldBuilderLoaded);

		setIl.Emit(OpCodes.Ret);

		propertyBuilder.SetGetMethod(getPropertyMethodBuilder);
		propertyBuilder.SetSetMethod(setPropertyMethodBuilder);

		var lazyProperty = new LazyProperty
		{
			PropertyInfoOriginal = propertyInfo,
			PropertyBuilder = propertyBuilder,
			FieldBuilderTypeRef = fieldBuilderTypeRef,
			FieldBuilderLoaded = fieldBuilderLoaded,
		};
		LazyProperties[propertyInfo] = lazyProperty;

		return lazyProperty;
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
