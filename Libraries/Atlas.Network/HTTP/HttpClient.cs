using Atlas.Core;
using System;
using System.IO;
using System.Net;
using System.Text;

namespace Atlas.Network
{
	public class HttpClient
	{
		public static int MaxAttempts { get; set; } = 5;
		public static int SleepMilliseconds = 500; // < ^ MaxAttempts

		public static string GetString(Call call, string uri)
		{
			byte[] bytes = GetBytes(call, uri)?.Bytes;
			if (bytes != null)
				return Encoding.ASCII.GetString(bytes);

			return null;
		}

		public class ByteResponse
		{
			public WebResponse Response { get; set; }
			public byte[] Bytes { get; set; }

			public ByteResponse() {}

			public ByteResponse(WebResponse response, byte[] bytes)
			{
				Response = response;
				Bytes = bytes;
			}
		}

		public static ByteResponse GetBytes(Call call, string uri)
		{
			using (CallTimer getCall = call.Timer("Get Uri", new Tag("Uri", uri)))
			{
				for (int attempt = 1; attempt <= MaxAttempts; attempt++)
				{
					if (attempt > 1)
						System.Threading.Thread.Sleep(SleepMilliseconds * (int)Math.Pow(2, attempt));

					HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri); // requests can't be reused between attempts
					request.Method = "GET";
					try
					{
						WebResponse response = request.GetResponse();
						Stream dataStream = response.GetResponseStream();

						var memoryStream = new MemoryStream();
						dataStream.CopyTo(memoryStream);
						byte[] data = memoryStream.ToArray();

						dataStream.Close();
						//response.Close(); // We want the Headers still (might need to copy them elsewhere if this causes problems)
						call.Log.Add("Uri Response", new Tag("Uri", request.RequestUri), new Tag("Size", memoryStream.Length));

						return new ByteResponse(response, data);
					}
					catch (WebException exception)
					{
						getCall.Log.Add(exception);

						if (exception.Response != null)
						{
							string response = new StreamReader(exception.Response.GetResponseStream()).ReadToEnd();
							getCall.Log.AddError("Exception: " + response);
						}

						if (exception.Status == WebExceptionStatus.ProtocolError)
							break;
					}
				}
				return null;
			}
		}

		public static WebResponse GetHead(Call call, string uri)
		{
			using (CallTimer headCall = call.Timer("Head Uri", new Tag("Uri", uri)))
			{
				for (int attempt = 1; attempt <= MaxAttempts; attempt++)
				{
					if (attempt > 1)
						System.Threading.Thread.Sleep(SleepMilliseconds * (int)Math.Pow(2, attempt));

					HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri); // requests can't be reused between attempts
					request.Method = "HEAD";
					try
					{
						WebResponse response = request.GetResponse();
						//response.Close();
						call.Log.Add("Uri Response", new Tag("Uri", request.RequestUri), new Tag("Response", response));

						return response;
					}
					catch (WebException exception)
					{
						headCall.Log.Add(exception);

						if (exception.Response != null)
						{
							string response = new StreamReader(exception.Response.GetResponseStream()).ReadToEnd();
							headCall.Log.AddError("Exception: " + response);
						}

						if (exception.Status == WebExceptionStatus.ProtocolError)
							break;
					}
				}
				return null;
			}
		}
	}
}
