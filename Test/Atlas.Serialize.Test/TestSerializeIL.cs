using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Atlas.Serialize.Test
{
	[Category("SerializeIL")]
	public class SerializeIL : TestSerializeBase
	{
		[Test, Description("Serialize Lazy Base")]
		public void ILMethod()
		{
			var lazyClass = new LazyClass();

			object obj = Activator.CreateInstance(lazyClass.NewType, true);
			FieldInfo fieldInfo = lazyClass.NewType.GetField("typeRef");
			fieldInfo.SetValue(obj, new TypeRef());

			PropertyInfo propertyInfo = lazyClass.NewType.GetProperty("prop");
			object result = propertyInfo.GetValue(obj);
		}
	}

	public class TypeRef
	{
		public int Load()
		{
			return 2;
		}
	}

	public class PropertyRef
	{
		public TypeRepo TypeRepo;
		public PropertyBuilder PropertyBuilder;
		public FieldBuilder FieldBuilderTypeRef;
	}

	public class LazyClass
	{
		public Type NewType;
		public Dictionary<PropertyInfo, PropertyRef> PropertyRefs = new Dictionary<PropertyInfo, PropertyRef>();

		public LazyClass()
		{
			NewType = CompileResultType();
		}

		public Type CompileResultType()
		{
			TypeBuilder typeBuilder = GetTypeBuilder();
			ConstructorBuilder constructor = typeBuilder.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);

			CreateProperty(typeBuilder);

			TypeInfo objectType = typeBuilder.CreateTypeInfo();
			return objectType;
		}

		private TypeBuilder GetTypeBuilder()
		{
			string typeSignature = "LoaderType";
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
					null);
			return typeBuilder;
		}

		private void CreateProperty(TypeBuilder typeBuilder)
		{
			string propertyName = "prop";
			Type propertyType = typeof(int);

			FieldBuilder fieldBuilderValue = typeBuilder.DefineField("_" + propertyName, propertyType, FieldAttributes.Private);
			FieldBuilder fieldBuilderTypeRef = typeBuilder.DefineField("typeRef", typeof(TypeRef), FieldAttributes.Public);
			MethodInfo methodInfoLoad = typeof(TypeRef).GetMethod(nameof(TypeRef.Load));

			PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);

			// GET

			MethodBuilder getPropertyMethodBuilder = typeBuilder.DefineMethod("get_" + propertyName, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, propertyType, Type.EmptyTypes);
			ILGenerator getIl = getPropertyMethodBuilder.GetILGenerator();

			// load value into field
			//getIl.Emit(OpCodes.Ldarg_0); // load this

			//getIl.Emit(OpCodes.Ldnull); // load 1 (true)

			getIl.Emit(OpCodes.Ldarg_0); // load this
			//getIl.Emit(OpCodes.Ldarg_0); // load this
			getIl.Emit(OpCodes.Ldfld, fieldBuilderTypeRef);
			//getIl.Emit(OpCodes.Ldarg_1);
			getIl.Emit(OpCodes.Call, methodInfoLoad);
			getIl.Emit(OpCodes.Ret);
			//getIl.Emit(OpCodes.Pop);

			/*getIl.Emit(OpCodes.Stfld, fieldBuilderValue); // save value to field

			// todo: return value from inner property

			getIl.Emit(OpCodes.Ldarg_0); // load this
			getIl.Emit(OpCodes.Ldfld, fieldBuilderValue);
			getIl.Emit(OpCodes.Ret);*/

			propertyBuilder.SetGetMethod(getPropertyMethodBuilder);
		}
	}
}
