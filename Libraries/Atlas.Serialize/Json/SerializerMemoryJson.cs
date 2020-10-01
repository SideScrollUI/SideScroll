using Atlas.Core;
using System;
using System.IO;
using Newtonsoft.Json;
using System.Text;

namespace Atlas.Serialize
{
	public class SerializerMemoryJson : SerializerMemory
	{
		//private MemoryStream stream = new MemoryStream();
		private string _json;

		public SerializerMemoryJson()
		{
		}

		public override void Save(Call call, object obj)
		{
			_json = JsonConvert.SerializeObject(obj); // pass obj.GetType()?
		}

		public override T Load<T>(Call call = null)
		{
			return JsonConvert.DeserializeObject<T>(_json);
		}

		public override object Load(Call call = null)
		{
			return JsonConvert.DeserializeObject(_json);
		}

		//public static T Clone<T>(Call call, T obj)
		public override T DeepCloneInternal<T>(Call call, object obj)
		{
			Save(call, obj);
			T copy = Load<T>(call);
			return copy;
		}

		public override object DeepCloneInternal(Call call, object obj)
		{
			Save(call, obj);
			object copy = Load(call);
			return copy;
		}
	}
}
