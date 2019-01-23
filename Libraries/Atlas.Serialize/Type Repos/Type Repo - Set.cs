using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Atlas.Serialize
{
	/*public class TypeRepoSet : TypeBase
	{
		public TypeRepoSet(Log log, TypeSchema typeSchema) : 
			base(log, typeSchema)
		{
		}

		public override void SaveObjects(Container container, BinaryWriter writer)
		{
			TypeBase listTypeSave = null;
			//if (typeSchema.listTypeIndex1 < 0)
			//	return;
			
			foreach (ISet list in objects)
			{
				writer.Write((int)list.Count);
				foreach (var item in list)
				{
					if (listTypeSave == null)
					{
						listTypeSave = container.idxTypeToBase[item.GetType()];
						typeSchema.listTypeIndex1 = listTypeSave.typeIndex;
					}
					listTypeSave.SaveObjectRef(item, writer);
				}
			}
		}

		public override void LoadObjects(Container container, BinaryReader reader)
		{
			if (typeSchema.listTypeIndex1 < 0)
				return;

			TypeInstance listTypeLoad = container.typeRepos[typeSchema.listTypeIndex1];
			for (int i = 0; i < objects.Count; i++)
			{
				ISet iList = (ISet)objects[i];
				int count = BitConverter.ToInt32(bytes, byteOffset);
				for (int j = 0; j < count; j++)
				{
					object obj = listTypeLoad.LoadObjectRef(bytes, ref byteOffset);
					iList.Add(obj);
				}
			}
		}
	}*/
}
/*

*/
