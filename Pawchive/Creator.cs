using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Pawchive.Models;

namespace Pawchive;

public class Creator
{
	private readonly PawchiveClient _client;
	private readonly CreatorModel _model;

	internal Creator(PawchiveClient client, CreatorModel model)
	{
		_client = client;
		_model = model;
	}

	public string Id => _model.Id;
	public string Name => _model.Name;
	public Service Service => _model.Service;
	public int Followers => _model.Favorited;

	/// <summary>When the creator was first indexed.</summary>
	public DateTime Indexed => _model.Indexed;

	/// <summary>Last update.</summary>
	public DateTime Updated => _model.Updated;

	public string? PublicId => _model.PublicId;

	public async Task<Post?> GetPostByIdAsync(string id, CancellationToken cancellationToken = default)
	{
		PostModel? model = await _client.GetJson<PostModel?>($"/api/v1/{_model.Service}/user/{_model.Id}/post/{id}", cancellationToken);

		if (model is null)
		{
			return null;
		}

		return new Post(_client, model);
	}

	public async Task<Post[]> GetPostsAsync(int page = 0, string? query = null, CancellationToken cancellationToken = default)
	{
		return await _client.GetCreatorPostsAsync(_model.Service, _model.Id, page, query, cancellationToken);
	}

	/// <summary>Lazily walks every page of this creator's posts.</summary>
	public IAsyncEnumerable<Post> EnumeratePostsAsync(int startPage = 0, string? query = null, CancellationToken cancellationToken = default)
	{
		return _client.EnumerateCreatorPostsAsync(_model.Service, _model.Id, startPage, query, cancellationToken);
	}

	public async Task<Announcement[]> GetAnnouncementsAsync(CancellationToken cancellationToken = default)
	{
		AnnouncementModel[] models = await _client.GetJson<AnnouncementModel[]>($"/api/v1/{_model.Service}/user/{_model.Id}/announcements", cancellationToken);

		if (models.Length == 0)
		{
			return [];
		}

		Announcement[] result = new Announcement[models.Length];
		for (int i = 0; i < models.Length; i++)
		{
			result[i] = new Announcement(_client, models[i]);
		}

		return result;
	}

	public async Task<Fancard[]> GetFancardsAsync(CancellationToken cancellationToken = default)
	{
		FancardModel[] models = await _client.GetJson<FancardModel[]>($"/api/v1/{_model.Service}/user/{_model.Id}/fancards", cancellationToken);

		if (models.Length == 0)
		{
			return [];
		}

		Fancard[] result = new Fancard[models.Length];
		for (int i = 0; i < models.Length; i++)
		{
			result[i] = new Fancard(_client, models[i]);
		}

		return result;
	}

	public async Task<Creator[]> GetLinkedAccountsAsync(CancellationToken cancellationToken = default)
	{
		CreatorModel[] models = await _client.GetJson<CreatorModel[]>($"/api/v1/{_model.Service}/user/{_model.Id}/links", cancellationToken);

		if (models.Length == 0) // technically, server never returns 404, this very unlikely that user can be removed
		{
			return [];
		}

		Creator[] result = new Creator[models.Length];
		for (int i = 0; i < models.Length; i++)
		{
			result[i] = new Creator(_client, models[i]);
		}

		return result;
	}

	public async Task FollowAsync(CancellationToken cancellationToken = default)
	{
		await _client.AddFavoriteCreatorAsync(_model.Service, _model.Id, cancellationToken);
	}

	public async Task UnfollowAsync(CancellationToken cancellationToken = default)
	{
		await _client.RemoveFavoriteCreatorAsync(_model.Service, _model.Id, cancellationToken);
	}
}