using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Pawchive.Models;

namespace Pawchive;

public sealed class PawchiveClient
{
	private const string PawchiveUrl = "https://pawchive.pw";

	private readonly HttpClient _http;
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string? _sessionCookie;

	public PawchiveClient(HttpClient http)
	{
		_http = http;
	}

	public PawchiveClient(HttpClientHandler handler, bool disposeHandler = false)
		: this(new HttpClient(handler, disposeHandler) { BaseAddress = new Uri(PawchiveUrl) })
	{
	}

	public PawchiveClient()
		: this(new HttpClient { BaseAddress = new Uri(PawchiveUrl) })
	{
	}

	public void SetSessionCookie(string? cookie) => _sessionCookie = cookie;

	public Task<string> Version => GetVersionAsync();

	private async Task<string> GetVersionAsync()
	{
		var res = await _http.GetAsync("/api/v1/app_version");
		res.EnsureSuccessStatusCode();

		return await res.Content.ReadAsStringAsync();
	}

	[UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026:RequiresUnreferencedCode", Justification = nameof(PawchiveJsonSerializationContext))]
	[UnconditionalSuppressMessage("ReflectionAnalysis", "IL3050:RequiresDynamicCodeAttribute", Justification = nameof(PawchiveJsonSerializationContext))]
	internal async Task<T> GetJson<T>(string path, bool auth = false)
	{
		HttpResponseMessage res;
		if (auth)
		{
			HttpRequestMessage req = new(HttpMethod.Get, path);
			if (_sessionCookie is not null)
			{
				req.Headers.Add("Cookie", _sessionCookie);
			}

			res = await _http.SendAsync(req);
		}
		else
		{
			res = await _http.GetAsync(path);
		}

		res.EnsureSuccessStatusCode();
		return JsonSerializer.Deserialize<T>(await res.Content.ReadAsStringAsync(), PawchiveJsonSerializationContext.Default.Options)!;
	}
}