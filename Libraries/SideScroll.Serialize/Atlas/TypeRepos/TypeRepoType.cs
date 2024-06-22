using System.Reflection;

namespace SideScroll.Serialize;

public class TypeRepoType(Serializer serializer, TypeSchema typeSchema) : TypeRepo(serializer, typeSchema)
{
	public class Creator : IRepoCreator
	{
		public TypeRepo? TryCreateRepo(Serializer serializer, TypeSchema typeSchema)
		{
			if (typeof(Type).IsAssignableFrom(typeSchema.Type))
				return new TypeRepoType(serializer, typeSchema);
			return null;
		}
	}

	public override void SaveObject(BinaryWriter writer, object obj)
	{
		writer.Write(((Type)obj).AssemblyQualifiedName!);
	}

	protected override object? CreateObject(int objectIndex)
	{
		long position = Reader!.BaseStream.Position;
		Reader.BaseStream.Position = ObjectOffsets![objectIndex];

		string assemblyQualifiedName = Reader.ReadString();
		//object obj = Type.GetType(assemblyQualifiedName, false);
		object? obj = Type.GetType(assemblyQualifiedName, AssemblyResolver, null);
		Reader.BaseStream.Position = position;

		ObjectsLoaded[objectIndex] = obj; // must assign before loading any more refs
		return obj;
	}

	// ignore Assembly version to allow loading shared 
	private static Assembly AssemblyResolver(AssemblyName assemblyName)
	{
		assemblyName.Version = null;
		return Assembly.Load(assemblyName);
	}

	public override void Clone(object source, object dest)
	{
		// assigning won't do anything since it's not a ref
		throw new Exception("Not cloneable");
	}
}
