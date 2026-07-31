using System;
using System.Diagnostics;
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

	public Task<Post?> GetPostByIdAsync(int id) => GetPostByIdAsync(id.ToString());

	public async Task<Post?> GetPostByIdAsync(string id)
	{
		PostModel? model = await _client.GetJson<PostModel?>($"/api/v1/{_model.Service}/user/{_model.Id}/post/{id}");

		if (model is null)
		{
			return null;
		}

		return new Post(_client, model);
	}

	public async Task<Creator[]> GetLinkedAccounts()
	{
		CreatorModel[] models = await _client.GetJson<CreatorModel[]>($"/api/v1/{_model.Service}/user/{_model.Id}/links");

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