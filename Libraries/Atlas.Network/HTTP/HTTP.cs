using Atlas.Core;
using System;
using System.IO;
using System.Net;
using System.Text;

namespace Atlas.Network
{
	public class HTTP
	{
		private const int MaxAttempts = 5;

		public Call call;

		public HTTP(Call call)
		{
			this.call = call;
		}

		public virtual string GetString(string uri)
		{
			byte[] bytes = GetBytes(uri);
			if (bytes != null)
				return Encoding.ASCII.GetString(bytes);
			return null;
		}

		public virtual byte[] GetBytes(string uri)
		{
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
			request.Method = "GET";

			return GetResponse(request);
		}

		private byte[] GetResponse(HttpWebRequest request)
		{
			using (CallTimer getCall = call.Timer("Downloading HTTP File", new Tag("URI", request.RequestUri)))
			{
				for (int attempt = 1; attempt <= MaxAttempts; attempt++)
				{
					try
					{
						WebResponse response = request.GetResponse();
						Stream dataStream = response.GetResponseStream();

						MemoryStream memoryStream = new MemoryStream();
						dataStream.CopyTo(memoryStream);
						byte[] data = memoryStream.ToArray();
						
						dataStream.Close();
						response.Close();
						getCall.log.Add("Downloaded HTTP File", new Tag("URI", request.RequestUri), new Tag("Size", memoryStream.Length));

						return data;
					}
					catch (WebException exception)
					{
						getCall.log.AddError("URI request " + request.RequestUri + " failed: " + exception.Message);

						if (exception.Response != null)
						{
							string response = new StreamReader(exception.Response.GetResponseStream()).ReadToEnd();
							call.log.AddError(response);
						}
					}
					System.Threading.Thread.Sleep(3000 * (int)Math.Pow(2, attempt));
				}
				throw new Exception("HTTP request failed " + MaxAttempts.ToString() + " times: " + request.RequestUri);
			}
		}
	}
}
