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

	public async Task<Post?> GetPostByIdAsync(string id, CancellationToken cancellationToken = default)
	{
		PostModel? model = await _client.GetJson<PostModel?>($"/api/v1/{_model.Service}/user/{_model.Id}/post/{id}", cancellationToken);

		if (model is null)
		{
			return null;
		}

		return new Post(_client, model);
	}

	public async Task<Announcement[]> GetAnnouncements(CancellationToken cancellationToken = default)
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

	public async Task<Fancard[]> GetFancards(CancellationToken cancellationToken = default)
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

	public async Task<Creator[]> GetLinkedAccounts(CancellationToken cancellationToken = default)
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
}