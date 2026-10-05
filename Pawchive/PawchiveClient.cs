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

		return (await res.Content.ReadAsStringAsync(cancellationToken)).Trim();
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

	public async Task<Post?> GetPostByIdAsync(Service service, string creatorId, string postId, CancellationToken cancellationToken = default)
	{
		PostModel? model = await GetJson<PostModel?>($"/api/v1/{service}/user/{creatorId}/post/{postId}", cancellationToken);

		if (model is null)
		{
			return null;
		}

		return new Post(this, model);
	}

	/// <summary>Gets a single page of posts for a creator.</summary>
	/// <param name="service">Service the creator belongs to.</param>
	/// <param name="creatorId">Creator id.</param>
	/// <param name="page">Zero-based page index, 50 posts per page.</param>
	/// <param name="query">Optional search term.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	public async Task<Post[]> GetCreatorPostsAsync(Service service, string creatorId, int page = 0, string? query = null, CancellationToken cancellationToken = default)
	{
		if (page < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(page));
		}

		PostModel[] models = await GetJson<PostModel[]>(BuildPagedPath($"/api/v1/{service}/user/{creatorId}", page, query), cancellationToken);

		return WrapPosts(models);
	}

	public async Task<Post[]> GetRecentPosts(int page = 0, CancellationToken cancellationToken = default)
	{
		if (page < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(page));
		}

		PostModel[] models = await GetJson<PostModel[]>(BuildPagedPath("/api/v1/posts", page, null), cancellationToken);

		return WrapPosts(models);
	}

	public async Task<Post[]> SearchPosts(string query, int page = 0, CancellationToken cancellationToken = default)
	{
		if (page < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(page));
		}

		PostModel[] models = await GetJson<PostModel[]>(BuildPagedPath("/api/v1/posts", page, query), cancellationToken);

		return WrapPosts(models);
	}

	/// <summary>Gets every stored revision of a post.</summary>
	/// <param name="service">Service the post belongs to.</param>
	/// <param name="creatorId">Creator id.</param>
	/// <param name="postId">Post id.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	public async Task<PostRevision[]> GetPostRevisionsAsync(Service service, string creatorId, string postId, CancellationToken cancellationToken = default)
	{
		PostRevisionModel[] models = await GetJson<PostRevisionModel[]>($"/api/v1/{service}/user/{creatorId}/post/{postId}/revisions", cancellationToken);

		if (models.Length == 0)
		{
			return [];
		}

		PostRevision[] result = new PostRevision[models.Length];
		for (int i = 0; i < models.Length; i++)
		{
			result[i] = new PostRevision(models[i]);
		}

		return result;
	}

	/// <summary>Gets the comment thread on a post.</summary>
	/// <param name="service">Service the post belongs to.</param>
	/// <param name="creatorId">Creator id.</param>
	/// <param name="postId">Post id.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	public async Task<Comment[]> GetPostCommentsAsync(Service service, string creatorId, string postId, CancellationToken cancellationToken = default)
	{
		CommentModel[] models = await GetJson<CommentModel[]>($"/api/v1/{service}/user/{creatorId}/post/{postId}/comments", cancellationToken);

		if (models.Length == 0)
		{
			return [];
		}

		Comment[] result = new Comment[models.Length];
		for (int i = 0; i < models.Length; i++)
		{
			result[i] = new Comment(models[i]);
		}

		return result;
	}

	/// <summary>Looks up a file and every post it appears in, by content hash.</summary>
	/// <param name="fileHash">File hash, without an extension.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	public async Task<FileHashResult> SearchFileByHashAsync(string fileHash, CancellationToken cancellationToken = default)
	{
		FileHashResultModel model = await GetJson<FileHashResultModel>($"/api/v1/search_hash/{fileHash}", cancellationToken);

		return new FileHashResult(model);
	}

	/// <summary>Lists the authenticated user's favorites.</summary>
	/// <param name="type">Either <c>artist</c> or <c>post</c>.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	public async Task<Favorite[]> GetFavoritesAsync(string type, CancellationToken cancellationToken = default)
	{
		FavoriteCreatorModel[] models = await GetJson<FavoriteCreatorModel[]>($"/api/v1/account/favorites?type={type}", cancellationToken);

		if (models.Length == 0)
		{
			return [];
		}

		Favorite[] result = new Favorite[models.Length];
		for (int i = 0; i < models.Length; i++)
		{
			result[i] = new Favorite(models[i]);
		}

		return result;
	}

	/// <summary>Adds a post to the authenticated user's favorites.</summary>
	public Task AddFavoritePostAsync(Service service, string creatorId, string postId, CancellationToken cancellationToken = default)
	{
		return SendAsync(HttpMethod.Post, $"/api/v1/favorites/post/{service}/{creatorId}/{postId}", cancellationToken);
	}

	/// <summary>Removes a post from the authenticated user's favorites.</summary>
	public Task RemoveFavoritePostAsync(Service service, string creatorId, string postId, CancellationToken cancellationToken = default)
	{
		return SendAsync(HttpMethod.Delete, $"/api/v1/favorites/post/{service}/{creatorId}/{postId}", cancellationToken);
	}

	/// <summary>Follows a creator.</summary>
	public Task AddFavoriteCreatorAsync(Service service, string creatorId, CancellationToken cancellationToken = default)
	{
		return SendAsync(HttpMethod.Post, $"/api/v1/favorites/creator/{service}/{creatorId}", cancellationToken);
	}

	/// <summary>Unfollows a creator.</summary>
	public Task RemoveFavoriteCreatorAsync(Service service, string creatorId, CancellationToken cancellationToken = default)
	{
		return SendAsync(HttpMethod.Delete, $"/api/v1/favorites/creator/{service}/{creatorId}", cancellationToken);
	}

	/// <summary>Reports a post. Requires a session.</summary>
	public Task FlagPostAsync(Service service, string creatorId, string postId, CancellationToken cancellationToken = default)
	{
		return SendAsync(HttpMethod.Post, $"/api/v1/{service}/user/{creatorId}/post/{postId}/flag", cancellationToken);
	}

	/// <summary>Checks whether the authenticated user has flagged a post.</summary>
	public async Task<bool> CheckPostFlagAsync(Service service, string creatorId, string postId, CancellationToken cancellationToken = default)
	{
		try
		{
			using HttpResponseMessage res = await _http.GetAsync($"/api/v1/{service}/user/{creatorId}/post/{postId}/flag", cancellationToken);

			return res.IsSuccessStatusCode;
		}
		catch (HttpRequestException)
		{
			return false;
		}
	}

	private static string BuildPagedPath(string path, int page, string? query)
	{
		string search = query is null ? "" : $"&q={Uri.EscapeDataString(query)}";

		return $"{path}?o={page * 50}{search}";
	}

	private Post[] WrapPosts(PostModel[] models)
	{
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

	private async Task SendAsync(HttpMethod method, string path, CancellationToken cancellationToken)
	{
		using HttpRequestMessage request = new(method, path);

		HttpResponseMessage res = await _http.SendAsync(request, cancellationToken);

		res.EnsureSuccessStatusCode();
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