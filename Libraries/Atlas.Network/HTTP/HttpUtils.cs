using Atlas.Core;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Atlas.Network
{
	public class HttpUtils
	{
		public static int MaxAttempts { get; set; } = 5;
		public static int SleepMilliseconds = 500; // < ^ MaxAttempts

		public static readonly HttpClient Client = new();

		public static string GetString(Call call, string uri)
		{
			return Task.Run(() => GetStringAsync(call, uri)).GetAwaiter().GetResult();
		}

		public static async Task<string> GetStringAsync(Call call, string uri)
		{
			var response = await GetBytesAsync(call, uri);
			byte[] bytes = response?.Bytes;
			if (bytes != null)
				return Encoding.ASCII.GetString(bytes);

			return null;
		}

		public static ViewHttpResponse GetBytes(Call call, string uri)
		{
			return Task.Run(() => GetBytesAsync(call, uri)).GetAwaiter().GetResult();
		}

		public static async Task<ViewHttpResponse> GetBytesAsync(Call call, string uri)
		{
			using CallTimer getCall = call.Timer("Get Uri", new Tag("Uri", uri));

			for (int attempt = 1; attempt <= MaxAttempts; attempt++)
			{
				if (attempt > 1)
					System.Threading.Thread.Sleep(SleepMilliseconds * (int)Math.Pow(2, attempt));

				try
				{
					Stopwatch stopwatch = Stopwatch.StartNew();
					HttpResponseMessage response = await Client.GetAsync(uri);

					byte[] bytes = await response.Content.ReadAsByteArrayAsync();

					stopwatch.Stop();

					var viewResponse = new ViewHttpResponse()
					{
						Uri = uri,
						Filename = response.RequestMessage.RequestUri.Segments.Last(),
						Milliseconds = stopwatch.ElapsedMilliseconds,
						Bytes = bytes,
						Response = response,
					};

					//response.Close(); // We want the Headers still (might need to copy them elsewhere if this causes problems)
					call.Log.Add("Uri Response", 
						new Tag("Uri", response.RequestMessage.RequestUri), 
						new Tag("Size", bytes.Length));

					return viewResponse;
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

		public static HttpResponseMessage GetHead(Call call, string uri)
		{
			return Task.Run(() => GetHeadAsync(call, uri)).GetAwaiter().GetResult();
		}

		public static async Task<HttpResponseMessage> GetHeadAsync(Call call, string uri)
		{
			using CallTimer headCall = call.Timer("Head Uri", new Tag("Uri", uri));

			for (int attempt = 1; attempt <= MaxAttempts; attempt++)
			{
				if (attempt > 1)
					System.Threading.Thread.Sleep(SleepMilliseconds * (int)Math.Pow(2, attempt));

				HttpRequestMessage request = new(HttpMethod.Head, uri);

				try
				{
					HttpResponseMessage response = await Client.SendAsync(request);

					//response.Close();
					call.Log.Add("Uri Response", 
						new Tag("Uri", request.RequestUri), 
						new Tag("Response", response));

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

	public class ViewHttpResponse
	{
		[HiddenColumn]
		public string Uri { get; set; }
		public string Filename { get; set; }

		[HiddenColumn]
		public string Body => Encoding.ASCII.GetString(Bytes);

		public HttpStatusCode? Status => Response?.StatusCode;

		[HiddenRow]
		public byte[] Bytes { get; set; }
		public double Milliseconds { get; set; }

		[HiddenColumn, HideNull]
		public object View { get; set; }

		[HiddenColumn]
		public HttpResponseMessage Response { get; set; }

		public override string ToString() => Filename;

		public ViewHttpResponse() { }

		public ViewHttpResponse(HttpResponseMessage response, byte[] bytes)
		{
			Response = response;
			Bytes = bytes;
		}
	}
}
