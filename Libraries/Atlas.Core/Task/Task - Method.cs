using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Atlas.Core
{
	public class TaskMethod : TaskCreator
	{
		private MethodInfo methodInfo;
		private object obj; // object to invoke method for

		public TaskMethod(MethodInfo methodInfo, object obj)
		{
			this.methodInfo = methodInfo;
			this.obj = obj;

			Label = methodInfo.Name;
		}

		public override string ToString()
		{
			return methodInfo.Name;
		}

		protected override Action CreateAction(Call call)
		{
			return () => RunMethod(call);
		}

		private void RunMethod(Call call)
		{
			methodInfo.Invoke(obj, new object[] { call });
		}
	}
}

/*
*/
