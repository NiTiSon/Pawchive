using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Pawchive.Models;

namespace Pawchive;

public sealed class PawchiveClient : IDisposable
{
	private const string PawchiveUrl = "https://pawchive.pw";
	internal const string PawchiveDataUrl = "https://file.pawchive.pw";
	internal const string DataUrlPrefix = "/data/";

	private readonly HttpClient _http;

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

	public void Dispose()
	{
		_http.Dispose();
	}

	public void Auth(string token)
	{
		ArgumentNullException.ThrowIfNull(token);

		_http.DefaultRequestHeaders.Add("Cookie", $"session={token}");
	}

	public void Logout()
	{
		_http.DefaultRequestHeaders.Remove("Cookie");
	}

	public async Task<string> GetVersionAsync(CancellationToken cancellationToken = default)
	{
		var res = await _http.GetAsync("/api/v1/app_version", cancellationToken);
		res.EnsureSuccessStatusCode();

		return await res.Content.ReadAsStringAsync(cancellationToken);
	}

	/// <summary>
	/// Gets all creators.
	/// </summary>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>A huge array of all ever existed creators.</returns>
	public async Task<Creator[]> GetCreatorsAsync(CancellationToken cancellationToken = default)
	{
		CreatorModel[] models = await GetJson<CreatorModel[]>($"/api/v1/creators", cancellationToken);

		Creator[] creators = new Creator[models.Length];
		for (int i = 0; i < models.Length; i++)
		{
			creators[i] = new Creator(this, models[i]);
		}

		return creators;
	}

	public async Task<Creator?> GetCreatorByIdAsync(Service service, string id, CancellationToken cancellationToken = default)
	{
		CreatorModel? model = await GetJson<CreatorModel?>($"/api/v1/{service}/user/{id}/profile", cancellationToken);

		if (model is null)
		{
			return null;
		}

		return new Creator(this, model);
	}

	public async Task<Post?> GetPostByIdAsync(Service service, string creatorId, string postId)
	{
		PostModel? model = await GetJson<PostModel?>($"/api/v1/{service}/user/{creatorId}/post/{postId}");

		if (model is null)
		{
			return null;
		}

		return new Post(this, model);
	}

	public async Task<Post[]> GetRecentPosts(int page = 0)
	{
		if (page < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(page));
		}

		PostModel[] models = await GetJson<PostModel[]>($"/api/v1/posts?o={page * 50}");

		if (models.Length == 0)
		{
			return [];
		}

		Post[] result = new Post[models.Length];
		for (int i = 0; i < models.Length; i++)
		{
			result[i] = new Post(this, models[i]);
		}
		return result;
	}

	public async Task<Post[]> SearchPosts(string query, int page = 0)
	{
		if (page < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(page));
		}

		PostModel[] models = await GetJson<PostModel[]>($"/api/v1/posts?q={query}&o={page * 50}");

		if (models.Length == 0)
		{
			return [];
		}

		Post[] result = new Post[models.Length];
		for (int i = 0; i < models.Length; i++)
		{
			result[i] = new Post(this, models[i]);
		}
		return result;
	}

	[UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026:RequiresUnreferencedCode", Justification = nameof(PawchiveJsonSerializationContext))]
	[UnconditionalSuppressMessage("ReflectionAnalysis", "IL3050:RequiresDynamicCodeAttribute", Justification = nameof(PawchiveJsonSerializationContext))]
	internal async Task<T> GetJson<T>(string path, CancellationToken cancellationToken = default)
	{
		HttpResponseMessage res = await _http.GetAsync(path, cancellationToken);

		res.EnsureSuccessStatusCode();
		return JsonSerializer.Deserialize<T>(await res.Content.ReadAsStringAsync(cancellationToken), PawchiveJsonSerializationContext.Default.Options)!;
	}

	[UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026:RequiresUnreferencedCode", Justification = nameof(PawchiveJsonSerializationContext))]
	[UnconditionalSuppressMessage("ReflectionAnalysis", "IL3050:RequiresDynamicCodeAttribute", Justification = nameof(PawchiveJsonSerializationContext))]
	internal async Task<T> PostJson<T>(string path, CancellationToken cancellationToken = default)
	{
		HttpResponseMessage res = await _http.PostAsync(path, null, cancellationToken);

		res.EnsureSuccessStatusCode();
		return JsonSerializer.Deserialize<T>(await res.Content.ReadAsStringAsync(cancellationToken), PawchiveJsonSerializationContext.Default.Options)!;
	}

	[UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026:RequiresUnreferencedCode", Justification = nameof(PawchiveJsonSerializationContext))]
	[UnconditionalSuppressMessage("ReflectionAnalysis", "IL3050:RequiresDynamicCodeAttribute", Justification = nameof(PawchiveJsonSerializationContext))]
	internal async Task<T> DeleteJson<T>(string path, CancellationToken cancellationToken = default)
	{
		HttpResponseMessage res = await _http.DeleteAsync(path, cancellationToken);

		res.EnsureSuccessStatusCode();
		return JsonSerializer.Deserialize<T>(await res.Content.ReadAsStringAsync(cancellationToken), PawchiveJsonSerializationContext.Default.Options)!;
	}
}