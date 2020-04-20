using Atlas.Core;
using System;
using System.IO;
using System.Net;
using System.Text;

namespace Atlas.Network
{
	public class HTTP
	{
		private const int MaxAttempts = 4;
		private const int SleepMilliseconds = 500; // < ^ MaxAttempts
		public Call call;

		public HTTP(Call call)
		{
			this.call = call;
		}

		public virtual string GetString(string uri, string accept = null)
		{
			byte[] bytes = GetResponse(uri, accept);
			if (bytes != null)
				return Encoding.ASCII.GetString(bytes);
			return null;
		}

		public virtual byte[] GetBytes(string uri)
		{
			return GetResponse(uri);
		}

		private byte[] GetResponse(string uri, string accept = null)
		{
			using (CallTimer getCall = call.Timer("Get Uri", new Tag("URI", uri)))
			{
				for (int attempt = 1; ; attempt++)
				{
					HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri); // requests can't be reused between attempts
					request.Method = "GET";
					request.Accept = accept;
					try
					{
						WebResponse response = request.GetResponse();
						Stream dataStream = response.GetResponseStream();

						MemoryStream memoryStream = new MemoryStream();
						dataStream.CopyTo(memoryStream);
						byte[] data = memoryStream.ToArray();
						
						dataStream.Close();
						response.Close();
						getCall.Log.Add("Downloaded HTTP File", new Tag("URI", request.RequestUri), new Tag("Size", memoryStream.Length));

						return data;
					}
					catch (WebException exception)
					{
						getCall.Log.AddError("URI request " + request.RequestUri + " failed: " + exception.Message);

						if (exception.Response != null)
						{
							string response = new StreamReader(exception.Response.GetResponseStream()).ReadToEnd();
							call.Log.AddError(response);
						}
					}
					if (attempt >= MaxAttempts)
						break;
					//System.Threading.Thread.Sleep(SleedMilliseconds * (int)Math.Pow(2, attempt));
					System.Threading.Thread.Sleep(SleepMilliseconds * attempt);
				}
				throw new Exception("HTTP request failed " + MaxAttempts + " times: " + uri);
			}
		}
	}
}
