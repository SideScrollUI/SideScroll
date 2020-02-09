using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Atlas.Core
{
	public interface ILoadAsync
	{
		Task<object> LoadAsync(Call call);
	}
}
