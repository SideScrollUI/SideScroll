using Atlas.Core;

namespace Atlas.Serialize
{
	public static class AtlasExtensions
	{
		public static T Clone<T>(this object obj, Call call = null)
		{
			call = call ?? new Call();
			return SerializerMemory.Clone<T>(call, obj);
		}
	}
}
