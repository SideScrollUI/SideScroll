using System;

namespace Atlas.Core
{
	public class TestBase
	{
		public Call call;

		public virtual void Initialize(string name)
		{
			call = new Call(name);
		}
	}
}
